using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class RC2Engine : IBlockCipher
{
	private const int BLOCK_SIZE = 8;

	private static readonly byte[] piTable = new byte[256]
	{
		217, 120, 249, 196, 25, 221, 181, 237, 40, 233,
		253, 121, 74, 160, 216, 157, 198, 126, 55, 131,
		43, 118, 83, 142, 98, 76, 100, 136, 68, 139,
		251, 162, 23, 154, 89, 245, 135, 179, 79, 19,
		97, 69, 109, 141, 9, 129, 125, 50, 189, 143,
		64, 235, 134, 183, 123, 11, 240, 149, 33, 34,
		92, 107, 78, 130, 84, 214, 101, 147, 206, 96,
		178, 28, 115, 86, 192, 20, 167, 140, 241, 220,
		18, 117, 202, 31, 59, 190, 228, 209, 66, 61,
		212, 48, 163, 60, 182, 38, 111, 191, 14, 218,
		70, 105, 7, 87, 39, 242, 29, 155, 188, 148,
		67, 3, 248, 17, 199, 246, 144, 239, 62, 231,
		6, 195, 213, 47, 200, 102, 30, 215, 8, 232,
		234, 222, 128, 82, 238, 247, 132, 170, 114, 172,
		53, 77, 106, 42, 150, 26, 210, 113, 90, 21,
		73, 116, 75, 159, 208, 94, 4, 24, 164, 236,
		194, 224, 65, 110, 15, 81, 203, 204, 36, 145,
		175, 80, 161, 244, 112, 57, 153, 124, 58, 133,
		35, 184, 180, 122, 252, 2, 54, 91, 37, 85,
		151, 49, 45, 93, 250, 152, 227, 138, 146, 174,
		5, 223, 41, 16, 103, 108, 186, 201, 211, 0,
		230, 207, 225, 158, 168, 44, 99, 22, 1, 63,
		88, 226, 137, 169, 13, 56, 52, 27, 171, 51,
		255, 176, 187, 72, 12, 95, 185, 177, 205, 46,
		197, 243, 219, 71, 229, 165, 156, 119, 10, 166,
		32, 104, 254, 127, 193, 173
	};

	private int[] workingKey;

	private bool encrypting;

	public virtual string AlgorithmName => "RC2";

	public virtual bool IsPartialBlockOkay => false;

	private int[] GenerateWorkingKey(byte[] key, int bits)
	{
		int[] array = new int[128];
		for (int i = 0; i != key.Length; i++)
		{
			array[i] = key[i] & 0xFF;
		}
		int num = key.Length;
		int num3;
		if (num < 128)
		{
			int num2 = 0;
			num3 = array[num - 1];
			do
			{
				num3 = piTable[(num3 + array[num2++]) & 0xFF] & 0xFF;
				array[num++] = num3;
			}
			while (num < 128);
		}
		num = bits + 7 >> 3;
		num3 = (array[128 - num] = piTable[array[128 - num] & (255 >> (7 & -bits))] & 0xFF);
		for (int num4 = 128 - num - 1; num4 >= 0; num4--)
		{
			num3 = (array[num4] = piTable[num3 ^ array[num4 + num]] & 0xFF);
		}
		int[] array2 = new int[64];
		for (int j = 0; j != array2.Length; j++)
		{
			array2[j] = array[2 * j] + (array[2 * j + 1] << 8);
		}
		return array2;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		encrypting = forEncryption;
		if (parameters is RC2Parameters)
		{
			RC2Parameters rC2Parameters = (RC2Parameters)parameters;
			workingKey = GenerateWorkingKey(rC2Parameters.GetKey(), rC2Parameters.EffectiveKeyBits);
			return;
		}
		if (parameters is KeyParameter)
		{
			KeyParameter keyParameter = (KeyParameter)parameters;
			byte[] key = keyParameter.GetKey();
			workingKey = GenerateWorkingKey(key, key.Length * 8);
			return;
		}
		throw new ArgumentException("invalid parameter passed to RC2 init - " + Platform.GetTypeName(parameters));
	}

	public virtual void Reset()
	{
	}

	public virtual int GetBlockSize()
	{
		return 8;
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (workingKey == null)
		{
			throw new InvalidOperationException("RC2 engine not initialised");
		}
		Check.DataLength(input, inOff, 8, "input buffer too short");
		Check.OutputLength(output, outOff, 8, "output buffer too short");
		if (encrypting)
		{
			EncryptBlock(input, inOff, output, outOff);
		}
		else
		{
			DecryptBlock(input, inOff, output, outOff);
		}
		return 8;
	}

	private int RotateWordLeft(int x, int y)
	{
		x &= 0xFFFF;
		return (x << y) | (x >> 16 - y);
	}

	private void EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = ((input[inOff + 7] & 0xFF) << 8) + (input[inOff + 6] & 0xFF);
		int num2 = ((input[inOff + 5] & 0xFF) << 8) + (input[inOff + 4] & 0xFF);
		int num3 = ((input[inOff + 3] & 0xFF) << 8) + (input[inOff + 2] & 0xFF);
		int num4 = ((input[inOff + 1] & 0xFF) << 8) + (input[inOff] & 0xFF);
		for (int i = 0; i <= 16; i += 4)
		{
			num4 = RotateWordLeft(num4 + (num3 & ~num) + (num2 & num) + workingKey[i], 1);
			num3 = RotateWordLeft(num3 + (num2 & ~num4) + (num & num4) + workingKey[i + 1], 2);
			num2 = RotateWordLeft(num2 + (num & ~num3) + (num4 & num3) + workingKey[i + 2], 3);
			num = RotateWordLeft(num + (num4 & ~num2) + (num3 & num2) + workingKey[i + 3], 5);
		}
		num4 += workingKey[num & 0x3F];
		num3 += workingKey[num4 & 0x3F];
		num2 += workingKey[num3 & 0x3F];
		num += workingKey[num2 & 0x3F];
		for (int j = 20; j <= 40; j += 4)
		{
			num4 = RotateWordLeft(num4 + (num3 & ~num) + (num2 & num) + workingKey[j], 1);
			num3 = RotateWordLeft(num3 + (num2 & ~num4) + (num & num4) + workingKey[j + 1], 2);
			num2 = RotateWordLeft(num2 + (num & ~num3) + (num4 & num3) + workingKey[j + 2], 3);
			num = RotateWordLeft(num + (num4 & ~num2) + (num3 & num2) + workingKey[j + 3], 5);
		}
		num4 += workingKey[num & 0x3F];
		num3 += workingKey[num4 & 0x3F];
		num2 += workingKey[num3 & 0x3F];
		num += workingKey[num2 & 0x3F];
		for (int k = 44; k < 64; k += 4)
		{
			num4 = RotateWordLeft(num4 + (num3 & ~num) + (num2 & num) + workingKey[k], 1);
			num3 = RotateWordLeft(num3 + (num2 & ~num4) + (num & num4) + workingKey[k + 1], 2);
			num2 = RotateWordLeft(num2 + (num & ~num3) + (num4 & num3) + workingKey[k + 2], 3);
			num = RotateWordLeft(num + (num4 & ~num2) + (num3 & num2) + workingKey[k + 3], 5);
		}
		outBytes[outOff] = (byte)num4;
		outBytes[outOff + 1] = (byte)(num4 >> 8);
		outBytes[outOff + 2] = (byte)num3;
		outBytes[outOff + 3] = (byte)(num3 >> 8);
		outBytes[outOff + 4] = (byte)num2;
		outBytes[outOff + 5] = (byte)(num2 >> 8);
		outBytes[outOff + 6] = (byte)num;
		outBytes[outOff + 7] = (byte)(num >> 8);
	}

	private void DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = ((input[inOff + 7] & 0xFF) << 8) + (input[inOff + 6] & 0xFF);
		int num2 = ((input[inOff + 5] & 0xFF) << 8) + (input[inOff + 4] & 0xFF);
		int num3 = ((input[inOff + 3] & 0xFF) << 8) + (input[inOff + 2] & 0xFF);
		int num4 = ((input[inOff + 1] & 0xFF) << 8) + (input[inOff] & 0xFF);
		for (int num5 = 60; num5 >= 44; num5 -= 4)
		{
			num = RotateWordLeft(num, 11) - ((num4 & ~num2) + (num3 & num2) + workingKey[num5 + 3]);
			num2 = RotateWordLeft(num2, 13) - ((num & ~num3) + (num4 & num3) + workingKey[num5 + 2]);
			num3 = RotateWordLeft(num3, 14) - ((num2 & ~num4) + (num & num4) + workingKey[num5 + 1]);
			num4 = RotateWordLeft(num4, 15) - ((num3 & ~num) + (num2 & num) + workingKey[num5]);
		}
		num -= workingKey[num2 & 0x3F];
		num2 -= workingKey[num3 & 0x3F];
		num3 -= workingKey[num4 & 0x3F];
		num4 -= workingKey[num & 0x3F];
		for (int num6 = 40; num6 >= 20; num6 -= 4)
		{
			num = RotateWordLeft(num, 11) - ((num4 & ~num2) + (num3 & num2) + workingKey[num6 + 3]);
			num2 = RotateWordLeft(num2, 13) - ((num & ~num3) + (num4 & num3) + workingKey[num6 + 2]);
			num3 = RotateWordLeft(num3, 14) - ((num2 & ~num4) + (num & num4) + workingKey[num6 + 1]);
			num4 = RotateWordLeft(num4, 15) - ((num3 & ~num) + (num2 & num) + workingKey[num6]);
		}
		num -= workingKey[num2 & 0x3F];
		num2 -= workingKey[num3 & 0x3F];
		num3 -= workingKey[num4 & 0x3F];
		num4 -= workingKey[num & 0x3F];
		for (int num7 = 16; num7 >= 0; num7 -= 4)
		{
			num = RotateWordLeft(num, 11) - ((num4 & ~num2) + (num3 & num2) + workingKey[num7 + 3]);
			num2 = RotateWordLeft(num2, 13) - ((num & ~num3) + (num4 & num3) + workingKey[num7 + 2]);
			num3 = RotateWordLeft(num3, 14) - ((num2 & ~num4) + (num & num4) + workingKey[num7 + 1]);
			num4 = RotateWordLeft(num4, 15) - ((num3 & ~num) + (num2 & num) + workingKey[num7]);
		}
		outBytes[outOff] = (byte)num4;
		outBytes[outOff + 1] = (byte)(num4 >> 8);
		outBytes[outOff + 2] = (byte)num3;
		outBytes[outOff + 3] = (byte)(num3 >> 8);
		outBytes[outOff + 4] = (byte)num2;
		outBytes[outOff + 5] = (byte)(num2 >> 8);
		outBytes[outOff + 6] = (byte)num;
		outBytes[outOff + 7] = (byte)(num >> 8);
	}
}
