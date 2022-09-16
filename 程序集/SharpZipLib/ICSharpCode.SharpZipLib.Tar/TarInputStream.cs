using System;
using System.IO;
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar;

public class TarInputStream : Stream
{
	public interface IEntryFactory
	{
		TarEntry CreateEntry(string name);

		TarEntry CreateEntryFromFile(string fileName);

		TarEntry CreateEntry(byte[] headerBuf);
	}

	public class EntryFactoryAdapter : IEntryFactory
	{
		public TarEntry CreateEntry(string name)
		{
			return TarEntry.CreateTarEntry(name);
		}

		public TarEntry CreateEntryFromFile(string fileName)
		{
			return TarEntry.CreateEntryFromFile(fileName);
		}

		public TarEntry CreateEntry(byte[] headerBuf)
		{
			return new TarEntry(headerBuf);
		}
	}

	protected bool hasHitEOF;

	protected long entrySize;

	protected long entryOffset;

	protected byte[] readBuf;

	protected TarBuffer buffer;

	protected TarEntry currEntry;

	protected IEntryFactory eFactory;

	private Stream inputStream;

	public override bool CanRead => inputStream.CanRead;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => inputStream.Length;

	public override long Position
	{
		get
		{
			return inputStream.Position;
		}
		set
		{
			throw new NotSupportedException("TarInputStream Seek not supported");
		}
	}

	public long Available => entrySize - entryOffset;

	public bool IsMarkSupported => false;

	public override void Flush()
	{
		inputStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException("TarInputStream Seek not supported");
	}

	public override void SetLength(long val)
	{
		throw new NotSupportedException("TarInputStream SetLength not supported");
	}

	public override void Write(byte[] array, int offset, int count)
	{
		throw new NotSupportedException("TarInputStream Write not supported");
	}

	public override void WriteByte(byte val)
	{
		throw new NotSupportedException("TarInputStream WriteByte not supported");
	}

	public TarInputStream(Stream inputStream)
		: this(inputStream, 20)
	{
	}

	public TarInputStream(Stream inputStream, int blockFactor)
	{
		this.inputStream = inputStream;
		buffer = TarBuffer.CreateInputTarBuffer(inputStream, blockFactor);
		readBuf = null;
		hasHitEOF = false;
		eFactory = null;
	}

	public void SetEntryFactory(IEntryFactory factory)
	{
		eFactory = factory;
	}

	public override void Close()
	{
		buffer.Close();
	}

	public int GetRecordSize()
	{
		return buffer.GetRecordSize();
	}

	public void Skip(long numToSkip)
	{
		byte[] array = new byte[8192];
		long num = numToSkip;
		while (num > 0)
		{
			int count = (int)((num > array.Length) ? array.Length : num);
			int num2 = Read(array, 0, count);
			if (num2 == -1)
			{
				break;
			}
			num -= num2;
		}
	}

	public void Mark(int markLimit)
	{
	}

	public void Reset()
	{
	}

	private void SkipToNextEntry()
	{
		long num = entrySize - entryOffset;
		if (num > 0)
		{
			Skip(num);
		}
		readBuf = null;
	}

