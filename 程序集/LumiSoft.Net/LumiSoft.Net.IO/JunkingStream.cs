using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LumiSoft.Net.IO;

public class JunkingStream : Stream
{
	public override bool CanRead => false;

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

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override int Read([In][Out] byte[] buffer, int offset, int size)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int size)
	{
	}
}
