using System;

namespace ICSharpCode.SharpZipLib.Zip.Compression.Streams;

public class StreamManipulator
{
	private byte[] window;

	private int window_start = 0;

	private int window_end = 0;

	private uint buffer = 0u;

	private int bits_in_buffer = 0;

	public int AvailableBits => bits_in_buffer;

	public int AvailableBytes => window_end - window_start + (bits_in_buffer >> 3);

	public bool IsNeedingInput => window_start == window_end;

	public int PeekBits(int n)
	{
		if (bits_in_buffer < n)
		{
			if (window_start == window_end)
			{
				return -1;
			}
			buffer |= (uint)(((window[window_start++] & 0xFF) | ((window[window_start++] & 0xFF) << 8)) << bits_in_buffer);
			bits_in_buffer += 16;
		}
		return (int)(buffer & ((1 << n) - 1));
	}

	public void DropBits(int n)
	{
		buffer >>= n;
		bits_in_buffer -= n;
	}

	public int GetBits(int n)
	{
		int num = PeekBits(n);
		if (num >= 0)
		{
			DropBits(n);
		}
		return num;
	}

	public void SkipToByteBoundary()
	{
		buffer >>= bits_in_buffer & 7;
		bits_in_buffer &= -8;
	}

	public int CopyBytes(byte[] output, int offset, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (((uint)bits_in_buffer & 7u) != 0)
		{
			throw new InvalidOperationException("Bit buffer is not byte aligned!");
		}
		int num = 0;
		while (bits_in_buffer > 0 && length > 0)
		{
			output[offset++] = (byte)buffer;
			buffer >>= 8;
			bits_in_buffer -= 8;
			length--;
			num++;
		}
		if (length == 0)
		{
			return num;
		}
		int num2 = window_end - window_start;
		if (length > num2)
		{
			length = num2;
		}
		Array.Copy(window, window_start, output, offset, length);
		window_start += length;
		if (((uint)(window_start - window_end) & (true ? 1u : 0u)) != 0)
		{
			buffer = window[window_start++] & 0xFFu;
			bits_in_buffer = 8;
		}
		return num + length;
	}

	public void Reset()
	{
		buffer = (uint)(window_start = (window_end = (bits_in_buffer = 0)));
	}

	public void SetInput(byte[] buf, int off, int len)
	{
		if (window_start < window_end)
		{
			throw new InvalidOperationException("Old input was not completely processed");
		}
		int num = off + len;
		if (0 > off || off > num || num > buf.Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (((uint)len & (true ? 1u : 0u)) != 0)
		{
			buffer |= (uint)((buf[off++] & 0xFF) << bits_in_buffer);
			bits_in_buffer += 8;
		}
		window = buf;
		window_start = off;
		window_end = num;
	}
}
