using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Encryption;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.Zip;

public class ZipInputStream : InflaterInputStream
{
	private delegate int ReaderDelegate(byte[] b, int offset, int length);

	private ReaderDelegate internalReader;

	private Crc32 crc = new Crc32();

	private ZipEntry entry = null;

	private long size;

	private int method;

	private int flags;

	private string password = null;

	public string Password
	{
		get
		{
			return password;
		}
		set
		{
			password = value;
		}
	}

	public bool CanDecompressEntry => entry != null && entry.Version <= 20;

	public override int Available => (entry != null) ? 1 : 0;

	public ZipInputStream(Stream baseInputStream)
		: base(baseInputStream, new Inflater(noHeader: true))
	{
		internalReader = InitialRead;
	}

	public ZipEntry GetNextEntry()
	{
		if (crc == null)
		{
			throw new InvalidOperationException("Closed.");
		}
		if (entry != null)
		{
			CloseEntry();
		}
		int num = inputBuffer.ReadLeInt();
		if (num == 33639248 || num == 101010256 || num == 84233040 || num == 101075792)
		{
			Close();
			return null;
		}
		if (num == 808471376 || num == 134695760)
		{
			num = inputBuffer.ReadLeInt();
		}
		if (num != 67324752)
		{
			throw new ZipException("Wrong Local header signature: 0x" + $"{num:X}");
		}
		short versionRequiredToExtract = (short)inputBuffer.ReadLeShort();
		flags = inputBuffer.ReadLeShort();
		method = inputBuffer.ReadLeShort();
		uint num2 = (uint)inputBuffer.ReadLeInt();
		int num3 = inputBuffer.ReadLeInt();
		csize = inputBuffer.ReadLeInt();
		size = inputBuffer.ReadLeInt();
		int num4 = inputBuffer.ReadLeShort();
		int num5 = inputBuffer.ReadLeShort();
		bool flag = (flags & 1) == 1;
		byte[] array = new byte[num4];
		inputBuffer.ReadRawBuffer(array);
		string name = ZipConstants.ConvertToString(array);
		entry = new ZipEntry(name, versionRequiredToExtract);
		entry.Flags = flags;
		if (method == 0 && ((!flag && csize != size) || (flag && csize - 12 != size)))
		{
			throw new ZipException("Stored, but compressed != uncompressed");
		}
		if (method != 0 && method != 8)
		{
			throw new ZipException("Unknown compression method " + method);
		}
		entry.CompressionMethod = (CompressionMethod)method;
		if ((flags & 8) == 0)
		{
			entry.Crc = num3 & 0xFFFFFFFFu;
			entry.Size = size & 0xFFFFFFFFu;
			entry.CompressedSize = csize & 0xFFFFFFFFu;
		}
		else
		{
			if (num3 != 0)
			{
				entry.Crc = num3 & 0xFFFFFFFFu;
			}
			if (size != 0)
			{
				entry.Size = size & 0xFFFFFFFFu;
			}
			if (csize != 0)
			{
				entry.CompressedSize = csize & 0xFFFFFFFFu;
			}
		}
		entry.DosTime = num2;
		if (num5 > 0)
		{
			byte[] array2 = new byte[num5];
			inputBuffer.ReadRawBuffer(array2);
			entry.ExtraData = array2;
		}
		internalReader = InitialRead;
		return entry;
	}

	private void ReadDataDescriptor()
	{
		if (inputBuffer.ReadLeInt() != 134695760)
		{
			throw new ZipException("Data descriptor signature not found");
		}
		entry.Crc = inputBuffer.ReadLeInt() & 0xFFFFFFFFu;
		csize = inputBuffer.ReadLeInt();
		size = inputBuffer.ReadLeInt();
		entry.Size = size & 0xFFFFFFFFu;
		entry.CompressedSize = csize & 0xFFFFFFFFu;
	}

