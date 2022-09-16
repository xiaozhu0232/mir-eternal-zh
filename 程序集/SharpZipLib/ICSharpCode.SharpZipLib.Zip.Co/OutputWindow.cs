using System;

namespace ICSharpCode.SharpZipLib.Zip.Compression.Streams;

public class OutputWindow
{
	private static int WINDOW_SIZE = 32768;

	private static int WINDOW_MASK = WINDOW_SIZE - 1;

	private byte[] window = new byte[WINDOW_SIZE];

	private int windowEnd = 0;

	private int windowFilled = 0;

	public void Write(int abyte)
	{
		if (windowFilled++ == WINDOW_SIZE)
		{
			throw new InvalidOperationException("Window full");
		}
		window[windowEnd++] = (byte)abyte;
		windowEnd &= WINDOW_MASK;
	}

	private void SlowRepeat(int repStart, int len, int dist)
	{
		while (len-- > 0)
		{
			window[windowEnd++] = window[repStart++];
			windowEnd &= WINDOW_MASK;
			repStart &= WINDOW_MASK;
		}
	}

	public void Repeat(int len, int dist)
	{
		if ((windowFilled += len) > WINDOW_SIZE)
		{
			throw new InvalidOperationException("Window full");
		}
		int num = (windowEnd - dist) & WINDOW_MASK;
		int num2 = WINDOW_SIZE - len;
		if (num <= num2 && windowEnd < num2)
		{
			if (len <= dist)
			{
				Array.Copy(window, num, window, windowEnd, len);
				windowEnd += len;
			}
			else
			{
				while (len-- > 0)
				{
					window[windowEnd++] = window[num++];
				}
			}
		}
		else
		{
			SlowRepeat(num, len, dist);
		}
	}

	public int CopyStored(StreamManipulator input, int len)
	{
		len = Math.Min(Math.Min(len, WINDOW_SIZE - windowFilled), input.AvailableBytes);
		int num = WINDOW_SIZE - windowEnd;
		int num2;
		if (len > num)
		{
			num2 = input.CopyBytes(window, windowEnd, num);
			if (num2 == num)
			{
				num2 += input.CopyBytes(window, 0, len - num);
			}
		}
		else
		{
			num2 = input.CopyBytes(window, windowEnd, len);
		}
		windowEnd = (windowEnd + num2) & WINDOW_MASK;
		windowFilled += num2;
		return num2;
	}

	public void CopyDict(byte[] dict, int offset, int len)
	{
		if (windowFilled > 0)
		{
			throw new InvalidOperationException();
		}
		if (len > WINDOW_SIZE)
		{
			offset += len - WINDOW_SIZE;
			len = WINDOW_SIZE;
		}
		Array.Copy(dict, offset, window, 0, len);
		windowEnd = len & WINDOW_MASK;
	}

	public int GetFreeSpace()
	{
		return WINDOW_SIZE - windowFilled;
	}

	public int GetAvailable()
	{
		return windowFilled;
	}

	public int CopyOutput(byte[] output, int offset, int len)
	{
		int num = windowEnd;
		if (len > windowFilled)
		{
			len = windowFilled;
		}
		else
		{
			num = (windowEnd - windowFilled + len) & WINDOW_MASK;
		}
		int num2 = len;
		int num3 = len - num;
		if (num3 > 0)
		{
			Array.Copy(window, WINDOW_SIZE - num3, output, offset, num3);
			offset += num3;
			len = num;
		}
		Array.Copy(window, num - len, output, offset, len);
		windowFilled -= num2;
		if (windowFilled < 0)
		{
			throw new InvalidOperationException();
		}
		return num2;
	}

	public void Reset()
	{
		windowFilled = (windowEnd = 0);
	}
}
