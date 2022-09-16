using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.Zlib;

public class ZInputStream : Stream
{
	private const int BufferSize = 4096;

	protected ZStream z;

	protected int flushLevel = 0;

	protected byte[] buf = new byte[4096];

	protected byte[] buf1 = new byte[1];

	protected bool compress;

	protected Stream input;

	protected bool closed;

	private bool nomoreinput = false;

	public sealed override bool CanRead => !closed;

	public sealed override bool CanSeek => false;

	public sealed override bool CanWrite => false;

	public virtual int FlushMode
	{
		get
		{
			return flushLevel;
		}
		set
		{
			flushLevel = value;
		}
	}

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

	public virtual long TotalIn => z.total_in;

	public virtual long TotalOut => z.total_out;

	private static ZStream GetDefaultZStream(bool nowrap)
	{
		ZStream zStream = new ZStream();
		zStream.inflateInit(nowrap);
		return zStream;
	}

	public ZInputStream(Stream input)
		: this(input, nowrap: false)
	{
	}

	public ZInputStream(Stream input, bool nowrap)
		: this(input, GetDefaultZStream(nowrap))
	{
	}

	public ZInputStream(Stream input, ZStream z)
	{
		if (z == null)
		{
			z = new ZStream();
		}
		if (z.istate == null && z.dstate == null)
		{
			z.inflateInit();
		}
		this.input = input;
		compress = z.istate == null;
		this.z = z;
		this.z.next_in = buf;
		this.z.next_in_index = 0;
		this.z.avail_in = 0;
	}

	public ZInputStream(Stream input, int level)
		: this(input, level, nowrap: false)
	{
	}

	public ZInputStream(Stream input, int level, bool nowrap)
	{
		this.input = input;
		compress = true;
		z = new ZStream();
		z.deflateInit(level, nowrap);
		z.next_in = buf;
		z.next_in_index = 0;
		z.avail_in = 0;
	}

	public override void Close()
	{
		if (!closed)
		{
			closed = true;
			Platform.Dispose(input);
			base.Close();
		}
	}

	public sealed override void Flush()
	{
	}

	public override int Read(byte[] b, int off, int len)
	{
		if (len == 0)
		{
			return 0;
		}
		z.next_out = b;
		z.next_out_index = off;
		z.avail_out = len;
		int num;
		do
		{
			if (z.avail_in == 0 && !nomoreinput)
			{
				z.next_in_index = 0;
				z.avail_in = input.Read(buf, 0, buf.Length);
				if (z.avail_in <= 0)
				{
					z.avail_in = 0;
					nomoreinput = true;
				}
			}
			num = (compress ? z.deflate(flushLevel) : z.inflate(flushLevel));
			if (nomoreinput && num == -5)
			{
				return 0;
			}
			if (num != 0 && num != 1)
			{
				throw new IOException((compress ? "de" : "in") + "flating: " + z.msg);
			}
			if ((nomoreinput || num == 1) && z.avail_out == len)
			{
				return 0;
			}
		}
		while (z.avail_out == len && num == 0);
		return len - z.avail_out;
	}

	public override int ReadByte()
	{
		if (Read(buf1, 0, 1) <= 0)
		{
			return -1;
		}
		return buf1[0];
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
