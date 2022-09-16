using System.IO;

namespace Org.BouncyCastle.Utilities.IO;

public sealed class Streams
{
	private const int BufferSize = 512;

	private Streams()
	{
	}

	public static void Drain(Stream inStr)
	{
		byte[] array = new byte[512];
		while (inStr.Read(array, 0, array.Length) > 0)
		{
		}
	}

	public static byte[] ReadAll(Stream inStr)
	{
		MemoryStream memoryStream = new MemoryStream();
		PipeAll(inStr, memoryStream);
		return memoryStream.ToArray();
	}

	public static byte[] ReadAllLimited(Stream inStr, int limit)
	{
		MemoryStream memoryStream = new MemoryStream();
		PipeAllLimited(inStr, limit, memoryStream);
		return memoryStream.ToArray();
	}

	public static int ReadFully(Stream inStr, byte[] buf)
	{
		return ReadFully(inStr, buf, 0, buf.Length);
	}

	public static int ReadFully(Stream inStr, byte[] buf, int off, int len)
	{
		int i;
		int num;
		for (i = 0; i < len; i += num)
		{
			num = inStr.Read(buf, off + i, len - i);
			if (num < 1)
			{
				break;
			}
		}
		return i;
	}

	public static void PipeAll(Stream inStr, Stream outStr)
	{
		byte[] array = new byte[512];
		int count;
		while ((count = inStr.Read(array, 0, array.Length)) > 0)
		{
			outStr.Write(array, 0, count);
		}
	}

	public static long PipeAllLimited(Stream inStr, long limit, Stream outStr)
	{
		byte[] array = new byte[512];
		long num = 0L;
		int num2;
		while ((num2 = inStr.Read(array, 0, array.Length)) > 0)
		{
			if (limit - num < num2)
			{
				throw new StreamOverflowException("Data Overflow");
			}
			num += num2;
			outStr.Write(array, 0, num2);
		}
		return num;
	}

	public static void WriteBufTo(MemoryStream buf, Stream output)
	{
		buf.WriteTo(output);
	}

	public static int WriteBufTo(MemoryStream buf, byte[] output, int offset)
	{
		int num = (int)buf.Length;
		buf.WriteTo(new MemoryStream(output, offset, num, writable: true));
		return num;
	}

	public static void WriteZeroes(Stream outStr, long count)
	{
		byte[] buffer = new byte[512];
		while (count > 512)
		{
			outStr.Write(buffer, 0, 512);
			count -= 512;
		}
		outStr.Write(buffer, 0, (int)count);
	}
}
