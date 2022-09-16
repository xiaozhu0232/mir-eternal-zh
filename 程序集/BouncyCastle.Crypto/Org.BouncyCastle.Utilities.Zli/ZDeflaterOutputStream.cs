using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.Zlib;

[Obsolete("Use 'ZOutputStream' instead")]
public class ZDeflaterOutputStream : Stream
{
	private const int BUFSIZE = 4192;

	protected ZStream z = new ZStream();

	protected int flushLevel = 0;

	protected byte[] buf = new byte[4192];

	private byte[] buf1 = new byte[1];

	protected Stream outp;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

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

	public ZDeflaterOutputStream(Stream outp)
		: this(outp, 6, nowrap: false)
	{
	}

	public ZDeflaterOutputStream(Stream outp, int level)
		: this(outp, level, nowrap: false)
	{
	}

	public ZDeflaterOutputStream(Stream outp, int level, bool nowrap)
	{
		this.outp = outp;
		z.deflateInit(level, nowrap);
	}

	public override void Write(byte[] b, int off, int len)
	{
		if (len == 0)
		{
			return;
		}
		z.next_in = b;
		z.next_in_index = off;
		z.avail_in = len;
		do
		{
			z.next_out = buf;
			z.next_out_index = 0;
			z.avail_out = 4192;
			if (z.deflate(flushLevel) != 0)
			{
				throw new IOException("deflating: " + z.msg);
			}
			if (z.avail_out < 4192)
			{
				outp.Write(buf, 0, 4192 - z.avail_out);
			}
		}
		while (z.avail_in > 0 || z.avail_out == 0);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return 0L;
	}

	public override void SetLength(long value)
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return 0;
	}

	public override void Flush()
	{
		outp.Flush();
	}

	public override void WriteByte(byte b)
	{
		buf1[0] = b;
		Write(buf1, 0, 1);
	}

	public void Finish()
	{
		do
		{
			z.next_out = buf;
			z.next_out_index = 0;
			z.avail_out = 4192;
			int num = z.deflate(4);
			if (num != 1 && num != 0)
			{
				throw new IOException("deflating: " + z.msg);
			}
			if (4192 - z.avail_out > 0)
			{
				outp.Write(buf, 0, 4192 - z.avail_out);
			}
		}
		while (z.avail_in > 0 || z.avail_out == 0);
		Flush();
	}

	public void End()
	{
		if (z != null)
		{
			z.deflateEnd();
			z.free();
			z = null;
		}
	}

	public override void Close()
	{
		try
		{
			Finish();
		}
		catch (IOException)
		{
		}
		finally
		{
			End();
			Platform.Dispose(outp);
			outp = null;
		}
		base.Close();
	}
}
