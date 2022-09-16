using System;
using System.Collections;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.Zip;

public class ZipOutputStream : DeflaterOutputStream
{
	private ArrayList entries = new ArrayList();

	private Crc32 crc = new Crc32();

	private ZipEntry curEntry = null;

	private int defaultCompressionLevel = Deflater.DEFAULT_COMPRESSION;

	private CompressionMethod curMethod = CompressionMethod.Deflated;

	private long size;

	private long offset = 0L;

	private byte[] zipComment = new byte[0];

	private bool patchEntryHeader = false;

	private long headerPatchPos = -1L;

	public bool IsFinished => entries == null;

	public ZipOutputStream(Stream baseOutputStream)
		: base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, noZlibHeaderOrFooter: true))
	{
	}

	public void SetComment(string comment)
	{
		byte[] array = ZipConstants.ConvertToArray(comment);
		if (array.Length > 65535)
		{
			throw new ArgumentOutOfRangeException("comment");
		}
		zipComment = array;
	}

	public void SetLevel(int level)
	{
		defaultCompressionLevel = level;
		def.SetLevel(level);
	}

	public int GetLevel()
	{
		return def.GetLevel();
	}

	private void WriteLeShort(int value)
	{
		baseOutputStream.WriteByte((byte)((uint)value & 0xFFu));
		baseOutputStream.WriteByte((byte)((uint)(value >> 8) & 0xFFu));
	}

	private void WriteLeInt(int value)
	{
		WriteLeShort(value);
		WriteLeShort(value >> 16);
	}

	private void WriteLeLong(long value)
	{
		WriteLeInt((int)value);
		WriteLeInt((int)(value >> 32));
	}

	public void PutNextEntry(ZipEntry entry)
	{
		if (entries == null)
		{
			throw new InvalidOperationException("ZipOutputStream was finished");
		}
		if (curEntry != null)
		{
			CloseEntry();
		}
		if (entries.Count >= 65535)
		{
			throw new ZipException("Too many entries for Zip file");
		}
		CompressionMethod compressionMethod = entry.CompressionMethod;
		int level = defaultCompressionLevel;
		entry.Flags = 0;
		patchEntryHeader = false;
		bool flag = true;
		if (compressionMethod == CompressionMethod.Stored)
		{
			if (entry.CompressedSize >= 0)
			{
				if (entry.Size < 0)
				{
					entry.Size = entry.CompressedSize;
				}
				else if (entry.Size != entry.CompressedSize)
				{
					throw new ZipException("Method STORED, but compressed size != size");
				}
			}
			else if (entry.Size >= 0)
			{
				entry.CompressedSize = entry.Size;
			}
			if (entry.Size < 0 || entry.Crc < 0)
			{
				if (base.CanPatchEntries)
				{
					flag = false;
				}
				else
				{
					compressionMethod = CompressionMethod.Deflated;
					level = 0;
				}
			}
		}
		if (compressionMethod == CompressionMethod.Deflated)
		{
			if (entry.Size == 0)
			{
				entry.CompressedSize = entry.Size;
				entry.Crc = 0L;
				compressionMethod = CompressionMethod.Stored;
			}
			else if (entry.CompressedSize < 0 || entry.Size < 0 || entry.Crc < 0)
			{
				flag = false;
			}
		}
		if (!flag)
		{
			if (!base.CanPatchEntries)
			{
				entry.Flags |= 8;
			}
			else
			{
				patchEntryHeader = true;
			}
		}
		if (base.Password != null)
		{
			entry.IsCrypted = true;
			if (entry.Crc < 0)
			{
				entry.Flags |= 8;
			}
		}
		entry.Offset = (int)offset;
		entry.CompressionMethod = compressionMethod;
		curMethod = compressionMethod;
		WriteLeInt(67324752);
		WriteLeShort(entry.Version);
		WriteLeShort(entry.Flags);
		WriteLeShort((byte)compressionMethod);
		WriteLeInt((int)entry.DosTime);
		if (flag)
		{
			WriteLeInt((int)entry.Crc);
			WriteLeInt((int)(entry.IsCrypted ? ((int)entry.CompressedSize + 12) : entry.CompressedSize));
			WriteLeInt((int)entry.Size);
		}
		else
		{
			if (patchEntryHeader)
			{
				headerPatchPos = baseOutputStream.Position;
			}
			WriteLeInt(0);
			WriteLeInt(0);
			WriteLeInt(0);
		}
		byte[] array = ZipConstants.ConvertToArray(entry.Name);
		if (array.Length > 65535)
		{
			throw new ZipException("Entry name too long.");
		}
		byte[] array2 = entry.ExtraData;
		if (array2 == null)
		{
			array2 = new byte[0];
		}
		if (array2.Length > 65535)
		{
			throw new ZipException("Extra data too long.");
		}
		WriteLeShort(array.Length);
		WriteLeShort(array2.Length);
		baseOutputStream.Write(array, 0, array.Length);
		baseOutputStream.Write(array2, 0, array2.Length);
		offset += 30 + array.Length + array2.Length;
		curEntry = entry;
		crc.Reset();
		if (compressionMethod == CompressionMethod.Deflated)
		{
			def.Reset();
			def.SetLevel(level);
		}
		size = 0L;
		if (entry.IsCrypted)
		{
			if (entry.Crc < 0)
			{
				WriteEncryptionHeader(entry.DosTime << 16);
			}
			else
			{
				WriteEncryptionHeader(entry.Crc);
			}
		}
	}

	public void CloseEntry()
	{
		if (curEntry == null)
		{
			throw new InvalidOperationException("No open entry");
		}
		if (curMethod == CompressionMethod.Deflated)
		{
			base.Finish();
		}
		long num = ((curMethod == CompressionMethod.Deflated) ? def.TotalOut : size);
		if (curEntry.Size < 0)
		{
			curEntry.Size = size;
		}
		else if (curEntry.Size != size)
		{
			throw new ZipException("size was " + size + ", but I expected " + curEntry.Size);
		}
		if (curEntry.CompressedSize < 0)
		{
			curEntry.CompressedSize = num;
		}
		else if (curEntry.CompressedSize != num)
		{
			throw new ZipException("compressed size was " + num + ", but I expected " + curEntry.CompressedSize);
		}
		if (curEntry.Crc < 0)
		{
			curEntry.Crc = crc.Value;
		}
		else if (curEntry.Crc != crc.Value)
		{
			throw new ZipException("crc was " + crc.Value + ", but I expected " + curEntry.Crc);
		}
		offset += num;
		if (offset > uint.MaxValue)
		{
			throw new ZipException("Maximum Zip file size exceeded");
		}
		if (curEntry.IsCrypted)
		{
			curEntry.CompressedSize += 12L;
		}
		if (patchEntryHeader)
		{
			long position = baseOutputStream.Position;
			baseOutputStream.Seek(headerPatchPos, SeekOrigin.Begin);
			WriteLeInt((int)curEntry.Crc);
			WriteLeInt((int)curEntry.CompressedSize);
			WriteLeInt((int)curEntry.Size);
			baseOutputStream.Seek(position, SeekOrigin.Begin);
			patchEntryHeader = false;
		}
		if (((uint)curEntry.Flags & 8u) != 0)
		{
			WriteLeInt(134695760);
			WriteLeInt((int)curEntry.Crc);
			WriteLeInt((int)curEntry.CompressedSize);
			WriteLeInt((int)curEntry.Size);
			offset += 16L;
		}
		entries.Add(curEntry);
		curEntry = null;
	}

	private void WriteEncryptionHeader(long crcValue)
	{
		offset += 12L;
		InitializePassword(base.Password);
		byte[] array = new byte[12];
		Random random = new Random();
		random.NextBytes(array);
		array[11] = (byte)(crcValue >> 24);
		EncryptBlock(array, 0, array.Length);
		baseOutputStream.Write(array, 0, array.Length);
	}

	public override void Write(byte[] b, int off, int len)
	{
		if (curEntry == null)
		{
			throw new InvalidOperationException("No open entry.");
		}
		if (len <= 0)
		{
			return;
		}
		crc.Update(b, off, len);
		size += len;
		if (size > uint.MaxValue || size < 0)
		{
			throw new ZipException("Maximum entry size exceeded");
		}
		switch (curMethod)
		{
		case CompressionMethod.Deflated:
			base.Write(b, off, len);
			break;
		case CompressionMethod.Stored:
			if (base.Password != null)
			{
				byte[] array = new byte[len];
				Array.Copy(b, off, array, 0, len);
				EncryptBlock(array, 0, len);
				baseOutputStream.Write(array, off, len);
			}
			else
			{
				baseOutputStream.Write(b, off, len);
			}
			break;
		}
	}

	public override void Finish()
	{
		if (entries == null)
		{
			return;
		}
		if (curEntry != null)
		{
			CloseEntry();
		}
		int num = 0;
		int num2 = 0;
		foreach (ZipEntry entry in entries)
		{
			CompressionMethod compressionMethod = entry.CompressionMethod;
			WriteLeInt(33639248);
			WriteLeShort(20);
			WriteLeShort(entry.Version);
			WriteLeShort(entry.Flags);
			WriteLeShort((short)compressionMethod);
			WriteLeInt((int)entry.DosTime);
			WriteLeInt((int)entry.Crc);
			WriteLeInt((int)entry.CompressedSize);
			WriteLeInt((int)entry.Size);
			byte[] array = ZipConstants.ConvertToArray(entry.Name);
			if (array.Length > 65535)
			{
				throw new ZipException("Name too long.");
			}
			byte[] array2 = entry.ExtraData;
			if (array2 == null)
			{
				array2 = new byte[0];
			}
			byte[] array3 = ((entry.Comment != null) ? ZipConstants.ConvertToArray(entry.Comment) : new byte[0]);
			if (array3.Length > 65535)
			{
				throw new ZipException("Comment too long.");
			}
			WriteLeShort(array.Length);
			WriteLeShort(array2.Length);
			WriteLeShort(array3.Length);
			WriteLeShort(0);
			WriteLeShort(0);
			if (entry.ExternalFileAttributes != -1)
			{
				WriteLeInt(entry.ExternalFileAttributes);
			}
			else if (entry.IsDirectory)
			{
				WriteLeInt(16);
			}
			else
			{
				WriteLeInt(0);
			}
			WriteLeInt(entry.Offset);
			baseOutputStream.Write(array, 0, array.Length);
			baseOutputStream.Write(array2, 0, array2.Length);
			baseOutputStream.Write(array3, 0, array3.Length);
			num++;
			num2 += 46 + array.Length + array2.Length + array3.Length;
		}
		WriteLeInt(101010256);
		WriteLeShort(0);
		WriteLeShort(0);
		WriteLeShort(num);
		WriteLeShort(num);
		WriteLeInt(num2);
		WriteLeInt((int)offset);
		WriteLeShort(zipComment.Length);
		baseOutputStream.Write(zipComment, 0, zipComment.Length);
		baseOutputStream.Flush();
		entries = null;
	}
}