	public void CloseEntry()
	{
		if (crc == null)
		{
			throw new InvalidOperationException("Closed.");
		}
		if (entry == null)
		{
			return;
		}
		if (method == 8)
		{
			if (((uint)flags & 8u) != 0)
			{
				byte[] array = new byte[2048];
				while (Read(array, 0, array.Length) > 0)
				{
				}
				return;
			}
			csize -= inf.TotalIn;
			inputBuffer.Available -= inf.RemainingInput;
		}
		if (inputBuffer.Available > csize && csize >= 0)
		{
			inputBuffer.Available = (int)(inputBuffer.Available - csize);
		}
		else
		{
			csize -= inputBuffer.Available;
			inputBuffer.Available = 0;
			while (csize != 0)
			{
				int num = (int)Skip(csize & 0xFFFFFFFFu);
				if (num <= 0)
				{
					throw new ZipException("Zip archive ends early.");
				}
				csize -= num;
			}
		}
		size = 0L;
		crc.Reset();
		if (method == 8)
		{
			inf.Reset();
		}
		entry = null;
	}

	public override int ReadByte()
	{
		byte[] array = new byte[1];
		if (Read(array, 0, 1) <= 0)
		{
			return -1;
		}
		return array[0] & 0xFF;
	}

	private int InitialRead(byte[] destination, int offset, int count)
	{
		if (entry.Version > 20)
		{
			throw new ZipException("Libray cannot extract this entry version required (" + entry.Version + ")");
		}
		if (entry.IsCrypted)
		{
			if (password == null)
			{
				throw new ZipException("No password set.");
			}
			PkzipClassicManaged pkzipClassicManaged = new PkzipClassicManaged();
			byte[] rgbKey = PkzipClassic.GenerateKeys(Encoding.ASCII.GetBytes(password));
			inputBuffer.CryptoTransform = pkzipClassicManaged.CreateDecryptor(rgbKey, null);
			byte[] array = new byte[12];
			inputBuffer.ReadClearTextBuffer(array, 0, 12);
			if ((flags & 8) == 0)
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
			if (csize >= 12)
			{
				csize -= 12L;
			}
		}
		else
		{
			inputBuffer.CryptoTransform = null;
		}
		if (method == 8 && inputBuffer.Available > 0)
		{
			inputBuffer.SetInflaterInput(inf);
		}
		internalReader = BodyRead;
		return BodyRead(destination, offset, count);
	}

	public override int Read(byte[] destination, int index, int count)
	{
		return internalReader(destination, index, count);
	}

	public int BodyRead(byte[] b, int off, int len)
	{
		if (crc == null)
		{
			throw new InvalidOperationException("Closed.");
		}
		if (entry == null || len <= 0)
		{
			return 0;
		}
		bool flag = false;
		switch (method)
		{
		case 8:
			len = base.Read(b, off, len);
			if (len <= 0)
			{
				if (!inf.IsFinished)
				{
					throw new ZipException("Inflater not finished!?");
				}
				inputBuffer.Available = inf.RemainingInput;
				if ((flags & 8) == 0 && (inf.TotalIn != csize || inf.TotalOut != size))
				{
					throw new ZipException("size mismatch: " + csize + ";" + size + " <-> " + inf.TotalIn + ";" + inf.TotalOut);
				}
				inf.Reset();
				flag = true;
			}
			break;
		case 0:
			if (len > csize && csize >= 0)
			{
				len = (int)csize;
			}
			len = inputBuffer.ReadClearTextBuffer(b, off, len);
			if (len > 0)
			{
				csize -= len;
				size -= len;
			}
			if (csize == 0)
			{
				flag = true;
			}
			else if (len < 0)
			{
				throw new ZipException("EOF in stored block");
			}
			break;
		}
		if (len > 0)
		{
			crc.Update(b, off, len);
		}
		if (flag)
		{
			StopDecrypting();
			if (((uint)flags & 8u) != 0)
			{
				ReadDataDescriptor();
			}
			if ((crc.Value & 0xFFFFFFFFu) != entry.Crc && entry.Crc != -1)
			{
				throw new ZipException("CRC mismatch");
			}
			crc.Reset();
			entry = null;
		}
		return len;
	}

	public override void Close()
	{
		base.Close();
		crc = null;
		entry = null;
	}
}
