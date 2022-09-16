using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Tar;

public class TarOutputStream : Stream
{
	protected bool debug;

	protected long currSize;

	protected long currBytes;

	protected byte[] blockBuf;

	protected int assemLen;

	protected byte[] assemBuf;

	protected TarBuffer buffer;

	protected Stream outputStream;

	public override bool CanRead => outputStream.CanRead;

	public override bool CanSeek => outputStream.CanSeek;

	public override bool CanWrite => outputStream.CanWrite;

	public override long Length => outputStream.Length;

	public override long Position
	{
		get
		{
			return outputStream.Position;
		}
		set
		{
			outputStream.Position = value;
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return outputStream.Seek(offset, origin);
	}

	public override void SetLength(long val)
	{
		outputStream.SetLength(val);
	}

	public override int ReadByte()
	{
		return outputStream.ReadByte();
	}

	public override int Read(byte[] b, int off, int len)
	{
		return outputStream.Read(b, off, len);
	}

	public override void Flush()
	{
		outputStream.Flush();
	}

	public TarOutputStream(Stream outputStream)
		: this(outputStream, 20)
	{
	}

	public TarOutputStream(Stream outputStream, int blockFactor)
	{
		this.outputStream = outputStream;
		buffer = TarBuffer.CreateOutputTarBuffer(outputStream, blockFactor);
		debug = false;
		assemLen = 0;
		assemBuf = new byte[512];
		blockBuf = new byte[512];
	}

	public void Finish()
	{
		WriteEOFRecord();
	}

	public override void Close()
	{
		Finish();
		buffer.Close();
	}

	public int GetRecordSize()
	{
		return buffer.GetRecordSize();
	}

	public void PutNextEntry(TarEntry entry)
	{
		if (entry.TarHeader.Name.Length >= TarHeader.NAMELEN)
		{
			TarHeader tarHeader = new TarHeader();
			tarHeader.TypeFlag = 76;
			tarHeader.Name += "././@LongLink";
			tarHeader.UserId = 0;
			tarHeader.GroupId = 0;
			tarHeader.GroupName = "";
			tarHeader.UserName = "";
			tarHeader.LinkName = "";
			tarHeader.Size = entry.TarHeader.Name.Length;
			tarHeader.WriteHeader(blockBuf);
			buffer.WriteBlock(blockBuf);
			int num = 0;
			while (num < entry.TarHeader.Name.Length)
			{
				Array.Clear(blockBuf, 0, blockBuf.Length);
				TarHeader.GetAsciiBytes(entry.TarHeader.Name, num, blockBuf, 0, 512);
				num += 512;
				buffer.WriteBlock(blockBuf);
			}
		}
		entry.WriteEntryHeader(blockBuf);
		buffer.WriteBlock(blockBuf);
		currBytes = 0L;
		currSize = (entry.IsDirectory ? 0 : entry.Size);
	}

	public void CloseEntry()
	{
		if (assemLen > 0)
		{
			for (int i = assemLen; i < assemBuf.Length; i++)
			{
				assemBuf[i] = 0;
			}
			buffer.WriteBlock(assemBuf);
			currBytes += assemLen;
			assemLen = 0;
		}
		if (currBytes < currSize)
		{
			throw new TarException("entry closed at '" + currBytes + "' before the '" + currSize + "' bytes specified in the header were written");
		}
	}

	public override void WriteByte(byte b)
	{
		Write(new byte[1] { b }, 0, 1);
	}

	public override void Write(byte[] wBuf, int wOffset, int numToWrite)
	{
		if (wBuf == null)
		{
			throw new ArgumentNullException("TarOutputStream.Write buffer null");
		}
		if (currBytes + numToWrite > currSize)
		{
			throw new ArgumentOutOfRangeException("request to write '" + numToWrite + "' bytes exceeds size in header of '" + currSize + "' bytes");
		}
		if (assemLen > 0)
		{
			if (assemLen + numToWrite >= blockBuf.Length)
			{
				int num = blockBuf.Length - assemLen;
				Array.Copy(assemBuf, 0, blockBuf, 0, assemLen);
				Array.Copy(wBuf, wOffset, blockBuf, assemLen, num);
				buffer.WriteBlock(blockBuf);
				currBytes += blockBuf.Length;
				wOffset += num;
				numToWrite -= num;
				assemLen = 0;
			}
			else
			{
				Array.Copy(wBuf, wOffset, assemBuf, assemLen, numToWrite);
				wOffset += numToWrite;
				assemLen += numToWrite;
				numToWrite -= numToWrite;
			}
		}
		while (numToWrite > 0)
		{
			if (numToWrite < blockBuf.Length)
			{
				Array.Copy(wBuf, wOffset, assemBuf, assemLen, numToWrite);
				assemLen += numToWrite;
				break;
			}
			buffer.WriteBlock(wBuf, wOffset);
			int num2 = blockBuf.Length;
			currBytes += num2;
			numToWrite -= num2;
			wOffset += num2;
		}
	}

	private void WriteEOFRecord()
	{
		Array.Clear(blockBuf, 0, blockBuf.Length);
		buffer.WriteBlock(blockBuf);
	}
}
