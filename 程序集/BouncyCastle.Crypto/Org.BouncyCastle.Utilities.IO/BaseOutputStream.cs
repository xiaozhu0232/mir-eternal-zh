using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.IO;

public abstract class BaseOutputStream : Stream
{
	private bool closed;

	public sealed override bool CanRead => false;

	public sealed override bool CanSeek => false;

	public sealed override bool CanWrite => !closed;

	public sealed override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public sealed override long Position
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

	public override void Close()
	{
		closed = true;
		base.Close();
	}

	public override void Flush()
	{
	}

	public sealed override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public sealed override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public sealed override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		int num = offset + count;
		for (int i = offset; i < num; i++)
		{
			WriteByte(buffer[i]);
		}
	}

	public virtual void Write(params byte[] buffer)
	{
		Write(buffer, 0, buffer.Length);
	}

	public override void WriteByte(byte b)
	{
		Write(new byte[1] { b }, 0, 1);
	}
}
