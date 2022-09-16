using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.Encoders;

public class Base64Encoder : IEncoder
{
	protected readonly byte[] encodingTable = new byte[64]
	{
		65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
		75, 76, 77, 78, 79, 80, 81, 82, 83, 84,
		85, 86, 87, 88, 89, 90, 97, 98, 99, 100,
		101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
		111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
		121, 122, 48, 49, 50, 51, 52, 53, 54, 55,
		56, 57, 43, 47
	};

	protected byte padding = 61;

	protected readonly byte[] decodingTable = new byte[128];

	protected void InitialiseDecodingTable()
	{
		Arrays.Fill(decodingTable, byte.MaxValue);
		for (int i = 0; i < encodingTable.Length; i++)
		{
			decodingTable[encodingTable[i]] = (byte)i;
		}
	}

	public Base64Encoder()
	{
		InitialiseDecodingTable();
	}

	public int Encode(byte[] inBuf, int inOff, int inLen, byte[] outBuf, int outOff)
	{
		int num = inOff;
		int num2 = inOff + inLen - 2;
		int num3 = outOff;
		while (num < num2)
		{
			uint num4 = inBuf[num++];
			uint num5 = inBuf[num++];
			uint num6 = inBuf[num++];
			outBuf[num3++] = encodingTable[(num4 >> 2) & 0x3F];
			outBuf[num3++] = encodingTable[((num4 << 4) | (num5 >> 4)) & 0x3F];
			outBuf[num3++] = encodingTable[((num5 << 2) | (num6 >> 6)) & 0x3F];
			outBuf[num3++] = encodingTable[num6 & 0x3F];
		}
		switch (inLen - (num - inOff))
		{
		case 1:
		{
			uint num9 = inBuf[num++];
			outBuf[num3++] = encodingTable[(num9 >> 2) & 0x3F];
			outBuf[num3++] = encodingTable[(num9 << 4) & 0x3F];
			outBuf[num3++] = padding;
			outBuf[num3++] = padding;
			break;
		}
		case 2:
		{
			uint num7 = inBuf[num++];
			uint num8 = inBuf[num++];
			outBuf[num3++] = encodingTable[(num7 >> 2) & 0x3F];
			outBuf[num3++] = encodingTable[((num7 << 4) | (num8 >> 4)) & 0x3F];
			outBuf[num3++] = encodingTable[(num8 << 2) & 0x3F];
			outBuf[num3++] = padding;
			break;
		}
		}
		return num3 - outOff;
	}

	public int Encode(byte[] buf, int off, int len, Stream outStream)
	{
		byte[] array = new byte[72];
		while (len > 0)
		{
			int num = System.Math.Min(54, len);
			int count = Encode(buf, off, num, array, 0);
			outStream.Write(array, 0, count);
			off += num;
			len -= num;
		}
		return (len + 2) / 3 * 4;
	}

	private bool ignore(char c)
	{
		if (c != '\n' && c != '\r' && c != '\t')
		{
			return c == ' ';
		}
		return true;
	}

	public int Decode(byte[] data, int off, int length, Stream outStream)
	{
		byte[] array = new byte[54];
		int num = 0;
		int num2 = 0;
		int num3 = off + length;
		while (num3 > off && ignore((char)data[num3 - 1]))
		{
			num3--;
		}
		int i = off;
		int num4 = num3 - 4;
		for (i = nextI(data, i, num4); i < num4; i = nextI(data, i, num4))
		{
			byte b = decodingTable[data[i++]];
			i = nextI(data, i, num4);
			byte b2 = decodingTable[data[i++]];
			i = nextI(data, i, num4);
			byte b3 = decodingTable[data[i++]];
			i = nextI(data, i, num4);
			byte b4 = decodingTable[data[i++]];
			if ((b | b2 | b3 | b4) >= 128)
			{
				throw new IOException("invalid characters encountered in base64 data");
			}
			array[num++] = (byte)((b << 2) | (b2 >> 4));
			array[num++] = (byte)((b2 << 4) | (b3 >> 2));
			array[num++] = (byte)((b3 << 6) | b4);
			if (num == array.Length)
			{
				outStream.Write(array, 0, num);
				num = 0;
			}
			num2 += 3;
		}
		if (num > 0)
		{
			outStream.Write(array, 0, num);
		}
		int num5 = nextI(data, i, num3);
		int num6 = nextI(data, num5 + 1, num3);
		int num7 = nextI(data, num6 + 1, num3);
		int num8 = nextI(data, num7 + 1, num3);
		return num2 + decodeLastBlock(outStream, (char)data[num5], (char)data[num6], (char)data[num7], (char)data[num8]);
	}

