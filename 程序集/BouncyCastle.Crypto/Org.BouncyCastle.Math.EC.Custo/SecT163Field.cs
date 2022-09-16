using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT163Field
{
	private const ulong M35 = 34359738367uL;

	private const ulong M55 = 36028797018963967uL;

	private static readonly ulong[] ROOT_Z = new ulong[3] { 13176245766935393968uL, 5270498306774195053uL, 19634136210uL };

	public static void Add(ulong[] x, ulong[] y, ulong[] z)
	{
		z[0] = x[0] ^ y[0];
		z[1] = x[1] ^ y[1];
		z[2] = x[2] ^ y[2];
	}

	public static void AddExt(ulong[] xx, ulong[] yy, ulong[] zz)
	{
		zz[0] = xx[0] ^ yy[0];
		zz[1] = xx[1] ^ yy[1];
		zz[2] = xx[2] ^ yy[2];
		zz[3] = xx[3] ^ yy[3];
		zz[4] = xx[4] ^ yy[4];
		zz[5] = xx[5] ^ yy[5];
	}

	public static void AddOne(ulong[] x, ulong[] z)
	{
		z[0] = x[0] ^ 1;
		z[1] = x[1];
		z[2] = x[2];
	}

	private static void AddTo(ulong[] x, ulong[] z)
	{
		ulong[] array;
		(array = z)[0] = array[0] ^ x[0];
		(array = z)[1] = array[1] ^ x[1];
		(array = z)[2] = array[2] ^ x[2];
	}

	public static ulong[] FromBigInteger(BigInteger x)
	{
		return Nat.FromBigInteger64(163, x);
	}

	public static void HalfTrace(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat192.CreateExt64();
		Nat192.Copy64(x, z);
		for (int i = 1; i < 163; i += 2)
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
		if (Nat192.IsZero64(x))
		{
			throw new InvalidOperationException();
		}
		ulong[] array = Nat192.Create64();
		ulong[] array2 = Nat192.Create64();
		Square(x, array);
		SquareN(array, 1, array2);
		Multiply(array, array2, array);
		SquareN(array2, 1, array2);
		Multiply(array, array2, array);
		SquareN(array, 3, array2);
		Multiply(array, array2, array);
		SquareN(array2, 3, array2);
		Multiply(array, array2, array);
		SquareN(array, 9, array2);
		Multiply(array, array2, array);
		SquareN(array2, 9, array2);
		Multiply(array, array2, array);
		SquareN(array, 27, array2);
		Multiply(array, array2, array);
		SquareN(array2, 27, array2);
		Multiply(array, array2, array);
		SquareN(array, 81, array2);
		Multiply(array, array2, z);
	}

	public static void Multiply(ulong[] x, ulong[] y, ulong[] z)
	{
		ulong[] array = new ulong[8];
		ImplMultiply(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(ulong[] x, ulong[] y, ulong[] zz)
	{
		ulong[] array = new ulong[8];
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
		num3 ^= (num6 << 29) ^ (num6 << 32) ^ (num6 << 35) ^ (num6 << 36);
		num4 ^= (num6 >> 35) ^ (num6 >> 32) ^ (num6 >> 29) ^ (num6 >> 28);
		num2 ^= (num5 << 29) ^ (num5 << 32) ^ (num5 << 35) ^ (num5 << 36);
		num3 ^= (num5 >> 35) ^ (num5 >> 32) ^ (num5 >> 29) ^ (num5 >> 28);
		num ^= (num4 << 29) ^ (num4 << 32) ^ (num4 << 35) ^ (num4 << 36);
		num2 ^= (num4 >> 35) ^ (num4 >> 32) ^ (num4 >> 29) ^ (num4 >> 28);
		ulong num7 = num3 >> 35;
		z[0] = num ^ num7 ^ (num7 << 3) ^ (num7 << 6) ^ (num7 << 7);
		z[1] = num2;
		z[2] = num3 & 0x7FFFFFFFFuL;
	}

	public static void Reduce29(ulong[] z, int zOff)
	{
		ulong num = z[zOff + 2];
		ulong num2 = num >> 35;
		ulong[] array;
		ulong[] array2 = (array = z);
		nint num3 = zOff;
		array2[zOff] = array[num3] ^ (num2 ^ (num2 << 3) ^ (num2 << 6) ^ (num2 << 7));
		z[zOff + 2] = num & 0x7FFFFFFFFuL;
	}

	public static void Sqrt(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat192.Create64();
		ulong num = Interleave.Unshuffle(x[0]);
		ulong num2 = Interleave.Unshuffle(x[1]);
		ulong num3 = (num & 0xFFFFFFFFu) | (num2 << 32);
		array[0] = (num >> 32) | (num2 & 0xFFFFFFFF00000000uL);
		num = Interleave.Unshuffle(x[2]);
		ulong num4 = num & 0xFFFFFFFFu;
		array[1] = num >> 32;
		Multiply(array, ROOT_Z, z);
		ulong[] array2;
		(array2 = z)[0] = array2[0] ^ num3;
		(array2 = z)[1] = array2[1] ^ num4;
	}

	public static void Square(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat192.CreateExt64();
		ImplSquare(x, array);
		Reduce(array, z);
	}

	public static void SquareAddToExt(ulong[] x, ulong[] zz)
	{
		ulong[] array = Nat192.CreateExt64();
		ImplSquare(x, array);
		AddExt(zz, array, zz);
	}

	public static void SquareN(ulong[] x, int n, ulong[] z)
	{
		ulong[] array = Nat192.CreateExt64();
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
		return (uint)(int)(x[0] ^ (x[2] >> 29)) & 1u;
	}

	protected static void ImplCompactExt(ulong[] zz)
	{
		ulong num = zz[0];
		ulong num2 = zz[1];
		ulong num3 = zz[2];
		ulong num4 = zz[3];
		ulong num5 = zz[4];
		ulong num6 = zz[5];
		zz[0] = num ^ (num2 << 55);
		zz[1] = (num2 >> 9) ^ (num3 << 46);
		zz[2] = (num3 >> 18) ^ (num4 << 37);
		zz[3] = (num4 >> 27) ^ (num5 << 28);
		zz[4] = (num5 >> 36) ^ (num6 << 19);
		zz[5] = num6 >> 45;
	}

	protected static void ImplMultiply(ulong[] x, ulong[] y, ulong[] zz)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = x[2];
		num3 = (num2 >> 46) ^ (num3 << 18);
		num2 = ((num >> 55) ^ (num2 << 9)) & 0x7FFFFFFFFFFFFFuL;
		num &= 0x7FFFFFFFFFFFFFuL;
		ulong num4 = y[0];
		ulong num5 = y[1];
		ulong num6 = y[2];
		num6 = (num5 >> 46) ^ (num6 << 18);
		num5 = ((num4 >> 55) ^ (num5 << 9)) & 0x7FFFFFFFFFFFFFuL;
		num4 &= 0x7FFFFFFFFFFFFFuL;
		ulong[] array = new ulong[10];
		ImplMulw(zz, num, num4, array, 0);
		ImplMulw(zz, num3, num6, array, 2);
		ulong num7 = num ^ num2 ^ num3;
		ulong num8 = num4 ^ num5 ^ num6;
		ImplMulw(zz, num7, num8, array, 4);
		ulong num9 = (num2 << 1) ^ (num3 << 2);
		ulong num10 = (num5 << 1) ^ (num6 << 2);
		ImplMulw(zz, num ^ num9, num4 ^ num10, array, 6);
		ImplMulw(zz, num7 ^ num9, num8 ^ num10, array, 8);
		ulong num11 = array[6] ^ array[8];
		ulong num12 = array[7] ^ array[9];
		ulong num13 = (num11 << 1) ^ array[6];
		ulong num14 = num11 ^ (num12 << 1) ^ array[7];
		ulong num15 = num12;
		ulong num16 = array[0];
		ulong num17 = array[1] ^ array[0] ^ array[4];
		ulong num18 = array[1] ^ array[5];
		ulong num19 = num16 ^ num13 ^ (array[2] << 4) ^ (array[2] << 1);
		ulong num20 = num17 ^ num14 ^ (array[3] << 4) ^ (array[3] << 1);
		ulong num21 = num18 ^ num15;
		num20 ^= num19 >> 55;
		num19 &= 0x7FFFFFFFFFFFFFuL;
		num21 ^= num20 >> 55;
		num20 &= 0x7FFFFFFFFFFFFFuL;
		num19 = (num19 >> 1) ^ ((num20 & 1) << 54);
		num20 = (num20 >> 1) ^ ((num21 & 1) << 54);
		num21 >>= 1;
		num19 ^= num19 << 1;
		num19 ^= num19 << 2;
		num19 ^= num19 << 4;
		num19 ^= num19 << 8;
		num19 ^= num19 << 16;
		num19 ^= num19 << 32;
		num19 &= 0x7FFFFFFFFFFFFFuL;
		num20 ^= num19 >> 54;
		num20 ^= num20 << 1;
		num20 ^= num20 << 2;
		num20 ^= num20 << 4;
		num20 ^= num20 << 8;
		num20 ^= num20 << 16;
		num20 ^= num20 << 32;
		num20 &= 0x7FFFFFFFFFFFFFuL;
		num21 ^= num20 >> 54;
		num21 ^= num21 << 1;
		num21 ^= num21 << 2;
		num21 ^= num21 << 4;
		num21 ^= num21 << 8;
		num21 ^= num21 << 16;
		num21 ^= num21 << 32;
		zz[0] = num16;
		zz[1] = num17 ^ num19 ^ array[2];
		zz[2] = num18 ^ num20 ^ num19 ^ array[3];
		zz[3] = num21 ^ num20;
		zz[4] = num21 ^ array[2];
		zz[5] = array[3];
		ImplCompactExt(zz);
	}

	protected static void ImplMulw(ulong[] u, ulong x, ulong y, ulong[] z, int zOff)
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
		ulong num3 = u[num & 3];
		int num4 = 47;
		do
		{
			num = (uint)(x >> num4);
			ulong num5 = u[num & 7] ^ (u[(num >> 3) & 7] << 3) ^ (u[(num >> 6) & 7] << 6);
			num3 ^= num5 << num4;
			num2 ^= num5 >> -num4;
		}
		while ((num4 -= 9) > 0);
		z[zOff] = num3 & 0x7FFFFFFFFFFFFFuL;
		z[zOff + 1] = (num3 >> 55) ^ (num2 << 9);
	}

	protected static void ImplSquare(ulong[] x, ulong[] zz)
	{
		Interleave.Expand64To128(x, 0, 3, zz, 0);
	}
}