	public TarEntry GetNextEntry()
	{
		if (hasHitEOF)
		{
			return null;
		}
		if (currEntry != null)
		{
			SkipToNextEntry();
		}
		byte[] array = buffer.ReadBlock();
		if (array == null)
		{
			hasHitEOF = true;
		}
		else if (buffer.IsEOFBlock(array))
		{
			hasHitEOF = true;
		}
		if (hasHitEOF)
		{
			currEntry = null;
		}
		else
		{
			try
			{
				TarHeader tarHeader = new TarHeader();
				tarHeader.ParseBuffer(array);
				if (!tarHeader.IsChecksumValid)
				{
					throw new TarException("Header checksum is invalid");
				}
				entryOffset = 0L;
				entrySize = tarHeader.Size;
				StringBuilder stringBuilder = null;
				if (tarHeader.TypeFlag == 76)
				{
					byte[] array2 = new byte[512];
					long num = entrySize;
					stringBuilder = new StringBuilder();
					while (num > 0)
					{
						int num2 = Read(array2, 0, (int)((num > array2.Length) ? array2.Length : num));
						if (num2 == -1)
						{
							throw new InvalidHeaderException("Failed to read long name entry");
						}
						stringBuilder.Append(TarHeader.ParseName(array2, 0, num2).ToString());
						num -= num2;
					}
					SkipToNextEntry();
					array = buffer.ReadBlock();
				}
				else if (tarHeader.TypeFlag == 103)
				{
					SkipToNextEntry();
					array = buffer.ReadBlock();
				}
				else if (tarHeader.TypeFlag == TarHeader.LF_XHDR)
				{
					SkipToNextEntry();
					array = buffer.ReadBlock();
				}
				else if (tarHeader.TypeFlag == 86)
				{
					SkipToNextEntry();
					array = buffer.ReadBlock();
				}
				else if (tarHeader.TypeFlag != 48 && tarHeader.TypeFlag != 0 && tarHeader.TypeFlag != 53)
				{
					SkipToNextEntry();
					array = buffer.ReadBlock();
				}
				if (eFactory == null)
				{
					currEntry = new TarEntry(array);
					if (stringBuilder != null)
					{
						currEntry.Name = stringBuilder.ToString();
					}
				}
				else
				{
					currEntry = eFactory.CreateEntry(array);
				}
				entryOffset = 0L;
				entrySize = currEntry.Size;
			}
			catch (InvalidHeaderException ex)
			{
				entrySize = 0L;
				entryOffset = 0L;
				currEntry = null;
				throw new InvalidHeaderException("bad header in record " + buffer.GetCurrentBlockNum() + " block " + buffer.GetCurrentBlockNum() + ", " + ex.Message);
			}
		}
		return currEntry;
	}

	public override int ReadByte()
	{
		byte[] array = new byte[1];
		int num = Read(array, 0, 1);
		if (num <= 0)
		{
			return -1;
		}
		return array[0];
	}

	public override int Read(byte[] outputBuffer, int offset, int count)
	{
		int num = 0;
		if (entryOffset >= entrySize)
		{
			return 0;
		}
		long num2 = count;
		if (num2 + entryOffset > entrySize)
		{
			num2 = entrySize - entryOffset;
		}
		if (readBuf != null)
		{
			int num3 = (int)((num2 > readBuf.Length) ? readBuf.Length : num2);
			Array.Copy(readBuf, 0, outputBuffer, offset, num3);
			if (num3 >= readBuf.Length)
			{
				readBuf = null;
			}
			else
			{
				int num4 = readBuf.Length - num3;
				byte[] destinationArray = new byte[num4];
				Array.Copy(readBuf, num3, destinationArray, 0, num4);
				readBuf = destinationArray;
			}
			num += num3;
			num2 -= num3;
			offset += num3;
		}
		while (num2 > 0)
		{
			byte[] array = buffer.ReadBlock();
			if (array == null)
			{
				throw new TarException("unexpected EOF with " + num2 + " bytes unread");
			}
			int num3 = (int)num2;
			int num5 = array.Length;
			if (num5 > num3)
			{
				Array.Copy(array, 0, outputBuffer, offset, num3);
				readBuf = new byte[num5 - num3];
				Array.Copy(array, num3, readBuf, 0, num5 - num3);
			}
			else
			{
				num3 = num5;
				Array.Copy(array, 0, outputBuffer, offset, num5);
			}
			num += num3;
			num2 -= num3;
			offset += num3;
		}
		entryOffset += num;
		return num;
	}

	public void CopyEntryContents(Stream outputStream)
	{
		byte[] array = new byte[32768];
		while (true)
		{
			bool flag = true;
			int num = Read(array, 0, array.Length);
			if (num <= 0)
			{
				break;
			}
			outputStream.Write(array, 0, num);
		}
	}
}
