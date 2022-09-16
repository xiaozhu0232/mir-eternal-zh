using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class SkipjackEngine : IBlockCipher
{
	private const int BLOCK_SIZE = 8;

	private static readonly short[] ftable = new short[256]
	{
		163, 215, 9, 131, 248, 72, 246, 244, 179, 33,
		21, 120, 153, 177, 175, 249, 231, 45, 77, 138,
		206, 76, 202, 46, 82, 149, 217, 30, 78, 56,
		68, 40, 10, 223, 2, 160, 23, 241, 96, 104,
		18, 183, 122, 195, 233, 250, 61, 83, 150, 132,
		107, 186, 242, 99, 154, 25, 124, 174, 229, 245,
		247, 22, 106, 162, 57, 182, 123, 15, 193, 147,
		129, 27, 238, 180, 26, 234, 208, 145, 47, 184,
		85, 185, 218, 133, 63, 65, 191, 224, 90, 88,
		128, 95, 102, 11, 216, 144, 53, 213, 192, 167,
		51, 6, 101, 105, 69, 0, 148, 86, 109, 152,
		155, 118, 151, 252, 178, 194, 176, 254, 219, 32,
		225, 235, 214, 228, 221, 71, 74, 29, 66, 237,
		158, 110, 73, 60, 205, 67, 39, 210, 7, 212,
		222, 199, 103, 24, 137, 203, 48, 31, 141, 198,
		143, 170, 200, 116, 220, 201, 93, 92, 49, 164,
		112, 136, 97, 44, 159, 13, 43, 135, 80, 130,
		84, 100, 38, 125, 3, 64, 52, 75, 28, 115,
		209, 196, 253, 59, 204, 251, 127, 171, 230, 62,
		91, 165, 173, 4, 35, 156, 20, 81, 34, 240,
		41, 121, 113, 126, 255, 140, 14, 226, 12, 239,
		188, 114, 117, 111, 55, 161, 236, 211, 142, 98,
		139, 134, 16, 232, 8, 119, 17, 190, 146, 79,
		36, 197, 50, 54, 157, 207, 243, 166, 187, 172,
		94, 108, 169, 19, 87, 37, 181, 227, 189, 168,
		58, 1, 5, 89, 42, 70
	};

	private int[] key0;

	private int[] key1;

	private int[] key2;

	private int[] key3;

	private bool encrypting;

	public virtual string AlgorithmName => "SKIPJACK";

	public virtual bool IsPartialBlockOkay => false;

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("invalid parameter passed to SKIPJACK init - " + Platform.GetTypeName(parameters));
		}
		byte[] key = ((KeyParameter)parameters).GetKey();
		encrypting = forEncryption;
		key0 = new int[32];
		key1 = new int[32];
		key2 = new int[32];
		key3 = new int[32];
		for (int i = 0; i < 32; i++)
		{
			key0[i] = key[i * 4 % 10] & 0xFF;
			key1[i] = key[(i * 4 + 1) % 10] & 0xFF;
			key2[i] = key[(i * 4 + 2) % 10] & 0xFF;
			key3[i] = key[(i * 4 + 3) % 10] & 0xFF;
		}
	}

	public virtual int GetBlockSize()
	{
		return 8;
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (key1 == null)
		{
			throw new InvalidOperationException("SKIPJACK engine not initialised");
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

	public virtual void Reset()
	{
	}

	private int G(int k, int w)
	{
		int num = (w >> 8) & 0xFF;
		int num2 = w & 0xFF;
		int num3 = ftable[num2 ^ key0[k]] ^ num;
		int num4 = ftable[num3 ^ key1[k]] ^ num2;
		int num5 = ftable[num4 ^ key2[k]] ^ num3;
		int num6 = ftable[num5 ^ key3[k]] ^ num4;
		return (num5 << 8) + num6;
	}

	public virtual int EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = (input[inOff] << 8) + (input[inOff + 1] & 0xFF);
		int num2 = (input[inOff + 2] << 8) + (input[inOff + 3] & 0xFF);
		int num3 = (input[inOff + 4] << 8) + (input[inOff + 5] & 0xFF);
		int num4 = (input[inOff + 6] << 8) + (input[inOff + 7] & 0xFF);
		int num5 = 0;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int num6 = num4;
				num4 = num3;
				num3 = num2;
				num2 = G(num5, num);
				num = num2 ^ num6 ^ (num5 + 1);
				num5++;
			}
			for (int k = 0; k < 8; k++)
			{
				int num7 = num4;
				num4 = num3;
				num3 = num ^ num2 ^ (num5 + 1);
				num2 = G(num5, num);
				num = num7;
				num5++;
			}
		}
		outBytes[outOff] = (byte)(num >> 8);
		outBytes[outOff + 1] = (byte)num;
		outBytes[outOff + 2] = (byte)(num2 >> 8);
		outBytes[outOff + 3] = (byte)num2;
		outBytes[outOff + 4] = (byte)(num3 >> 8);
		outBytes[outOff + 5] = (byte)num3;
		outBytes[outOff + 6] = (byte)(num4 >> 8);
		outBytes[outOff + 7] = (byte)num4;
		return 8;
	}

	private int H(int k, int w)
	{
		int num = w & 0xFF;
		int num2 = (w >> 8) & 0xFF;
		int num3 = ftable[num2 ^ key3[k]] ^ num;
		int num4 = ftable[num3 ^ key2[k]] ^ num2;
		int num5 = ftable[num4 ^ key1[k]] ^ num3;
		int num6 = ftable[num5 ^ key0[k]] ^ num4;
		return (num6 << 8) + num5;
	}

	public virtual int DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = (input[inOff] << 8) + (input[inOff + 1] & 0xFF);
		int num2 = (input[inOff + 2] << 8) + (input[inOff + 3] & 0xFF);
		int num3 = (input[inOff + 4] << 8) + (input[inOff + 5] & 0xFF);
		int num4 = (input[inOff + 6] << 8) + (input[inOff + 7] & 0xFF);
		int num5 = 31;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int num6 = num3;
				num3 = num4;
				num4 = num;
				num = H(num5, num2);
				num2 = num ^ num6 ^ (num5 + 1);
				num5--;
			}
			for (int k = 0; k < 8; k++)
			{
				int num7 = num3;
				num3 = num4;
				num4 = num2 ^ num ^ (num5 + 1);
				num = H(num5, num2);
				num2 = num7;
				num5--;
			}
		}
		outBytes[outOff] = (byte)(num >> 8);
		outBytes[outOff + 1] = (byte)num;
		outBytes[outOff + 2] = (byte)(num2 >> 8);
		outBytes[outOff + 3] = (byte)num2;
		outBytes[outOff + 4] = (byte)(num3 >> 8);
		outBytes[outOff + 5] = (byte)num3;
		outBytes[outOff + 6] = (byte)(num4 >> 8);
		outBytes[outOff + 7] = (byte)num4;
		return 8;
	}
}
