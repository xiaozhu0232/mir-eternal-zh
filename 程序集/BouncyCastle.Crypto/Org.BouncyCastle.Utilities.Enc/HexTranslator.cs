namespace Org.BouncyCastle.Utilities.Encoders;

public class HexTranslator : ITranslator
{
	private static readonly byte[] hexTable = new byte[16]
	{
		48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
		97, 98, 99, 100, 101, 102
	};

	public int GetEncodedBlockSize()
	{
		return 2;
	}

	public int Encode(byte[] input, int inOff, int length, byte[] outBytes, int outOff)
	{
		int num = 0;
		int num2 = 0;
		while (num < length)
		{
			outBytes[outOff + num2] = hexTable[(input[inOff] >> 4) & 0xF];
			outBytes[outOff + num2 + 1] = hexTable[input[inOff] & 0xF];
			inOff++;
			num++;
			num2 += 2;
		}
		return length * 2;
	}

	public int GetDecodedBlockSize()
	{
		return 1;
	}

	public int Decode(byte[] input, int inOff, int length, byte[] outBytes, int outOff)
	{
		int num = length / 2;
		for (int i = 0; i < num; i++)
		{
			byte b = input[inOff + i * 2];
			byte b2 = input[inOff + i * 2 + 1];
			if (b < 97)
			{
				outBytes[outOff] = (byte)(b - 48 << 4);
			}
			else
			{
				outBytes[outOff] = (byte)(b - 97 + 10 << 4);
			}
			if (b2 < 97)
			{
				byte[] array;
				byte[] array2 = (array = outBytes);
				int num2 = outOff;
				nint num3 = num2;
				array2[num2] = (byte)(array[num3] + (byte)(b2 - 48));
			}
			else
			{
				byte[] array;
				byte[] array3 = (array = outBytes);
				int num4 = outOff;
				nint num3 = num4;
				array3[num4] = (byte)(array[num3] + (byte)(b2 - 97 + 10));
			}
			outOff++;
		}
		return num;
	}
}
