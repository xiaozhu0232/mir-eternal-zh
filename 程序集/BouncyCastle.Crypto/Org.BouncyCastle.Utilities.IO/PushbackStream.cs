using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.IO;

public class PushbackStream : FilterStream
{
	private int buf = -1;

	public PushbackStream(Stream s)
		: base(s)
	{
	}

	public override int ReadByte()
	{
		if (buf != -1)
		{
			int result = buf;
			buf = -1;
			return result;
		}
		return base.ReadByte();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buf != -1 && count > 0)
		{
			buffer[offset] = (byte)buf;
			buf = -1;
			return 1;
		}
		return base.Read(buffer, offset, count);
	}

	public virtual void Unread(int b)
	{
		if (buf != -1)
		{
			throw new InvalidOperationException("Can only push back one byte");
		}
		buf = b & 0xFF;
	}
}
