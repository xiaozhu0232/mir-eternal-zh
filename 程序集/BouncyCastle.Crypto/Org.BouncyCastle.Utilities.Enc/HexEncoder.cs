using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.Encoders;

public class HexEncoder : IEncoder
{
	protected readonly byte[] encodingTable = new byte[16]
	{
		48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
		97, 98, 99, 100, 101, 102
	};

	protected readonly byte[] decodingTable = new byte[128];

	protected void InitialiseDecodingTable()
	{
		Arrays.Fill(decodingTable, byte.MaxValue);
		for (int i = 0; i < encodingTable.Length; i++)
		{
			decodingTable[encodingTable[i]] = (byte)i;
		}
		decodingTable[65] = decodingTable[97];
		decodingTable[66] = decodingTable[98];
		decodingTable[67] = decodingTable[99];
		decodingTable[68] = decodingTable[100];
		decodingTable[69] = decodingTable[101];
		decodingTable[70] = decodingTable[102];
	}

	public HexEncoder()
	{
		InitialiseDecodingTable();
	}

	public int Encode(byte[] inBuf, int inOff, int inLen, byte[] outBuf, int outOff)
	{
		int num = inOff;
		int num2 = inOff + inLen;
		int num3 = outOff;
		while (num < num2)
		{
			uint num4 = inBuf[num++];
			outBuf[num3++] = encodingTable[num4 >> 4];
			outBuf[num3++] = encodingTable[num4 & 0xF];
		}
		return num3 - outOff;
	}

	public int Encode(byte[] buf, int off, int len, Stream outStream)
	{
		byte[] array = new byte[72];
		while (len > 0)
		{
			int num = System.Math.Min(36, len);
			int count = Encode(buf, off, num, array, 0);
			outStream.Write(array, 0, count);
			off += num;
			len -= num;
		}
		return len * 2;
	}

	private static bool Ignore(char c)
	{
		if (c != '\n' && c != '\r' && c != '\t')
		{
			return c == ' ';
		}
		return true;
	}

	public int Decode(byte[] data, int off, int length, Stream outStream)
	{
		int num = 0;
		byte[] array = new byte[36];
		int num2 = 0;
		int num3 = off + length;
		while (num3 > off && Ignore((char)data[num3 - 1]))
		{
			num3--;
		}
		int i = off;
		while (i < num3)
		{
			for (; i < num3 && Ignore((char)data[i]); i++)
			{
			}
			byte b = decodingTable[data[i++]];
			for (; i < num3 && Ignore((char)data[i]); i++)
			{
			}
			byte b2 = decodingTable[data[i++]];
			if ((b | b2) >= 128)
			{
				throw new IOException("invalid characters encountered in Hex data");
			}
			array[num2++] = (byte)((b << 4) | b2);
			if (num2 == array.Length)
			{
				outStream.Write(array, 0, num2);
				num2 = 0;
			}
			num++;
		}
		if (num2 > 0)
		{
			outStream.Write(array, 0, num2);
		}
		return num;
	}

	public int DecodeString(string data, Stream outStream)
	{
		int num = 0;
		byte[] array = new byte[36];
		int num2 = 0;
		int num3 = data.Length;
		while (num3 > 0 && Ignore(data[num3 - 1]))
		{
			num3--;
		}
		int i = 0;
		while (i < num3)
		{
			for (; i < num3 && Ignore(data[i]); i++)
			{
			}
			byte b = decodingTable[(uint)data[i++]];
			for (; i < num3 && Ignore(data[i]); i++)
			{
			}
			byte b2 = decodingTable[(uint)data[i++]];
			if ((b | b2) >= 128)
			{
				throw new IOException("invalid characters encountered in Hex data");
			}
			array[num2++] = (byte)((b << 4) | b2);
			if (num2 == array.Length)
			{
				outStream.Write(array, 0, num2);
				num2 = 0;
			}
			num++;
		}
		if (num2 > 0)
		{
			outStream.Write(array, 0, num2);
		}
		return num;
	}

	internal byte[] DecodeStrict(string str, int off, int len)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (off < 0 || len < 0 || off > str.Length - len)
		{
			throw new IndexOutOfRangeException("invalid offset and/or length specified");
		}
		if (((uint)len & (true ? 1u : 0u)) != 0)
		{
			throw new ArgumentException("a hexadecimal encoding must have an even number of characters", "len");
		}
		int num = len >> 1;
		byte[] array = new byte[num];
		int num2 = off;
		for (int i = 0; i < num; i++)
		{
			byte b = decodingTable[(uint)str[num2++]];
			byte b2 = decodingTable[(uint)str[num2++]];
			if ((b | b2) >= 128)
			{
				throw new IOException("invalid characters encountered in Hex data");
			}
			array[i] = (byte)((b << 4) | b2);
		}
		return array;
	}
}
