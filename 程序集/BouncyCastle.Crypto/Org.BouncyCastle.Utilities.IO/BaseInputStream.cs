using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.IO;

public abstract class BaseInputStream : Stream
{
	private bool closed;

	public sealed override bool CanRead => !closed;

	public sealed override bool CanSeek => false;

	public sealed override bool CanWrite => false;

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

	public sealed override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = offset;
		try
		{
			int num2 = offset + count;
			while (num < num2)
			{
				int num3 = ReadByte();
				if (num3 != -1)
				{
					buffer[num++] = (byte)num3;
					continue;
				}
				break;
			}
		}
		catch (IOException)
		{
			if (num == offset)
			{
				throw;
			}
		}
		return num - offset;
	}

	public sealed override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public sealed override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public sealed override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
