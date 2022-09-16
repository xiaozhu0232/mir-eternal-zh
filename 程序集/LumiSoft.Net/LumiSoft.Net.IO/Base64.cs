using System;
using System.Text;

namespace LumiSoft.Net.IO;

public class Base64
{
	private static readonly byte[] BASE64_ENCODE_TABLE = new byte[64]
	{
		65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
		75, 76, 77, 78, 79, 80, 81, 82, 83, 84,
		85, 86, 87, 88, 89, 90, 97, 98, 99, 100,
		101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
		111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
		121, 122, 48, 49, 50, 51, 52, 53, 54, 55,
		56, 57, 43, 47
	};

	private static readonly short[] BASE64_DECODE_TABLE = new short[128]
	{
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, 62, -1, -1, -1, 63, 52, 53,
		54, 55, 56, 57, 58, 59, 60, 61, -1, -1,
		-1, -1, -1, -1, -1, 0, 1, 2, 3, 4,
		5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
		15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
		25, -1, -1, -1, -1, -1, -1, 26, 27, 28,
		29, 30, 31, 32, 33, 34, 35, 36, 37, 38,
		39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
		49, 50, 51, -1, -1, -1, -1, -1
	};

	public byte[] Encode(byte[] buffer, int offset, int count, bool last)
	{
		throw new NotImplementedException();
	}

	public byte[] Decode(string value, bool ignoreNonBase64Chars)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		byte[] bytes = Encoding.ASCII.GetBytes(value);
		byte[] array = new byte[bytes.Length];
		int num = Decode(bytes, 0, bytes.Length, array, 0, ignoreNonBase64Chars);
		byte[] array2 = new byte[num];
		Array.Copy(array, array2, num);
		return array2;
	}

	public byte[] Decode(byte[] data, int offset, int count, bool ignoreNonBase64Chars)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] array = new byte[data.Length];
		int num = Decode(data, offset, count, array, 0, ignoreNonBase64Chars);
		byte[] array2 = new byte[num];
		Array.Copy(array, array2, num);
		return array2;
	}

	public int Decode(byte[] encBuffer, int encOffset, int encCount, byte[] buffer, int offset, bool ignoreNonBase64Chars)
	{
		if (encBuffer == null)
		{
			throw new ArgumentNullException("encBuffer");
		}
		if (encOffset < 0)
		{
			throw new ArgumentOutOfRangeException("encOffset", "Argument 'encOffset' value must be >= 0.");
		}
		if (encCount < 0)
		{
			throw new ArgumentOutOfRangeException("encCount", "Argument 'encCount' value must be >= 0.");
		}
		if (encOffset + encCount > encBuffer.Length)
		{
			throw new ArgumentOutOfRangeException("encCount", "Argument 'count' is bigger than than argument 'encBuffer'.");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset >= buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		int num = encOffset;
		int result = 0;
		byte[] array = new byte[4];
		while (num - encOffset < encCount)
		{
			int num2 = 0;
			while (num2 < 4)
			{
				if (num - encOffset >= encCount)
				{
					if (num2 == 0)
					{
						break;
					}
					throw new FormatException("Invalid incomplete base64 4-char block");
				}
				short num3 = encBuffer[num++];
				if (num3 == 61)
				{
					if (num2 < 2)
					{
						throw new FormatException("Invalid base64 padding.");
					}
					if (num2 == 2)
					{
						num++;
					}
					break;
				}
				if (num3 > 127 || BASE64_DECODE_TABLE[num3] == -1)
				{
					if (!ignoreNonBase64Chars)
					{
						throw new FormatException("Invalid base64 char '" + num3 + "'.");
					}
				}
				else
				{
					array[num2++] = (byte)BASE64_DECODE_TABLE[num3];
				}
			}
			if (num2 > 1)
			{
				buffer[result++] = (byte)((array[0] << 2) | (array[1] >> 4));
			}
			if (num2 > 2)
			{
				buffer[result++] = (byte)(((array[1] & 0xF) << 4) | (array[2] >> 2));
			}
			if (num2 > 3)
			{
				buffer[result++] = (byte)(((array[2] & 3) << 6) | array[3]);
			}
		}
		return result;
	}
}