	private int nextI(byte[] data, int i, int finish)
	{
		while (i < finish && ignore((char)data[i]))
		{
			i++;
		}
		return i;
	}

	public int DecodeString(string data, Stream outStream)
	{
		byte[] array = new byte[54];
		int num = 0;
		int num2 = 0;
		int num3 = data.Length;
		while (num3 > 0 && ignore(data[num3 - 1]))
		{
			num3--;
		}
		int i = 0;
		int num4 = num3 - 4;
		for (i = nextI(data, i, num4); i < num4; i = nextI(data, i, num4))
		{
			byte b = decodingTable[(uint)data[i++]];
			i = nextI(data, i, num4);
			byte b2 = decodingTable[(uint)data[i++]];
			i = nextI(data, i, num4);
			byte b3 = decodingTable[(uint)data[i++]];
			i = nextI(data, i, num4);
			byte b4 = decodingTable[(uint)data[i++]];
			if ((b | b2 | b3 | b4) >= 128)
			{
				throw new IOException("invalid characters encountered in base64 data");
			}
			array[num++] = (byte)((b << 2) | (b2 >> 4));
			array[num++] = (byte)((b2 << 4) | (b3 >> 2));
			array[num++] = (byte)((b3 << 6) | b4);
			num2 += 3;
			if (num == array.Length)
			{
				outStream.Write(array, 0, num);
				num = 0;
			}
		}
		if (num > 0)
		{
			outStream.Write(array, 0, num);
		}
		int num5 = nextI(data, i, num3);
		int num6 = nextI(data, num5 + 1, num3);
		int num7 = nextI(data, num6 + 1, num3);
		int index = nextI(data, num7 + 1, num3);
		return num2 + decodeLastBlock(outStream, data[num5], data[num6], data[num7], data[index]);
	}

	private int decodeLastBlock(Stream outStream, char c1, char c2, char c3, char c4)
	{
		if (c3 == padding)
		{
			if (c4 != padding)
			{
				throw new IOException("invalid characters encountered at end of base64 data");
			}
			byte b = decodingTable[(uint)c1];
			byte b2 = decodingTable[(uint)c2];
			if ((b | b2) >= 128)
			{
				throw new IOException("invalid characters encountered at end of base64 data");
			}
			outStream.WriteByte((byte)((b << 2) | (b2 >> 4)));
			return 1;
		}
		if (c4 == padding)
		{
			byte b3 = decodingTable[(uint)c1];
			byte b4 = decodingTable[(uint)c2];
			byte b5 = decodingTable[(uint)c3];
			if ((b3 | b4 | b5) >= 128)
			{
				throw new IOException("invalid characters encountered at end of base64 data");
			}
			outStream.WriteByte((byte)((b3 << 2) | (b4 >> 4)));
			outStream.WriteByte((byte)((b4 << 4) | (b5 >> 2)));
			return 2;
		}
		byte b6 = decodingTable[(uint)c1];
		byte b7 = decodingTable[(uint)c2];
		byte b8 = decodingTable[(uint)c3];
		byte b9 = decodingTable[(uint)c4];
		if ((b6 | b7 | b8 | b9) >= 128)
		{
			throw new IOException("invalid characters encountered at end of base64 data");
		}
		outStream.WriteByte((byte)((b6 << 2) | (b7 >> 4)));
		outStream.WriteByte((byte)((b7 << 4) | (b8 >> 2)));
		outStream.WriteByte((byte)((b8 << 6) | b9));
		return 3;
	}

	private int nextI(string data, int i, int finish)
	{
		while (i < finish && ignore(data[i]))
		{
			i++;
		}
		return i;
	}
}
