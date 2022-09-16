using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT193Field
{
	private const ulong M01 = 1uL;

	private const ulong M49 = 562949953421311uL;

	public static void Add(ulong[] x, ulong[] y, ulong[] z)
	{
		z[0] = x[0] ^ y[0];
		z[1] = x[1] ^ y[1];
		z[2] = x[2] ^ y[2];
		z[3] = x[3] ^ y[3];
	}

	public static void AddExt(ulong[] xx, ulong[] yy, ulong[] zz)
	{
		zz[0] = xx[0] ^ yy[0];
		zz[1] = xx[1] ^ yy[1];
		zz[2] = xx[2] ^ yy[2];
		zz[3] = xx[3] ^ yy[3];
		zz[4] = xx[4] ^ yy[4];
		zz[5] = xx[5] ^ yy[5];
		zz[6] = xx[6] ^ yy[6];
	}

	public static void AddOne(ulong[] x, ulong[] z)
	{
		z[0] = x[0] ^ 1;
		z[1] = x[1];
		z[2] = x[2];
		z[3] = x[3];
	}

	private static void AddTo(ulong[] x, ulong[] z)
	{
		ulong[] array;
		(array = z)[0] = array[0] ^ x[0];
		(array = z)[1] = array[1] ^ x[1];
		(array = z)[2] = array[2] ^ x[2];
		(array = z)[3] = array[3] ^ x[3];
	}

	public static ulong[] FromBigInteger(BigInteger x)
	{
		return Nat.FromBigInteger64(193, x);
	}

	public static void HalfTrace(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat256.CreateExt64();
		Nat256.Copy64(x, z);
		for (int i = 1; i < 193; i += 2)
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
		if (Nat256.IsZero64(x))
		{
			throw new InvalidOperationException();
		}
		ulong[] array = Nat256.Create64();
		ulong[] array2 = Nat256.Create64();
		Square(x, array);
		SquareN(array, 1, array2);
		Multiply(array, array2, array);
		SquareN(array2, 1, array2);
		Multiply(array, array2, array);
		SquareN(array, 3, array2);
		Multiply(array, array2, array);
		SquareN(array, 6, array2);
		Multiply(array, array2, array);
		SquareN(array, 12, array2);
		Multiply(array, array2, array);
		SquareN(array, 24, array2);
		Multiply(array, array2, array);
		SquareN(array, 48, array2);
		Multiply(array, array2, array);
		SquareN(array, 96, array2);
		Multiply(array, array2, z);
	}

	public static void Multiply(ulong[] x, ulong[] y, ulong[] z)
	{
		ulong[] array = Nat256.CreateExt64();
		ImplMultiply(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(ulong[] x, ulong[] y, ulong[] zz)
	{
		ulong[] array = Nat256.CreateExt64();
		ImplMultiply(x, y, array);
		AddExt(zz, array, zz);
	}

	public static void Reduce(ulong[] xx, ulong[] z)
	{
		ulong num = xx[0];
		ulong num2 = xx[1];
		ulong num3 = xx[2];
		ulong num4 = xx[3];
		ulong num5 = xx[4];
		ulong num6 = xx[5];
		ulong num7 = xx[6];
		num3 ^= num7 << 63;
		num4 ^= (num7 >> 1) ^ (num7 << 14);
		num5 ^= num7 >> 50;
		num2 ^= num6 << 63;
		num3 ^= (num6 >> 1) ^ (num6 << 14);
		num4 ^= num6 >> 50;
		num ^= num5 << 63;
		num2 ^= (num5 >> 1) ^ (num5 << 14);
		num3 ^= num5 >> 50;
		ulong num8 = num4 >> 1;
		z[0] = num ^ num8 ^ (num8 << 15);
		z[1] = num2 ^ (num8 >> 49);
		z[2] = num3;
		z[3] = num4 & 1;
	}

	public static void Reduce63(ulong[] z, int zOff)
	{
		ulong num = z[zOff + 3];
		ulong num2 = num >> 1;
		ulong[] array;
		ulong[] array2 = (array = z);
		nint num3 = zOff;
		array2[zOff] = array[num3] ^ (num2 ^ (num2 << 15));
		ulong[] array3 = (array = z);
		int num4 = zOff + 1;
		num3 = num4;
		array3[num4] = array[num3] ^ (num2 >> 49);
		z[zOff + 3] = num & 1;
	}

	public static void Sqrt(ulong[] x, ulong[] z)
	{
		ulong num = Interleave.Unshuffle(x[0]);
		ulong num2 = Interleave.Unshuffle(x[1]);
		ulong num3 = (num & 0xFFFFFFFFu) | (num2 << 32);
		ulong num4 = (num >> 32) | (num2 & 0xFFFFFFFF00000000uL);
		num = Interleave.Unshuffle(x[2]);
		ulong num5 = (num & 0xFFFFFFFFu) ^ (x[3] << 32);
		ulong num6 = num >> 32;
		z[0] = num3 ^ (num4 << 8);
		z[1] = num5 ^ (num6 << 8) ^ (num4 >> 56) ^ (num4 << 33);
		z[2] = (num6 >> 56) ^ (num6 << 33) ^ (num4 >> 31);
		z[3] = num6 >> 31;
	}

	public static void Square(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat256.CreateExt64();
		ImplSquare(x, array);
		Reduce(array, z);
	}

	public static void SquareAddToExt(ulong[] x, ulong[] zz)
	{
		ulong[] array = Nat256.CreateExt64();
		ImplSquare(x, array);
		AddExt(zz, array, zz);
	}

	public static void SquareN(ulong[] x, int n, ulong[] z)
	{
		ulong[] array = Nat256.CreateExt64();
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
		return (uint)(int)x[0] & 1u;
	}

	protected static void ImplCompactExt(ulong[] zz)
	{
		ulong num = zz[0];
		ulong num2 = zz[1];
		ulong num3 = zz[2];
		ulong num4 = zz[3];
		ulong num5 = zz[4];
		ulong num6 = zz[5];
		ulong num7 = zz[6];
		ulong num8 = zz[7];
		zz[0] = num ^ (num2 << 49);
		zz[1] = (num2 >> 15) ^ (num3 << 34);
		zz[2] = (num3 >> 30) ^ (num4 << 19);
		zz[3] = (num4 >> 45) ^ (num5 << 4) ^ (num6 << 53);
		zz[4] = (num5 >> 60) ^ (num7 << 38) ^ (num6 >> 11);
		zz[5] = (num7 >> 26) ^ (num8 << 23);
		zz[6] = num8 >> 41;
		zz[7] = 0uL;
	}

	protected static void ImplExpand(ulong[] x, ulong[] z)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = x[2];
		ulong num4 = x[3];
		z[0] = num & 0x1FFFFFFFFFFFFuL;
		z[1] = ((num >> 49) ^ (num2 << 15)) & 0x1FFFFFFFFFFFFuL;
		z[2] = ((num2 >> 34) ^ (num3 << 30)) & 0x1FFFFFFFFFFFFuL;
		z[3] = (num3 >> 19) ^ (num4 << 45);
	}

	protected static void ImplMultiply(ulong[] x, ulong[] y, ulong[] zz)
	{
		ulong[] array = new ulong[4];
		ulong[] array2 = new ulong[4];
		ImplExpand(x, array);
		ImplExpand(y, array2);
		ulong[] u = new ulong[8];
		ImplMulwAcc(u, array[0], array2[0], zz, 0);
		ImplMulwAcc(u, array[1], array2[1], zz, 1);
		ImplMulwAcc(u, array[2], array2[2], zz, 2);
		ImplMulwAcc(u, array[3], array2[3], zz, 3);
		ulong[] array3;
		for (int num = 5; num > 0; num--)
		{
			ulong[] array4 = (array3 = zz);
			int num2 = num;
			nint num3 = num2;
			array4[num2] = array3[num3] ^ zz[num - 1];
		}
		ImplMulwAcc(u, array[0] ^ array[1], array2[0] ^ array2[1], zz, 1);
		ImplMulwAcc(u, array[2] ^ array[3], array2[2] ^ array2[3], zz, 3);
		for (int num4 = 7; num4 > 1; num4--)
		{
			ulong[] array5 = (array3 = zz);
			int num5 = num4;
			nint num3 = num5;
			array5[num5] = array3[num3] ^ zz[num4 - 2];
		}
		ulong num6 = array[0] ^ array[2];
		ulong num7 = array[1] ^ array[3];
		ulong num8 = array2[0] ^ array2[2];
		ulong num9 = array2[1] ^ array2[3];
		ImplMulwAcc(u, num6 ^ num7, num8 ^ num9, zz, 3);
		ulong[] array6 = new ulong[3];
		ImplMulwAcc(u, num6, num8, array6, 0);
		ImplMulwAcc(u, num7, num9, array6, 1);
		ulong num10 = array6[0];
		ulong num11 = array6[1];
		ulong num12 = array6[2];
		(array3 = zz)[2] = array3[2] ^ num10;
		(array3 = zz)[3] = array3[3] ^ (num10 ^ num11);
		(array3 = zz)[4] = array3[4] ^ (num12 ^ num11);
		(array3 = zz)[5] = array3[5] ^ num12;
		ImplCompactExt(zz);
	}

	protected static void ImplMulwAcc(ulong[] u, ulong x, ulong y, ulong[] z, int zOff)
	{
		u[1] = y;
		u[2] = u[1] << 1;
		u[3] = u[2] ^ y;
		u[4] = u[2] << 1;
		u[5] = u[4] ^ y;
		u[6] = u[3] << 1;
		u[7] = u[6] ^ y;
		uint num = (uint)x;
		ulong num2 = 0uL;
		ulong num3 = u[num & 7] ^ (u[(num >> 3) & 7] << 3);
		int num4 = 36;
		do
		{
			num = (uint)(x >> num4);
			ulong num5 = u[num & 7] ^ (u[(num >> 3) & 7] << 3) ^ (u[(num >> 6) & 7] << 6) ^ (u[(num >> 9) & 7] << 9) ^ (u[(num >> 12) & 7] << 12);
			num3 ^= num5 << num4;
			num2 ^= num5 >> -num4;
		}
		while ((num4 -= 15) > 0);
		ulong[] array;
		ulong[] array2 = (array = z);
		nint num6 = zOff;
		array2[zOff] = array[num6] ^ (num3 & 0x1FFFFFFFFFFFFuL);
		ulong[] array3 = (array = z);
		int num7 = zOff + 1;
		num6 = num7;
		array3[num7] = array[num6] ^ ((num3 >> 49) ^ (num2 << 15));
	}

	protected static void ImplSquare(ulong[] x, ulong[] zz)
	{
		Interleave.Expand64To128(x, 0, 3, zz, 0);
		zz[6] = x[3] & 1;
	}
}
