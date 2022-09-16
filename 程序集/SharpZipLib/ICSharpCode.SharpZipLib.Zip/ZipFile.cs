using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Encryption;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.Zip;

public class ZipFile : IEnumerable
{
	public delegate void KeysRequiredEventHandler(object sender, KeysRequiredEventArgs e);

	private class ZipEntryEnumeration : IEnumerator
	{
		private ZipEntry[] array;

		private int ptr = -1;

		public object Current => array[ptr];

		public ZipEntryEnumeration(ZipEntry[] arr)
		{
			array = arr;
		}

		public void Reset()
		{
			ptr = -1;
		}

		public bool MoveNext()
		{
			return ++ptr < array.Length;
		}
	}

	private class PartialInputStream : InflaterInputStream
	{
		private Stream baseStream;

		private long filepos;

		private long end;

		public override int Available
		{
			get
			{
				long num = end - filepos;
				if (num > int.MaxValue)
				{
					return int.MaxValue;
				}
				return (int)num;
			}
		}

		public PartialInputStream(Stream baseStream, long start, long len)
			: base(baseStream)
		{
			this.baseStream = baseStream;
			filepos = start;
			end = start + len;
		}

		public override int ReadByte()
		{
			if (filepos == end)
			{
				return -1;
			}
			lock (baseStream)
			{
				baseStream.Seek(filepos++, SeekOrigin.Begin);
				return baseStream.ReadByte();
			}
		}

		public override void Close()
		{
		}

		public override int Read(byte[] b, int off, int len)
		{
			if (len > end - filepos)
			{
				len = (int)(end - filepos);
				if (len == 0)
				{
					return 0;
				}
			}
			lock (baseStream)
			{
				baseStream.Seek(filepos, SeekOrigin.Begin);
				int num = baseStream.Read(b, off, len);
				if (num > 0)
				{
					filepos += len;
				}
				return num;
			}
		}

		public long SkipBytes(long amount)
		{
			if (amount < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (amount > end - filepos)
			{
				amount = end - filepos;
			}
			filepos += amount;
			return amount;
		}
	}

	private string name;

	private string comment;

	private Stream baseStream;

	private bool isStreamOwner = true;

	private long offsetOfFirstEntry = 0L;

	private ZipEntry[] entries;

	public KeysRequiredEventHandler KeysRequired;

	private byte[] key = null;

	private byte[] iv = null;

