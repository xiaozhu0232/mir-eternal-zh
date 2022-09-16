using System;

namespace ICSharpCode.SharpZipLib.Checksums;

public sealed class Adler32 : IChecksum
{
	private static readonly uint BASE = 65521u;

	private uint checksum;

	public long Value => checksum;

	public Adler32()
	{
		Reset();
	}

	public void Reset()
	{
		checksum = 1u;
	}

	public void Update(int bval)
	{
		uint num = checksum & 0xFFFFu;
		uint num2 = checksum >> 16;
		num = (uint)((int)num + (bval & 0xFF)) % BASE;
		num2 = (num + num2) % BASE;
		checksum = (num2 << 16) + num;
	}

	public void Update(byte[] buffer)
	{
		Update(buffer, 0, buffer.Length);
	}

	public void Update(byte[] buf, int off, int len)
	{
		if (buf == null)
		{
			throw new ArgumentNullException("buf");
		}
		if (off < 0 || len < 0 || off + len > buf.Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		uint num = checksum & 0xFFFFu;
		uint num2 = checksum >> 16;
		while (len > 0)
		{
			int num3 = 3800;
			if (num3 > len)
			{
				num3 = len;
			}
			len -= num3;
			while (--num3 >= 0)
			{
				num += (uint)(buf[off++] & 0xFF);
				num2 += num;
			}
			num %= BASE;
			num2 %= BASE;
		}
		checksum = (num2 << 16) | num;
	}
}
