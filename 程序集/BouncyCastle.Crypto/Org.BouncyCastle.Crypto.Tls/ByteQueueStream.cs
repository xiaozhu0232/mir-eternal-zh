using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class ByteQueueStream : Stream
{
	private readonly ByteQueue buffer;

	public virtual int Available => buffer.Available;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public ByteQueueStream()
	{
		buffer = new ByteQueue();
	}

	public override void Flush()
	{
	}

	public virtual int Peek(byte[] buf)
	{
		int num = System.Math.Min(buffer.Available, buf.Length);
		buffer.Read(buf, 0, num, 0);
		return num;
	}

	public virtual int Read(byte[] buf)
	{
		return Read(buf, 0, buf.Length);
	}

	public override int Read(byte[] buf, int off, int len)
	{
		int num = System.Math.Min(buffer.Available, len);
		buffer.RemoveData(buf, off, num, 0);
		return num;
	}

	public override int ReadByte()
	{
		if (buffer.Available == 0)
		{
			return -1;
		}
		return buffer.RemoveData(1, 0)[0] & 0xFF;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public virtual int Skip(int n)
	{
		int num = System.Math.Min(buffer.Available, n);
		buffer.RemoveData(num);
		return num;
	}

	public virtual void Write(byte[] buf)
	{
		buffer.AddData(buf, 0, buf.Length);
	}

	public override void Write(byte[] buf, int off, int len)
	{
		buffer.AddData(buf, off, len);
	}

	public override void WriteByte(byte b)
	{
		buffer.AddData(new byte[1] { b }, 0, 1);
	}
}
