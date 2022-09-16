using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class RijndaelEngine : IBlockCipher
{
	private static readonly int MAXROUNDS = 14;

	private static readonly int MAXKC = 64;

	private static readonly byte[] Logtable = new byte[256]
	{
		0, 0, 25, 1, 50, 2, 26, 198, 75, 199,
		27, 104, 51, 238, 223, 3, 100, 4, 224, 14,
		52, 141, 129, 239, 76, 113, 8, 200, 248, 105,
		28, 193, 125, 194, 29, 181, 249, 185, 39, 106,
		77, 228, 166, 114, 154, 201, 9, 120, 101, 47,
		138, 5, 33, 15, 225, 36, 18, 240, 130, 69,
		53, 147, 218, 142, 150, 143, 219, 189, 54, 208,
		206, 148, 19, 92, 210, 241, 64, 70, 131, 56,
		102, 221, 253, 48, 191, 6, 139, 98, 179, 37,
		226, 152, 34, 136, 145, 16, 126, 110, 72, 195,
		163, 182, 30, 66, 58, 107, 40, 84, 250, 133,
		61, 186, 43, 121, 10, 21, 155, 159, 94, 202,
		78, 212, 172, 229, 243, 115, 167, 87, 175, 88,
		168, 80, 244, 234, 214, 116, 79, 174, 233, 213,
		231, 230, 173, 232, 44, 215, 117, 122, 235, 22,
		11, 245, 89, 203, 95, 176, 156, 169, 81, 160,
		127, 12, 246, 111, 23, 196, 73, 236, 216, 67,
		31, 45, 164, 118, 123, 183, 204, 187, 62, 90,
		251, 96, 177, 134, 59, 82, 161, 108, 170, 85,
		41, 157, 151, 178, 135, 144, 97, 190, 220, 252,
		188, 149, 207, 205, 55, 63, 91, 209, 83, 57,
		132, 60, 65, 162, 109, 71, 20, 42, 158, 93,
		86, 242, 211, 171, 68, 17, 146, 217, 35, 32,
		46, 137, 180, 124, 184, 38, 119, 153, 227, 165,
		103, 74, 237, 222, 197, 49, 254, 24, 13, 99,
		140, 128, 192, 247, 112, 7
	};

	private static readonly byte[] Alogtable = new byte[511]
	{
		0, 3, 5, 15, 17, 51, 85, 255, 26, 46,
		114, 150, 161, 248, 19, 53, 95, 225, 56, 72,
		216, 115, 149, 164, 247, 2, 6, 10, 30, 34,
		102, 170, 229, 52, 92, 228, 55, 89, 235, 38,
		106, 190, 217, 112, 144, 171, 230, 49, 83, 245,
		4, 12, 20, 60, 68, 204, 79, 209, 104, 184,
		211, 110, 178, 205, 76, 212, 103, 169, 224, 59,
		77, 215, 98, 166, 241, 8, 24, 40, 120, 136,
		131, 158, 185, 208, 107, 189, 220, 127, 129, 152,
		179, 206, 73, 219, 118, 154, 181, 196, 87, 249,
		16, 48, 80, 240, 11, 29, 39, 105, 187, 214,
		97, 163, 254, 25, 43, 125, 135, 146, 173, 236,
		47, 113, 147, 174, 233, 32, 96, 160, 251, 22,
		58, 78, 210, 109, 183, 194, 93, 231, 50, 86,
		250, 21, 63, 65, 195, 94, 226, 61, 71, 201,
		64, 192, 91, 237, 44, 116, 156, 191, 218, 117,
		159, 186, 213, 100, 172, 239, 42, 126, 130, 157,
		188, 223, 122, 142, 137, 128, 155, 182, 193, 88,
		232, 35, 101, 175, 234, 37, 111, 177, 200, 67,
		197, 84, 252, 31, 33, 99, 165, 244, 7, 9,
		27, 45, 119, 153, 176, 203, 70, 202, 69, 207,
		74, 222, 121, 139, 134, 145, 168, 227, 62, 66,
		198, 81, 243, 14, 18, 54, 90, 238, 41, 123,
		141, 140, 143, 138, 133, 148, 167, 242, 13, 23,
		57, 75, 221, 124, 132, 151, 162, 253, 28, 36,
		108, 180, 199, 82, 246, 1, 3, 5, 15, 17,
		51, 85, 255, 26, 46, 114, 150, 161, 248, 19,
		53, 95, 225, 56, 72, 216, 115, 149, 164, 247,
		2, 6, 10, 30, 34, 102, 170, 229, 52, 92,
		228, 55, 89, 235, 38, 106, 190, 217, 112, 144,
		171, 230, 49, 83, 245, 4, 12, 20, 60, 68,
		204, 79, 209, 104, 184, 211, 110, 178, 205, 76,
		212, 103, 169, 224, 59, 77, 215, 98, 166, 241,
		8, 24, 40, 120, 136, 131, 158, 185, 208, 107,
		189, 220, 127, 129, 152, 179, 206, 73, 219, 118,
		154, 181, 196, 87, 249, 16, 48, 80, 240, 11,
		29, 39, 105, 187, 214, 97, 163, 254, 25, 43,
		125, 135, 146, 173, 236, 47, 113, 147, 174, 233,
		32, 96, 160, 251, 22, 58, 78, 210, 109, 183,
		194, 93, 231, 50, 86, 250, 21, 63, 65, 195,
		94, 226, 61, 71, 201, 64, 192, 91, 237, 44,
		116, 156, 191, 218, 117, 159, 186, 213, 100, 172,
		239, 42, 126, 130, 157, 188, 223, 122, 142, 137,
		128, 155, 182, 193, 88, 232, 35, 101, 175, 234,
		37, 111, 177, 200, 67, 197, 84, 252, 31, 33,
		99, 165, 244, 7, 9, 27, 45, 119, 153, 176,
		203, 70, 202, 69, 207, 74, 222, 121, 139, 134,
		145, 168, 227, 62, 66, 198, 81, 243, 14, 18,
		54, 90, 238, 41, 123, 141, 140, 143, 138, 133,
		148, 167, 242, 13, 23, 57, 75, 221, 124, 132,
		151, 162, 253, 28, 36, 108, 180, 199, 82, 246,
		1
	};

	private static readonly byte[] S = new byte[256]
	{
		99, 124, 119, 123, 242, 107, 111, 197, 48, 1,
		103, 43, 254, 215, 171, 118, 202, 130, 201, 125,
		250, 89, 71, 240, 173, 212, 162, 175, 156, 164,
		114, 192, 183, 253, 147, 38, 54, 63, 247, 204,
		52, 165, 229, 241, 113, 216, 49, 21, 4, 199,
		35, 195, 24, 150, 5, 154, 7, 18, 128, 226,
		235, 39, 178, 117, 9, 131, 44, 26, 27, 110,
		90, 160, 82, 59, 214, 179, 41, 227, 47, 132,
		83, 209, 0, 237, 32, 252, 177, 91, 106, 203,
		190, 57, 74, 76, 88, 207, 208, 239, 170, 251,
		67, 77, 51, 133, 69, 249, 2, 127, 80, 60,
		159, 168, 81, 163, 64, 143, 146, 157, 56, 245,
		188, 182, 218, 33, 16, 255, 243, 210, 205, 12,
		19, 236, 95, 151, 68, 23, 196, 167, 126, 61,
		100, 93, 25, 115, 96, 129, 79, 220, 34, 42,
		144, 136, 70, 238, 184, 20, 222, 94, 11, 219,
		224, 50, 58, 10, 73, 6, 36, 92, 194, 211,
		172, 98, 145, 149, 228, 121, 231, 200, 55, 109,
		141, 213, 78, 169, 108, 86, 244, 234, 101, 122,
		174, 8, 186, 120, 37, 46, 28, 166, 180, 198,
		232, 221, 116, 31, 75, 189, 139, 138, 112, 62,
		181, 102, 72, 3, 246, 14, 97, 53, 87, 185,
		134, 193, 29, 158, 225, 248, 152, 17, 105, 217,
		142, 148, 155, 30, 135, 233, 206, 85, 40, 223,
		140, 161, 137, 13, 191, 230, 66, 104, 65, 153,
		45, 15, 176, 84, 187, 22
	};

	private static readonly byte[] Si = new byte[256]
	{
		82, 9, 106, 213, 48, 54, 165, 56, 191, 64,
		163, 158, 129, 243, 215, 251, 124, 227, 57, 130,
		155, 47, 255, 135, 52, 142, 67, 68, 196, 222,
		233, 203, 84, 123, 148, 50, 166, 194, 35, 61,
		238, 76, 149, 11, 66, 250, 195, 78, 8, 46,
		161, 102, 40, 217, 36, 178, 118, 91, 162, 73,
		109, 139, 209, 37, 114, 248, 246, 100, 134, 104,
		152, 22, 212, 164, 92, 204, 93, 101, 182, 146,
		108, 112, 72, 80, 253, 237, 185, 218, 94, 21,
		70, 87, 167, 141, 157, 132, 144, 216, 171, 0,
		140, 188, 211, 10, 247, 228, 88, 5, 184, 179,
		69, 6, 208, 44, 30, 143, 202, 63, 15, 2,
		193, 175, 189, 3, 1, 19, 138, 107, 58, 145,
		17, 65, 79, 103, 220, 234, 151, 242, 207, 206,
		240, 180, 230, 115, 150, 172, 116, 34, 231, 173,
		53, 133, 226, 249, 55, 232, 28, 117, 223, 110,
		71, 241, 26, 113, 29, 41, 197, 137, 111, 183,
		98, 14, 170, 24, 190, 27, 252, 86, 62, 75,
		198, 210, 121, 32, 154, 219, 192, 254, 120, 205,
		90, 244, 31, 221, 168, 51, 136, 7, 199, 49,
		177, 18, 16, 89, 39, 128, 236, 95, 96, 81,
		127, 169, 25, 181, 74, 13, 45, 229, 122, 159,
		147, 201, 156, 239, 160, 224, 59, 77, 174, 42,
		245, 176, 200, 235, 187, 60, 131, 83, 153, 97,
		23, 43, 4, 126, 186, 119, 214, 38, 225, 105,
		20, 99, 85, 33, 12, 125
	};

	private static readonly byte[] rcon = new byte[30]
	{
		1, 2, 4, 8, 16, 32, 64, 128, 27, 54,
		108, 216, 171, 77, 154, 47, 94, 188, 99, 198,
		151, 53, 106, 212, 179, 125, 250, 239, 197, 145
	};

	private static readonly byte[][] shifts0 = new byte[5][]
	{
		new byte[4] { 0, 8, 16, 24 },
		new byte[4] { 0, 8, 16, 24 },
		new byte[4] { 0, 8, 16, 24 },
		new byte[4] { 0, 8, 16, 32 },
		new byte[4] { 0, 8, 24, 32 }
	};

	private static readonly byte[][] shifts1 = new byte[5][]
	{
		new byte[4] { 0, 24, 16, 8 },
		new byte[4] { 0, 32, 24, 16 },
		new byte[4] { 0, 40, 32, 24 },
		new byte[4] { 0, 48, 40, 24 },
		new byte[4] { 0, 56, 40, 32 }
	};

	private int BC;

	private long BC_MASK;

	private int ROUNDS;

	private int blockBits;

	private long[][] workingKey;

	private long A0;

	private long A1;

	private long A2;

	private long A3;

	private bool forEncryption;

	private byte[] shifts0SC;

	private byte[] shifts1SC;

	public virtual string AlgorithmName => "Rijndael";

	public virtual bool IsPartialBlockOkay => false;

	private byte Mul0x2(int b)
	{
		if (b != 0)
		{
			return Alogtable[25 + (Logtable[b] & 0xFF)];
		}
		return 0;
	}

	private byte Mul0x3(int b)
	{
		if (b != 0)
		{
			return Alogtable[1 + (Logtable[b] & 0xFF)];
		}
		return 0;
	}

	private byte Mul0x9(int b)
	{
		if (b >= 0)
		{
			return Alogtable[199 + b];
		}
		return 0;
	}

	private byte Mul0xb(int b)
	{
		if (b >= 0)
		{
			return Alogtable[104 + b];
		}
		return 0;
	}

	private byte Mul0xd(int b)
	{
		if (b >= 0)
		{
			return Alogtable[238 + b];
		}
		return 0;
	}

	private byte Mul0xe(int b)
	{
		if (b >= 0)
		{
			return Alogtable[223 + b];
		}
		return 0;
	}

	private void KeyAddition(long[] rk)
	{
		A0 ^= rk[0];
		A1 ^= rk[1];
		A2 ^= rk[2];
		A3 ^= rk[3];
	}

	private long Shift(long r, int shift)
	{
		ulong num = (ulong)r >> shift;
		if (shift > 31)
		{
			num &= 0xFFFFFFFFu;
		}
		return ((long)num | (r << BC - shift)) & BC_MASK;
	}

	private void ShiftRow(byte[] shiftsSC)
	{
		A1 = Shift(A1, shiftsSC[1]);
		A2 = Shift(A2, shiftsSC[2]);
		A3 = Shift(A3, shiftsSC[3]);
	}

	private long ApplyS(long r, byte[] box)
	{
		long num = 0L;
		for (int i = 0; i < BC; i += 8)
		{
			num |= (long)(box[(int)((r >> i) & 0xFF)] & 0xFF) << i;
		}
		return num;
	}

	private void Substitution(byte[] box)
	{
		A0 = ApplyS(A0, box);
		A1 = ApplyS(A1, box);
		A2 = ApplyS(A2, box);
		A3 = ApplyS(A3, box);
	}

	private void MixColumn()
	{
		long num3;
		long num2;
		long num;
		long num4 = (num3 = (num2 = (num = 0L)));
		for (int i = 0; i < BC; i += 8)
		{
			int num5 = (int)((A0 >> i) & 0xFF);
			int num6 = (int)((A1 >> i) & 0xFF);
			int num7 = (int)((A2 >> i) & 0xFF);
			int num8 = (int)((A3 >> i) & 0xFF);
			num4 |= (long)((Mul0x2(num5) ^ Mul0x3(num6) ^ num7 ^ num8) & 0xFF) << i;
			num3 |= (long)((Mul0x2(num6) ^ Mul0x3(num7) ^ num8 ^ num5) & 0xFF) << i;
			num2 |= (long)((Mul0x2(num7) ^ Mul0x3(num8) ^ num5 ^ num6) & 0xFF) << i;
			num |= (long)((Mul0x2(num8) ^ Mul0x3(num5) ^ num6 ^ num7) & 0xFF) << i;
		}
		A0 = num4;
		A1 = num3;
		A2 = num2;
		A3 = num;
	}

	private void InvMixColumn()
	{
		long num3;
		long num2;
		long num;
		long num4 = (num3 = (num2 = (num = 0L)));
		for (int i = 0; i < BC; i += 8)
		{
			int num5 = (int)((A0 >> i) & 0xFF);
			int num6 = (int)((A1 >> i) & 0xFF);
			int num7 = (int)((A2 >> i) & 0xFF);
			int num8 = (int)((A3 >> i) & 0xFF);
			num5 = ((num5 != 0) ? (Logtable[num5 & 0xFF] & 0xFF) : (-1));
			num6 = ((num6 != 0) ? (Logtable[num6 & 0xFF] & 0xFF) : (-1));
			num7 = ((num7 != 0) ? (Logtable[num7 & 0xFF] & 0xFF) : (-1));
			num8 = ((num8 != 0) ? (Logtable[num8 & 0xFF] & 0xFF) : (-1));
			num4 |= (long)((Mul0xe(num5) ^ Mul0xb(num6) ^ Mul0xd(num7) ^ Mul0x9(num8)) & 0xFF) << i;
			num3 |= (long)((Mul0xe(num6) ^ Mul0xb(num7) ^ Mul0xd(num8) ^ Mul0x9(num5)) & 0xFF) << i;
			num2 |= (long)((Mul0xe(num7) ^ Mul0xb(num8) ^ Mul0xd(num5) ^ Mul0x9(num6)) & 0xFF) << i;
			num |= (long)((Mul0xe(num8) ^ Mul0xb(num5) ^ Mul0xd(num6) ^ Mul0x9(num7)) & 0xFF) << i;
		}
		A0 = num4;
		A1 = num3;
		A2 = num2;
		A3 = num;
	}

	private long[][] GenerateWorkingKey(byte[] key)
	{
		int num = 0;
		int num2 = key.Length * 8;
		byte[,] array = new byte[4, MAXKC];
		long[][] array2 = new long[MAXROUNDS + 1][];
		for (int i = 0; i < MAXROUNDS + 1; i++)
		{
			array2[i] = new long[4];
		}
		int num3 = num2 switch
		{
			128 => 4, 
			160 => 5, 
			192 => 6, 
			224 => 7, 
			256 => 8, 
			_ => throw new ArgumentException("Key length not 128/160/192/224/256 bits."), 
		};
		if (num2 >= blockBits)
		{
			ROUNDS = num3 + 6;
		}
		else
		{
			ROUNDS = BC / 8 + 6;
		}
		int num4 = 0;
		for (int j = 0; j < key.Length; j++)
		{
			array[j % 4, j / 4] = key[num4++];
		}
		int num5 = 0;
		int num6 = 0;
		while (num6 < num3 && num5 < (ROUNDS + 1) * (BC / 8))
		{
			for (int k = 0; k < 4; k++)
			{
				long[] array3;
				long[] array4 = (array3 = array2[num5 / (BC / 8)]);
				int num7 = k;
				nint num8 = num7;
				array4[num7] = array3[num8] | ((long)(array[k, num6] & 0xFF) << num5 * 8 % BC);
			}
			num6++;
			num5++;
		}
		while (num5 < (ROUNDS + 1) * (BC / 8))
		{
			byte[,] array5;
			for (int l = 0; l < 4; l++)
			{
				byte[,] array6 = (array5 = array);
				int num9 = l;
				nint num8 = num9;
				array6[num9, 0] = (byte)(array5[(int)num8, 0] ^ S[array[(l + 1) % 4, num3 - 1] & 0xFF]);
			}
			(array5 = array)[0, 0] = (byte)(array5[0, 0] ^ rcon[num++]);
			if (num3 <= 6)
			{
				for (int m = 1; m < num3; m++)
				{
					for (int n = 0; n < 4; n++)
					{
						byte[,] array7 = (array5 = array);
						int num10 = n;
						nint num8 = num10;
						int num11 = m;
						nint num12 = num11;
						array7[num10, num11] = (byte)(array5[(int)num8, (int)num12] ^ array[n, m - 1]);
					}
				}
			}
			else
			{
				for (int num13 = 1; num13 < 4; num13++)
				{
					for (int num14 = 0; num14 < 4; num14++)
					{
						byte[,] array8 = (array5 = array);
						int num15 = num14;
						nint num8 = num15;
						int num16 = num13;
						nint num12 = num16;
						array8[num15, num16] = (byte)(array5[(int)num8, (int)num12] ^ array[num14, num13 - 1]);
					}
				}
				for (int num17 = 0; num17 < 4; num17++)
				{
					byte[,] array9 = (array5 = array);
					int num18 = num17;
					nint num8 = num18;
					array9[num18, 4] = (byte)(array5[(int)num8, 4] ^ S[array[num17, 3] & 0xFF]);
				}
				for (int num19 = 5; num19 < num3; num19++)
				{
					for (int num20 = 0; num20 < 4; num20++)
					{
						byte[,] array10 = (array5 = array);
						int num21 = num20;
						nint num8 = num21;
						int num22 = num19;
						nint num12 = num22;
						array10[num21, num22] = (byte)(array5[(int)num8, (int)num12] ^ array[num20, num19 - 1]);
					}
				}
			}
			int num23 = 0;
			while (num23 < num3 && num5 < (ROUNDS + 1) * (BC / 8))
			{
				for (int num24 = 0; num24 < 4; num24++)
				{
					long[] array3;
					long[] array11 = (array3 = array2[num5 / (BC / 8)]);
					int num25 = num24;
					nint num8 = num25;
					array11[num25] = array3[num8] | ((long)(array[num24, num23] & 0xFF) << num5 * 8 % BC);
				}
				num23++;
				num5++;
			}
		}
		return array2;
	}

	public RijndaelEngine()
		: this(128)
	{
	}

	public RijndaelEngine(int blockBits)
	{
		switch (blockBits)
		{
		case 128:
			BC = 32;
			BC_MASK = 4294967295L;
			shifts0SC = shifts0[0];
			shifts1SC = shifts1[0];
			break;
		case 160:
			BC = 40;
			BC_MASK = 1099511627775L;
			shifts0SC = shifts0[1];
			shifts1SC = shifts1[1];
			break;
		case 192:
			BC = 48;
			BC_MASK = 281474976710655L;
			shifts0SC = shifts0[2];
			shifts1SC = shifts1[2];
			break;
		case 224:
			BC = 56;
			BC_MASK = 72057594037927935L;
			shifts0SC = shifts0[3];
			shifts1SC = shifts1[3];
			break;
		case 256:
			BC = 64;
			BC_MASK = -1L;
			shifts0SC = shifts0[4];
			shifts1SC = shifts1[4];
			break;
		default:
			throw new ArgumentException("unknown blocksize to Rijndael");
		}
		this.blockBits = blockBits;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (typeof(KeyParameter).IsInstanceOfType(parameters))
		{
			workingKey = GenerateWorkingKey(((KeyParameter)parameters).GetKey());
			this.forEncryption = forEncryption;
			return;
		}
		throw new ArgumentException("invalid parameter passed to Rijndael init - " + Platform.GetTypeName(parameters));
	}

	public virtual int GetBlockSize()
	{
		return BC / 2;
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (workingKey == null)
		{
			throw new InvalidOperationException("Rijndael engine not initialised");
		}
		Check.DataLength(input, inOff, BC / 2, "input buffer too short");
		Check.OutputLength(output, outOff, BC / 2, "output buffer too short");
		UnPackBlock(input, inOff);
		if (forEncryption)
		{
			EncryptBlock(workingKey);
		}
		else
		{
			DecryptBlock(workingKey);
		}
		PackBlock(output, outOff);
		return BC / 2;
	}

	public virtual void Reset()
	{
	}

	private void UnPackBlock(byte[] bytes, int off)
	{
		int num = off;
		A0 = bytes[num++] & 0xFF;
		A1 = bytes[num++] & 0xFF;
		A2 = bytes[num++] & 0xFF;
		A3 = bytes[num++] & 0xFF;
		for (int i = 8; i != BC; i += 8)
		{
			A0 |= (long)(bytes[num++] & 0xFF) << i;
			A1 |= (long)(bytes[num++] & 0xFF) << i;
			A2 |= (long)(bytes[num++] & 0xFF) << i;
			A3 |= (long)(bytes[num++] & 0xFF) << i;
		}
	}

	private void PackBlock(byte[] bytes, int off)
	{
		int num = off;
		for (int i = 0; i != BC; i += 8)
		{
			bytes[num++] = (byte)(A0 >> i);
			bytes[num++] = (byte)(A1 >> i);
			bytes[num++] = (byte)(A2 >> i);
			bytes[num++] = (byte)(A3 >> i);
		}
	}

	private void EncryptBlock(long[][] rk)
	{
		KeyAddition(rk[0]);
		for (int i = 1; i < ROUNDS; i++)
		{
			Substitution(S);
			ShiftRow(shifts0SC);
			MixColumn();
			KeyAddition(rk[i]);
		}
		Substitution(S);
		ShiftRow(shifts0SC);
		KeyAddition(rk[ROUNDS]);
	}

	private void DecryptBlock(long[][] rk)
	{
		KeyAddition(rk[ROUNDS]);
		Substitution(Si);
		ShiftRow(shifts1SC);
		for (int num = ROUNDS - 1; num > 0; num--)
		{
			KeyAddition(rk[num]);
			InvMixColumn();
			Substitution(Si);
			ShiftRow(shifts1SC);
		}
		KeyAddition(rk[0]);
	}
}
