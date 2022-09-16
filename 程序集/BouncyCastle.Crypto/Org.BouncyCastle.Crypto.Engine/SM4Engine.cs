using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class SM4Engine : IBlockCipher
{
	private const int BlockSize = 16;

	private static readonly byte[] Sbox = new byte[256]
	{
		214, 144, 233, 254, 204, 225, 61, 183, 22, 182,
		20, 194, 40, 251, 44, 5, 43, 103, 154, 118,
		42, 190, 4, 195, 170, 68, 19, 38, 73, 134,
		6, 153, 156, 66, 80, 244, 145, 239, 152, 122,
		51, 84, 11, 67, 237, 207, 172, 98, 228, 179,
		28, 169, 201, 8, 232, 149, 128, 223, 148, 250,
		117, 143, 63, 166, 71, 7, 167, 252, 243, 115,
		23, 186, 131, 89, 60, 25, 230, 133, 79, 168,
		104, 107, 129, 178, 113, 100, 218, 139, 248, 235,
		15, 75, 112, 86, 157, 53, 30, 36, 14, 94,
		99, 88, 209, 162, 37, 34, 124, 59, 1, 33,
		120, 135, 212, 0, 70, 87, 159, 211, 39, 82,
		76, 54, 2, 231, 160, 196, 200, 158, 234, 191,
		138, 210, 64, 199, 56, 181, 163, 247, 242, 206,
		249, 97, 21, 161, 224, 174, 93, 164, 155, 52,
		26, 85, 173, 147, 50, 48, 245, 140, 177, 227,
		29, 246, 226, 46, 130, 102, 202, 96, 192, 41,
		35, 171, 13, 83, 78, 111, 213, 219, 55, 69,
		222, 253, 142, 47, 3, 255, 106, 114, 109, 108,
		91, 81, 141, 27, 175, 146, 187, 221, 188, 127,
		17, 217, 92, 65, 31, 16, 90, 216, 10, 193,
		49, 136, 165, 205, 123, 189, 45, 116, 208, 18,
		184, 229, 180, 176, 137, 105, 151, 74, 12, 150,
		119, 126, 101, 185, 241, 9, 197, 110, 198, 132,
		24, 240, 125, 236, 58, 220, 77, 32, 121, 238,
		95, 62, 215, 203, 57, 72
	};

	private static readonly uint[] CK = new uint[32]
	{
		462357u, 472066609u, 943670861u, 1415275113u, 1886879365u, 2358483617u, 2830087869u, 3301692121u, 3773296373u, 4228057617u,
		404694573u, 876298825u, 1347903077u, 1819507329u, 2291111581u, 2762715833u, 3234320085u, 3705924337u, 4177462797u, 337322537u,
		808926789u, 1280531041u, 1752135293u, 2223739545u, 2695343797u, 3166948049u, 3638552301u, 4110090761u, 269950501u, 741554753u,
		1213159005u, 1684763257u
	};

	private static readonly uint[] FK = new uint[4] { 2746333894u, 1453994832u, 1736282519u, 2993693404u };

	private uint[] rk;

	public virtual string AlgorithmName => "SM4";

	public virtual bool IsPartialBlockOkay => false;

	private static uint tau(uint A)
	{
		uint num = Sbox[A >> 24];
		uint num2 = Sbox[(A >> 16) & 0xFF];
		uint num3 = Sbox[(A >> 8) & 0xFF];
		uint num4 = Sbox[A & 0xFF];
		return (num << 24) | (num2 << 16) | (num3 << 8) | num4;
	}

	private static uint L_ap(uint B)
	{
		return B ^ Integers.RotateLeft(B, 13) ^ Integers.RotateLeft(B, 23);
	}

	private uint T_ap(uint Z)
	{
		return L_ap(tau(Z));
	}

	private void ExpandKey(bool forEncryption, byte[] key)
	{
		uint num = Pack.BE_To_UInt32(key, 0) ^ FK[0];
		uint num2 = Pack.BE_To_UInt32(key, 4) ^ FK[1];
		uint num3 = Pack.BE_To_UInt32(key, 8) ^ FK[2];
		uint num4 = Pack.BE_To_UInt32(key, 12) ^ FK[3];
		if (forEncryption)
		{
			rk[0] = num ^ T_ap(num2 ^ num3 ^ num4 ^ CK[0]);
			rk[1] = num2 ^ T_ap(num3 ^ num4 ^ rk[0] ^ CK[1]);
			rk[2] = num3 ^ T_ap(num4 ^ rk[0] ^ rk[1] ^ CK[2]);
			rk[3] = num4 ^ T_ap(rk[0] ^ rk[1] ^ rk[2] ^ CK[3]);
			for (int i = 4; i < 32; i++)
			{
				rk[i] = rk[i - 4] ^ T_ap(rk[i - 3] ^ rk[i - 2] ^ rk[i - 1] ^ CK[i]);
			}
			return;
		}
		rk[31] = num ^ T_ap(num2 ^ num3 ^ num4 ^ CK[0]);
		rk[30] = num2 ^ T_ap(num3 ^ num4 ^ rk[31] ^ CK[1]);
		rk[29] = num3 ^ T_ap(num4 ^ rk[31] ^ rk[30] ^ CK[2]);
		rk[28] = num4 ^ T_ap(rk[31] ^ rk[30] ^ rk[29] ^ CK[3]);
		for (int num5 = 27; num5 >= 0; num5--)
		{
			rk[num5] = rk[num5 + 4] ^ T_ap(rk[num5 + 3] ^ rk[num5 + 2] ^ rk[num5 + 1] ^ CK[31 - num5]);
		}
	}

	private static uint L(uint B)
	{
		return B ^ Integers.RotateLeft(B, 2) ^ Integers.RotateLeft(B, 10) ^ Integers.RotateLeft(B, 18) ^ Integers.RotateLeft(B, 24);
	}

	private static uint T(uint Z)
	{
		return L(tau(Z));
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter keyParameter))
		{
			throw new ArgumentException("invalid parameter passed to SM4 init - " + Platform.GetTypeName(parameters), "parameters");
		}
		byte[] key = keyParameter.GetKey();
		if (key.Length != 16)
		{
			throw new ArgumentException("SM4 requires a 128 bit key", "parameters");
		}
		if (rk == null)
		{
			rk = new uint[32];
		}
		ExpandKey(forEncryption, key);
	}

	public virtual int GetBlockSize()
	{
		return 16;
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (rk == null)
		{
			throw new InvalidOperationException("SM4 not initialised");
		}
		Check.DataLength(input, inOff, 16, "input buffer too short");
		Check.OutputLength(output, outOff, 16, "output buffer too short");
		uint num = Pack.BE_To_UInt32(input, inOff);
		uint num2 = Pack.BE_To_UInt32(input, inOff + 4);
		uint num3 = Pack.BE_To_UInt32(input, inOff + 8);
		uint num4 = Pack.BE_To_UInt32(input, inOff + 12);
		for (int i = 0; i < 32; i += 4)
		{
			num ^= T(num2 ^ num3 ^ num4 ^ rk[i]);
			num2 ^= T(num3 ^ num4 ^ num ^ rk[i + 1]);
			num3 ^= T(num4 ^ num ^ num2 ^ rk[i + 2]);
			num4 ^= T(num ^ num2 ^ num3 ^ rk[i + 3]);
		}
		Pack.UInt32_To_BE(num4, output, outOff);
		Pack.UInt32_To_BE(num3, output, outOff + 4);
		Pack.UInt32_To_BE(num2, output, outOff + 8);
		Pack.UInt32_To_BE(num, output, outOff + 12);
		return 16;
	}

	public virtual void Reset()
	{
	}
}