	private byte[] Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
		}
	}

	public string Password
	{
		set
		{
			if (value == null || value.Length == 0)
			{
				key = null;
			}
			else
			{
				key = PkzipClassic.GenerateKeys(Encoding.ASCII.GetBytes(value));
			}
		}
	}

	private bool HaveKeys => key != null;

	private bool IsStreamOwner
	{
		get
		{
			return isStreamOwner;
		}
		set
		{
			isStreamOwner = value;
		}
	}

	[IndexerName("EntryByIndex")]
	public ZipEntry this[int index] => (ZipEntry)entries[index].Clone();

	public string ZipFileComment => comment;

	public string Name => name;

	public int Size
	{
		get
		{
			if (entries != null)
			{
				return entries.Length;
			}
			throw new InvalidOperationException("ZipFile is closed");
		}
	}

	private void OnKeysRequired(string fileName)
	{
		if (KeysRequired != null)
		{
			KeysRequiredEventArgs keysRequiredEventArgs = new KeysRequiredEventArgs(fileName, key);
			KeysRequired(this, keysRequiredEventArgs);
			key = keysRequiredEventArgs.Key;
		}
	}

	public ZipFile(string name)
	{
		this.name = name;
		baseStream = File.OpenRead(name);
		try
		{
			ReadEntries();
		}
		catch
		{
			Close();
			throw;
		}
	}

	public ZipFile(FileStream file)
	{
		baseStream = file;
		name = file.Name;
		try
		{
			ReadEntries();
		}
		catch
		{
			Close();
			throw;
		}
	}

	public ZipFile(Stream baseStream)
	{
		this.baseStream = baseStream;
		name = null;
		try
		{
			ReadEntries();
		}
		catch
		{
			Close();
			throw;
		}
	}

	private int ReadLeShort()
	{
		return baseStream.ReadByte() | (baseStream.ReadByte() << 8);
	}

	private int ReadLeInt()
	{
		return ReadLeShort() | (ReadLeShort() << 16);
	}

	private long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
	{
		long num = endLocation - minimumBlockSize;
		if (num < 0)
		{
			return -1L;
		}
		long num2 = Math.Max(num - maximumVariableData, 0L);
		do
		{
			if (num < num2)
			{
				return -1L;
			}
			baseStream.Seek(num--, SeekOrigin.Begin);
		}
		while (ReadLeInt() != signature);
		return baseStream.Position;
	}

	private void ReadEntries()
	{
		if (!baseStream.CanSeek)
		{
			throw new ZipException("ZipFile stream must be seekable");
		}
		long num = LocateBlockWithSignature(101010256, baseStream.Length, 22, 65535);
		if (num < 0)
		{
			throw new ZipException("Cannot find central directory");
		}
		int num2 = ReadLeShort();
		int num3 = ReadLeShort();
		int num4 = ReadLeShort();
		int num5 = ReadLeShort();
		int num6 = ReadLeInt();
		int num7 = ReadLeInt();
		int num8 = ReadLeShort();
		byte[] array = new byte[num8];
		baseStream.Read(array, 0, array.Length);
		comment = ZipConstants.ConvertToString(array);
		entries = new ZipEntry[num5];
		if (num7 < num - (4 + num6))
		{
			offsetOfFirstEntry = num - (4 + num6 + num7);
			if (offsetOfFirstEntry <= 0)
			{
				throw new ZipException("Invalid SFX file");
			}
		}
		baseStream.Seek(offsetOfFirstEntry + num7, SeekOrigin.Begin);
		for (int i = 0; i < num4; i++)
		{
			if (ReadLeInt() != 33639248)
			{
				throw new ZipException("Wrong Central Directory signature");
			}
			int madeByInfo = ReadLeShort();
			int versionRequiredToExtract = ReadLeShort();
			int flags = ReadLeShort();
			int compressionMethod = ReadLeShort();
			int num9 = ReadLeInt();
			int num10 = ReadLeInt();
			int num11 = ReadLeInt();
			int num12 = ReadLeInt();
			int num13 = ReadLeShort();
			int num14 = ReadLeShort();
			int num15 = ReadLeShort();
			int num16 = ReadLeShort();
			int num17 = ReadLeShort();
			int externalFileAttributes = ReadLeInt();
			int offset = ReadLeInt();
			byte[] array2 = new byte[Math.Max(num13, num15)];
			baseStream.Read(array2, 0, num13);
			string text = ZipConstants.ConvertToString(array2, num13);
			ZipEntry zipEntry = new ZipEntry(text, versionRequiredToExtract, madeByInfo);
			zipEntry.CompressionMethod = (CompressionMethod)compressionMethod;
			zipEntry.Crc = num10 & 0xFFFFFFFFu;
			zipEntry.Size = num12 & 0xFFFFFFFFu;
			zipEntry.CompressedSize = num11 & 0xFFFFFFFFu;
			zipEntry.Flags = flags;
			zipEntry.DosTime = (uint)num9;
			if (num14 > 0)
			{
				byte[] array3 = new byte[num14];
				baseStream.Read(array3, 0, num14);
				zipEntry.ExtraData = array3;
			}
			if (num15 > 0)
			{
				baseStream.Read(array2, 0, num15);
				zipEntry.Comment = ZipConstants.ConvertToString(array2, num15);
			}
			zipEntry.ZipFileIndex = i;
			zipEntry.Offset = offset;
			zipEntry.ExternalFileAttributes = externalFileAttributes;
			entries[i] = zipEntry;
		}
	}

	public void Close()
	{
		entries = null;
		if (isStreamOwner)
		{
			lock (baseStream)
			{
				baseStream.Close();
			}
		}
	}

	public IEnumerator GetEnumerator()
	{
		if (entries == null)
		{
			throw new InvalidOperationException("ZipFile has closed");
		}
		return new ZipEntryEnumeration(entries);
	}

	public int FindEntry(string name, bool ignoreCase)
	{
		if (entries == null)
		{
			throw new InvalidOperationException("ZipFile has been closed");
		}
		for (int i = 0; i < entries.Length; i++)
		{
			if (string.Compare(name, entries[i].Name, ignoreCase) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public ZipEntry GetEntry(string name)
	{
		if (entries == null)
		{
			throw new InvalidOperationException("ZipFile has been closed");
		}
		int num = FindEntry(name, ignoreCase: true);
		return (num >= 0) ? ((ZipEntry)entries[num].Clone()) : null;
	}

	public bool TestArchive(bool testData)
	{
		bool result = true;
		try
		{
			for (int i = 0; i < Size; i++)
			{
				long num = TestLocalHeader(this[i], fullTest: true, extractTest: true);
				if (testData)
				{
					Stream inputStream = GetInputStream(this[i]);
					Crc32 crc = new Crc32();
					byte[] array = new byte[4096];
					int len;
					while ((len = inputStream.Read(array, 0, array.Length)) > 0)
					{
						crc.Update(array, 0, len);
					}
					if (this[i].Crc != crc.Value)
					{
						result = false;
						break;
					}
				}
			}
		}
		catch
		{
			result = false;
		}
		return result;
	}

	private long TestLocalHeader(ZipEntry entry, bool fullTest, bool extractTest)
	{
		lock (baseStream)
		{
			baseStream.Seek(offsetOfFirstEntry + entry.Offset, SeekOrigin.Begin);
			if (ReadLeInt() != 67324752)
			{
				throw new ZipException("Wrong local header signature");
			}
			short num = (short)ReadLeShort();
			if (extractTest && num > 20)
			{
				throw new ZipException($"Version required to extract this entry not supported ({num})");
			}
			short num2 = (short)ReadLeShort();
			if (extractTest && ((uint)num2 & 0x3060u) != 0)
			{
				throw new ZipException("The library doesnt support the zip version required to extract this entry");
			}
			if (num2 != entry.Flags)
			{
				throw new ZipException("Central header/local header flags mismatch");
			}
			if (entry.CompressionMethod != (CompressionMethod)ReadLeShort())
			{
				throw new ZipException("Central header/local header compression method mismatch");
			}
			num = (short)ReadLeShort();
			num = (short)ReadLeShort();
			int num3 = ReadLeInt();
			if (fullTest && (num2 & 8) == 0 && num3 != (int)entry.Crc)
			{
				throw new ZipException("Central header/local header crc mismatch");
			}
			num3 = ReadLeInt();
			num3 = ReadLeInt();
			int num4 = ReadLeShort();
			if (entry.Name.Length > num4)
			{
				throw new ZipException("file name length mismatch");
			}
			int num5 = num4 + ReadLeShort();
			return offsetOfFirstEntry + entry.Offset + 30 + num5;
		}
	}

	private long CheckLocalHeader(ZipEntry entry)
	{
		return TestLocalHeader(entry, fullTest: false, extractTest: true);
	}

	private void ReadFully(Stream s, byte[] outBuf)
	{
		int num = 0;
		int num2 = outBuf.Length;
		while (num2 > 0)
		{
			int num3 = s.Read(outBuf, num, num2);
			if (num3 <= 0)
			{
				throw new ZipException("Unexpected EOF");
			}
			num += num3;
			num2 -= num3;
		}
	}

	private void CheckClassicPassword(CryptoStream classicCryptoStream, ZipEntry entry)
	{
		byte[] array = new byte[12];
		ReadFully(classicCryptoStream, array);
		if ((entry.Flags & 8) == 0)
		{
			if (array[11] != (byte)(entry.Crc >> 24))
			{
				throw new ZipException("Invalid password");
			}
		}
		else if (array[11] != (byte)((entry.DosTime >> 8) & 0xFF))
		{
			throw new ZipException("Invalid password");
		}
	}

	private Stream CreateAndInitDecryptionStream(Stream baseStream, ZipEntry entry)
	{
		CryptoStream cryptoStream = null;
		if (entry.Version < 50 || (entry.Flags & 0x40) == 0)
		{
			PkzipClassicManaged pkzipClassicManaged = new PkzipClassicManaged();
			OnKeysRequired(entry.Name);
			if (!HaveKeys)
			{
				throw new ZipException("No password available for encrypted stream");
			}
			cryptoStream = new CryptoStream(baseStream, pkzipClassicManaged.CreateDecryptor(key, iv), CryptoStreamMode.Read);
			CheckClassicPassword(cryptoStream, entry);
			return cryptoStream;
		}
		throw new ZipException("Decryption method not supported");
	}

	private void WriteEncryptionHeader(Stream stream, long crcValue)
	{
		byte[] array = new byte[12];
		Random random = new Random();
		random.NextBytes(array);
		array[11] = (byte)(crcValue >> 24);
		stream.Write(array, 0, array.Length);
	}

	private Stream CreateAndInitEncryptionStream(Stream baseStream, ZipEntry entry)
	{
		CryptoStream cryptoStream = null;
		if (entry.Version < 50 || (entry.Flags & 0x40) == 0)
		{
			PkzipClassicManaged pkzipClassicManaged = new PkzipClassicManaged();
			OnKeysRequired(entry.Name);
			if (!HaveKeys)
			{
				throw new ZipException("No password available for encrypted stream");
			}
			cryptoStream = new CryptoStream(baseStream, pkzipClassicManaged.CreateEncryptor(key, iv), CryptoStreamMode.Write);
			if (entry.Crc < 0 || ((uint)entry.Flags & 8u) != 0)
			{
				WriteEncryptionHeader(cryptoStream, entry.DosTime << 16);
			}
			else
			{
				WriteEncryptionHeader(cryptoStream, entry.Crc);
			}
		}
		return cryptoStream;
	}

	private Stream GetOutputStream(ZipEntry entry, string fileName)
	{
		baseStream.Seek(0L, SeekOrigin.End);
		Stream stream = File.OpenWrite(fileName);
		if (entry.IsCrypted)
		{
			stream = CreateAndInitEncryptionStream(stream, entry);
		}
		switch (entry.CompressionMethod)
		{
		case CompressionMethod.Deflated:
			stream = new DeflaterOutputStream(stream);
			break;
		default:
			throw new ZipException("Unknown compression method " + entry.CompressionMethod);
		case CompressionMethod.Stored:
			break;
		}
		return stream;
	}

	public Stream GetInputStream(ZipEntry entry)
	{
		if (entries == null)
		{
			throw new InvalidOperationException("ZipFile has closed");
		}
		int num = entry.ZipFileIndex;
		if (num < 0 || num >= entries.Length || entries[num].Name != entry.Name)
		{
			num = FindEntry(entry.Name, ignoreCase: true);
			if (num < 0)
			{
				throw new IndexOutOfRangeException();
			}
		}
		return GetInputStream(num);
	}

	public Stream GetInputStream(int entryIndex)
	{
		if (entries == null)
		{
			throw new InvalidOperationException("ZipFile has closed");
		}
		long start = CheckLocalHeader(entries[entryIndex]);
		CompressionMethod compressionMethod = entries[entryIndex].CompressionMethod;
		Stream stream = new PartialInputStream(baseStream, start, entries[entryIndex].CompressedSize);
		if (entries[entryIndex].IsCrypted)
		{
			stream = CreateAndInitDecryptionStream(stream, entries[entryIndex]);
			if (stream == null)
			{
				throw new ZipException("Unable to decrypt this entry");
			}
		}
		return compressionMethod switch
		{
			CompressionMethod.Stored => stream, 
			CompressionMethod.Deflated => new InflaterInputStream(stream, new Inflater(noHeader: true)), 
			_ => throw new ZipException("Unsupported compression method " + compressionMethod), 
		};
	}
}
