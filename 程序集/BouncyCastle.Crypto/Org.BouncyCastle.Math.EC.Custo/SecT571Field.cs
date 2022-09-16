using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT571Field
{
	private const ulong M59 = 576460752303423487uL;

	private static readonly ulong[] ROOT_Z = new ulong[9] { 3161836309350906777uL, 10804290191530228771uL, 14625517132619890193uL, 7312758566309945096uL, 17890083061325672324uL, 8945041530681231562uL, 13695892802195391589uL, 6847946401097695794uL, 541669439031730457uL };

	public static void Add(ulong[] x, ulong[] y, ulong[] z)
	{
		for (int i = 0; i < 9; i++)
		{
			z[i] = x[i] ^ y[i];
		}
	}

	private static void Add(ulong[] x, int xOff, ulong[] y, int yOff, ulong[] z, int zOff)
	{
		for (int i = 0; i < 9; i++)
		{
			z[zOff + i] = x[xOff + i] ^ y[yOff + i];
		}
	}

	public static void AddBothTo(ulong[] x, ulong[] y, ulong[] z)
	{
		for (int i = 0; i < 9; i++)
		{
			ulong[] array;
			ulong[] array2 = (array = z);
			int num = i;
			nint num2 = num;
			array2[num] = array[num2] ^ (x[i] ^ y[i]);
		}
	}

	private static void AddBothTo(ulong[] x, int xOff, ulong[] y, int yOff, ulong[] z, int zOff)
	{
		for (int i = 0; i < 9; i++)
		{
			ulong[] array;
			ulong[] array2 = (array = z);
			int num = zOff + i;
			nint num2 = num;
			array2[num] = array[num2] ^ (x[xOff + i] ^ y[yOff + i]);
		}
	}

	public static void AddExt(ulong[] xx, ulong[] yy, ulong[] zz)
	{
		for (int i = 0; i < 18; i++)
		{
			zz[i] = xx[i] ^ yy[i];
		}
	}

	public static void AddOne(ulong[] x, ulong[] z)
	{
		z[0] = x[0] ^ 1;
		for (int i = 1; i < 9; i++)
		{
			z[i] = x[i];
		}
	}

	private static void AddTo(ulong[] x, ulong[] z)
	{
		for (int i = 0; i < 9; i++)
		{
			ulong[] array;
			ulong[] array2 = (array = z);
			int num = i;
			nint num2 = num;
			array2[num] = array[num2] ^ x[i];
		}
	}

	public static ulong[] FromBigInteger(BigInteger x)
	{
		return Nat.FromBigInteger64(571, x);
	}

	public static void HalfTrace(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat576.CreateExt64();
		Nat576.Copy64(x, z);
		for (int i = 1; i < 571; i += 2)
		{
			ImplSquare(z, array);
			Reduce(array, z);
			ImplSquare(z, array);
			Reduce(array, z);
			AddTo(x, z);
		}
	}

	public static void Invert(ulong[] x, ulong[] z)
	{
		if (Nat576.IsZero64(x))
		{
			throw new InvalidOperationException();
		}
		ulong[] array = Nat576.Create64();
		ulong[] array2 = Nat576.Create64();
		ulong[] array3 = Nat576.Create64();
		Square(x, array3);
		Square(array3, array);
		Square(array, array2);
		Multiply(array, array2, array);
		SquareN(array, 2, array2);
		Multiply(array, array2, array);
		Multiply(array, array3, array);
		SquareN(array, 5, array2);
		Multiply(array, array2, array);
		SquareN(array2, 5, array2);
		Multiply(array, array2, array);
		SquareN(array, 15, array2);
		Multiply(array, array2, array3);
		SquareN(array3, 30, array);
		SquareN(array, 30, array2);
		Multiply(array, array2, array);
		SquareN(array, 60, array2);
		Multiply(array, array2, array);
		SquareN(array2, 60, array2);
		Multiply(array, array2, array);
		SquareN(array, 180, array2);
		Multiply(array, array2, array);
		SquareN(array2, 180, array2);
		Multiply(array, array2, array);
		Multiply(array, array3, z);
	}

	public static void Multiply(ulong[] x, ulong[] y, ulong[] z)
	{
		ulong[] array = Nat576.CreateExt64();
		ImplMultiply(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(ulong[] x, ulong[] y, ulong[] zz)
	{
		ulong[] array = Nat576.CreateExt64();
		ImplMultiply(x, y, array);
		AddExt(zz, array, zz);
	}

	public static void MultiplyPrecomp(ulong[] x, ulong[] precomp, ulong[] z)
	{
		ulong[] array = Nat576.CreateExt64();
		ImplMultiplyPrecomp(x, precomp, array);
		Reduce(array, z);
	}

	public static void MultiplyPrecompAddToExt(ulong[] x, ulong[] precomp, ulong[] zz)
	{
		ulong[] array = Nat576.CreateExt64();
		ImplMultiplyPrecomp(x, precomp, array);
		AddExt(zz, array, zz);
	}

	public static ulong[] PrecompMultiplicand(ulong[] x)
	{
		int num = 144;
		ulong[] array = new ulong[num << 1];
		Array.Copy(x, 0, array, 9, 9);
		int num2 = 0;
		for (int num3 = 7; num3 > 0; num3--)
		{
			num2 += 18;
			Nat.ShiftUpBit64(9, array, num2 >> 1, 0uL, array, num2);
			Reduce5(array, num2);
			Add(array, 9, array, num2, array, num2 + 9);
		}
		Nat.ShiftUpBits64(num, array, 0, 4, 0uL, array, num);
		return array;
	}

	public static void Reduce(ulong[] xx, ulong[] z)
	{
		ulong num = xx[9];
		ulong num2 = xx[17];
		ulong num3 = num;
		num = num3 ^ (num2 >> 59) ^ (num2 >> 57) ^ (num2 >> 54) ^ (num2 >> 49);
		num3 = xx[8] ^ (num2 << 5) ^ (num2 << 7) ^ (num2 << 10) ^ (num2 << 15);
		for (int num4 = 16; num4 >= 10; num4--)
		{
			num2 = xx[num4];
			z[num4 - 8] = num3 ^ (num2 >> 59) ^ (num2 >> 57) ^ (num2 >> 54) ^ (num2 >> 49);
			num3 = xx[num4 - 9] ^ (num2 << 5) ^ (num2 << 7) ^ (num2 << 10) ^ (num2 << 15);
		}
		num2 = num;
		z[1] = num3 ^ (num2 >> 59) ^ (num2 >> 57) ^ (num2 >> 54) ^ (num2 >> 49);
		num3 = xx[0] ^ (num2 << 5) ^ (num2 << 7) ^ (num2 << 10) ^ (num2 << 15);
		ulong num5 = z[8];
		ulong num6 = num5 >> 59;
		z[0] = num3 ^ num6 ^ (num6 << 2) ^ (num6 << 5) ^ (num6 << 10);
		z[8] = num5 & 0x7FFFFFFFFFFFFFFuL;
	}

	public static void Reduce5(ulong[] z, int zOff)
	{
		ulong num = z[zOff + 8];
		ulong num2 = num >> 59;
		ulong[] array;
		ulong[] array2 = (array = z);
		nint num3 = zOff;
		array2[zOff] = array[num3] ^ (num2 ^ (num2 << 2) ^ (num2 << 5) ^ (num2 << 10));
		z[zOff + 8] = num & 0x7FFFFFFFFFFFFFFuL;
	}

	public static void Sqrt(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat576.Create64();
		ulong[] array2 = Nat576.Create64();
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			ulong num2 = Interleave.Unshuffle(x[num++]);
			ulong num3 = Interleave.Unshuffle(x[num++]);
			array[i] = (num2 & 0xFFFFFFFFu) | (num3 << 32);
			array2[i] = (num2 >> 32) | (num3 & 0xFFFFFFFF00000000uL);
		}
		ulong num4 = Interleave.Unshuffle(x[num]);
		array[4] = num4 & 0xFFFFFFFFu;
		array2[4] = num4 >> 32;
		Multiply(array2, ROOT_Z, z);
		Add(z, array, z);
	}

	public static void Square(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat576.CreateExt64();
		ImplSquare(x, array);
		Reduce(array, z);
	}

	public static void SquareAddToExt(ulong[] x, ulong[] zz)
	{
		ulong[] array = Nat576.CreateExt64();
		ImplSquare(x, array);
		AddExt(zz, array, zz);
	}

	public static void SquareN(ulong[] x, int n, ulong[] z)
	{
		ulong[] array = Nat576.CreateExt64();
		ImplSquare(x, array);
		Reduce(array, z);
		while (--n > 0)
		{
			ImplSquare(z, array);
			Reduce(array, z);
		}
	}

	public static uint Trace(ulong[] x)
	{
		return (uint)(int)(x[0] ^ (x[8] >> 49) ^ (x[8] >> 57)) & 1u;
	}

	protected static void ImplMultiply(ulong[] x, ulong[] y, ulong[] zz)
	{
		ulong[] u = new ulong[16];
		for (int i = 0; i < 9; i++)
		{
			ImplMulwAcc(u, x[i], y[i], zz, i << 1);
		}
		ulong num = zz[0];
		ulong num2 = zz[1];
		num ^= zz[2];
		zz[1] = num ^ num2;
		num2 ^= zz[3];
		num ^= zz[4];
		zz[2] = num ^ num2;
		num2 ^= zz[5];
		num ^= zz[6];
		zz[3] = num ^ num2;
		num2 ^= zz[7];
		num ^= zz[8];
		zz[4] = num ^ num2;
		num2 ^= zz[9];
		num ^= zz[10];
		zz[5] = num ^ num2;
		num2 ^= zz[11];
		num ^= zz[12];
		zz[6] = num ^ num2;
		num2 ^= zz[13];
		num ^= zz[14];
		zz[7] = num ^ num2;
		num2 ^= zz[15];
		num ^= zz[16];
		zz[8] = num ^ num2;
		num2 ^= zz[17];
		ulong num3 = num ^ num2;
		zz[9] = zz[0] ^ num3;
		zz[10] = zz[1] ^ num3;
		zz[11] = zz[2] ^ num3;
		zz[12] = zz[3] ^ num3;
		zz[13] = zz[4] ^ num3;
		zz[14] = zz[5] ^ num3;
		zz[15] = zz[6] ^ num3;
		zz[16] = zz[7] ^ num3;
		zz[17] = zz[8] ^ num3;
		ImplMulwAcc(u, x[0] ^ x[1], y[0] ^ y[1], zz, 1);
		ImplMulwAcc(u, x[0] ^ x[2], y[0] ^ y[2], zz, 2);
		ImplMulwAcc(u, x[0] ^ x[3], y[0] ^ y[3], zz, 3);
		ImplMulwAcc(u, x[1] ^ x[2], y[1] ^ y[2], zz, 3);
		ImplMulwAcc(u, x[0] ^ x[4], y[0] ^ y[4], zz, 4);
		ImplMulwAcc(u, x[1] ^ x[3], y[1] ^ y[3], zz, 4);
		ImplMulwAcc(u, x[0] ^ x[5], y[0] ^ y[5], zz, 5);
		ImplMulwAcc(u, x[1] ^ x[4], y[1] ^ y[4], zz, 5);
		ImplMulwAcc(u, x[2] ^ x[3], y[2] ^ y[3], zz, 5);
		ImplMulwAcc(u, x[0] ^ x[6], y[0] ^ y[6], zz, 6);
		ImplMulwAcc(u, x[1] ^ x[5], y[1] ^ y[5], zz, 6);
		ImplMulwAcc(u, x[2] ^ x[4], y[2] ^ y[4], zz, 6);
		ImplMulwAcc(u, x[0] ^ x[7], y[0] ^ y[7], zz, 7);
		ImplMulwAcc(u, x[1] ^ x[6], y[1] ^ y[6], zz, 7);
		ImplMulwAcc(u, x[2] ^ x[5], y[2] ^ y[5], zz, 7);
		ImplMulwAcc(u, x[3] ^ x[4], y[3] ^ y[4], zz, 7);
		ImplMulwAcc(u, x[0] ^ x[8], y[0] ^ y[8], zz, 8);
		ImplMulwAcc(u, x[1] ^ x[7], y[1] ^ y[7], zz, 8);
		ImplMulwAcc(u, x[2] ^ x[6], y[2] ^ y[6], zz, 8);
		ImplMulwAcc(u, x[3] ^ x[5], y[3] ^ y[5], zz, 8);
		ImplMulwAcc(u, x[1] ^ x[8], y[1] ^ y[8], zz, 9);
		ImplMulwAcc(u, x[2] ^ x[7], y[2] ^ y[7], zz, 9);
		ImplMulwAcc(u, x[3] ^ x[6], y[3] ^ y[6], zz, 9);
		ImplMulwAcc(u, x[4] ^ x[5], y[4] ^ y[5], zz, 9);
		ImplMulwAcc(u, x[2] ^ x[8], y[2] ^ y[8], zz, 10);
		ImplMulwAcc(u, x[3] ^ x[7], y[3] ^ y[7], zz, 10);
		ImplMulwAcc(u, x[4] ^ x[6], y[4] ^ y[6], zz, 10);
		ImplMulwAcc(u, x[3] ^ x[8], y[3] ^ y[8], zz, 11);
		ImplMulwAcc(u, x[4] ^ x[7], y[4] ^ y[7], zz, 11);
		ImplMulwAcc(u, x[5] ^ x[6], y[5] ^ y[6], zz, 11);
		ImplMulwAcc(u, x[4] ^ x[8], y[4] ^ y[8], zz, 12);
		ImplMulwAcc(u, x[5] ^ x[7], y[5] ^ y[7], zz, 12);
		ImplMulwAcc(u, x[5] ^ x[8], y[5] ^ y[8], zz, 13);
		ImplMulwAcc(u, x[6] ^ x[7], y[6] ^ y[7], zz, 13);
		ImplMulwAcc(u, x[6] ^ x[8], y[6] ^ y[8], zz, 14);
		ImplMulwAcc(u, x[7] ^ x[8], y[7] ^ y[8], zz, 15);
	}

	protected static void ImplMultiplyPrecomp(ulong[] x, ulong[] precomp, ulong[] zz)
	{
		uint num = 15u;
		for (int num2 = 56; num2 >= 0; num2 -= 8)
		{
			for (int i = 1; i < 9; i += 2)
			{
				uint num3 = (uint)(x[i] >> num2);
				uint num4 = num3 & num;
				uint num5 = (num3 >> 4) & num;
				AddBothTo(precomp, (int)(9 * num4), precomp, (int)(9 * (num5 + 16)), zz, i - 1);
			}
			Nat.ShiftUpBits64(16, zz, 0, 8, 0uL);
		}
		for (int num6 = 56; num6 >= 0; num6 -= 8)
		{
			for (int j = 0; j < 9; j += 2)
			{
				uint num7 = (uint)(x[j] >> num6);
				uint num8 = num7 & num;
				uint num9 = (num7 >> 4) & num;
				AddBothTo(precomp, (int)(9 * num8), precomp, (int)(9 * (num9 + 16)), zz, j);
			}
			if (num6 > 0)
			{
				Nat.ShiftUpBits64(18, zz, 0, 8, 0uL);
			}
		}
	}

	protected static void ImplMulwAcc(ulong[] u, ulong x, ulong y, ulong[] z, int zOff)
	{
		u[1] = y;
		for (int i = 2; i < 16; i += 2)
		{
			u[i] = u[i >> 1] << 1;
			u[i + 1] = u[i] ^ y;
		}
		uint num = (uint)x;
		ulong num2 = 0uL;
		ulong num3 = u[num & 0xF] ^ (u[(num >> 4) & 0xF] << 4);
		int num4 = 56;
		do
		{
			num = (uint)(x >> num4);
			ulong num5 = u[num & 0xF] ^ (u[(num >> 4) & 0xF] << 4);
			num3 ^= num5 << num4;
			num2 ^= num5 >> -num4;
		}
		while ((num4 -= 8) > 0);
		for (int j = 0; j < 7; j++)
		{
			x = (x & 0xFEFEFEFEFEFEFEFEuL) >> 1;
			num2 ^= x & (ulong)((long)(y << j) >> 63);
		}
		ulong[] array;
		ulong[] array2 = (array = z);
		nint num6 = zOff;
		array2[zOff] = array[num6] ^ num3;
		ulong[] array3 = (array = z);
		int num7 = zOff + 1;
		num6 = num7;
		array3[num7] = array[num6] ^ num2;
	}

	protected static void ImplSquare(ulong[] x, ulong[] zz)
	{
		Interleave.Expand64To128(x, 0, 9, zz, 0);
	}
}
