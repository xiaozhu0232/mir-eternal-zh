using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.Zlib;

[Obsolete("Use 'ZInputStream' instead")]
public class ZInflaterInputStream : Stream
{
	private const int BUFSIZE = 4192;

	protected ZStream z = new ZStream();

	protected int flushLevel = 0;

	protected byte[] buf = new byte[4192];

	private byte[] buf1 = new byte[1];

	protected Stream inp = null;

	private bool nomoreinput = false;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => 0L;

	public override long Position
	{
		get
		{
			return 0L;
		}
		set
		{
		}
	}

	public ZInflaterInputStream(Stream inp)
		: this(inp, nowrap: false)
	{
	}

	public ZInflaterInputStream(Stream inp, bool nowrap)
	{
		this.inp = inp;
		z.inflateInit(nowrap);
		z.next_in = buf;
		z.next_in_index = 0;
		z.avail_in = 0;
	}

	public override void Write(byte[] b, int off, int len)
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return 0L;
	}

	public override void SetLength(long value)
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
				z.avail_in = inp.Read(buf, 0, 4192);
				if (z.avail_in <= 0)
				{
					z.avail_in = 0;
					nomoreinput = true;
				}
			}
			num = z.inflate(flushLevel);
			if (nomoreinput && num == -5)
			{
				return 0;
			}
			if (num != 0 && num != 1)
			{
				throw new IOException("inflating: " + z.msg);
			}
			if ((nomoreinput || num == 1) && z.avail_out == len)
			{
				return 0;
			}
		}
		while (z.avail_out == len && num == 0);
		return len - z.avail_out;
	}

	public override void Flush()
	{
		inp.Flush();
	}

	public override void WriteByte(byte b)
	{
	}

	public override void Close()
	{
		Platform.Dispose(inp);
		base.Close();
	}

	public override int ReadByte()
	{
		if (Read(buf1, 0, 1) <= 0)
		{
			return -1;
		}
		return buf1[0] & 0xFF;
	}
}
