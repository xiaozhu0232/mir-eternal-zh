using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class ByteQueue
{
	private const int DefaultCapacity = 1024;

	private byte[] databuf;

	private int skipped = 0;

	private int available = 0;

	private bool readOnlyBuf = false;

	public int Available => available;

	public static int NextTwoPow(int i)
	{
		i |= i >> 1;
		i |= i >> 2;
		i |= i >> 4;
		i |= i >> 8;
		i |= i >> 16;
		return i + 1;
	}

	public ByteQueue()
		: this(1024)
	{
	}

	public ByteQueue(int capacity)
	{
		databuf = ((capacity == 0) ? TlsUtilities.EmptyBytes : new byte[capacity]);
	}

	public ByteQueue(byte[] buf, int off, int len)
	{
		databuf = buf;
		skipped = off;
		available = len;
		readOnlyBuf = true;
	}

	public void AddData(byte[] data, int offset, int len)
	{
		if (readOnlyBuf)
		{
			throw new InvalidOperationException("Cannot add data to read-only buffer");
		}
		if (skipped + available + len > databuf.Length)
		{
			int num = NextTwoPow(available + len);
			if (num > databuf.Length)
			{
				byte[] destinationArray = new byte[num];
				Array.Copy(databuf, skipped, destinationArray, 0, available);
				databuf = destinationArray;
			}
			else
			{
				Array.Copy(databuf, skipped, databuf, 0, available);
			}
			skipped = 0;
		}
		Array.Copy(data, offset, databuf, skipped + available, len);
		available += len;
	}

	public void CopyTo(Stream output, int length)
	{
		if (length > available)
		{
			throw new InvalidOperationException("Cannot copy " + length + " bytes, only got " + available);
		}
		output.Write(databuf, skipped, length);
	}

	public void Read(byte[] buf, int offset, int len, int skip)
	{
		if (buf.Length - offset < len)
		{
			throw new ArgumentException("Buffer size of " + buf.Length + " is too small for a read of " + len + " bytes");
		}
		if (available - skip < len)
		{
			throw new InvalidOperationException("Not enough data to read");
		}
		Array.Copy(databuf, skipped + skip, buf, offset, len);
	}

	public MemoryStream ReadFrom(int length)
	{
		if (length > available)
		{
			throw new InvalidOperationException("Cannot read " + length + " bytes, only got " + available);
		}
		int index = skipped;
		available -= length;
		skipped += length;
		return new MemoryStream(databuf, index, length, writable: false);
	}

	public void RemoveData(int i)
	{
		if (i > available)
		{
			throw new InvalidOperationException("Cannot remove " + i + " bytes, only got " + available);
		}
		available -= i;
		skipped += i;
	}

	public void RemoveData(byte[] buf, int off, int len, int skip)
	{
		Read(buf, off, len, skip);
		RemoveData(skip + len);
	}

	public byte[] RemoveData(int len, int skip)
	{
		byte[] array = new byte[len];
		RemoveData(array, 0, len, skip);
		return array;
	}

	public void Shrink()
	{
		if (available == 0)
		{
			databuf = TlsUtilities.EmptyBytes;
			skipped = 0;
			return;
		}
		int num = NextTwoPow(available);
		if (num < databuf.Length)
		{
			byte[] destinationArray = new byte[num];
			Array.Copy(databuf, skipped, destinationArray, 0, available);
			databuf = destinationArray;
			skipped = 0;
		}
	}
}
