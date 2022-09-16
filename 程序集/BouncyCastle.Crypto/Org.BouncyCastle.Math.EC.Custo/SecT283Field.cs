using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT283Field
{
	private const ulong M27 = 134217727uL;

	private const ulong M57 = 144115188075855871uL;

	private static readonly ulong[] ROOT_Z = new ulong[5] { 878416384462358536uL, 3513665537849438403uL, 9369774767598502668uL, 585610922974906400uL, 34087042uL };

	public static void Add(ulong[] x, ulong[] y, ulong[] z)
	{
		z[0] = x[0] ^ y[0];
		z[1] = x[1] ^ y[1];
		z[2] = x[2] ^ y[2];
		z[3] = x[3] ^ y[3];
		z[4] = x[4] ^ y[4];
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
		zz[7] = xx[7] ^ yy[7];
		zz[8] = xx[8] ^ yy[8];
	}

	public static void AddOne(ulong[] x, ulong[] z)
	{
		z[0] = x[0] ^ 1;
		z[1] = x[1];
		z[2] = x[2];
		z[3] = x[3];
		z[4] = x[4];
	}

	private static void AddTo(ulong[] x, ulong[] z)
	{
		ulong[] array;
		(array = z)[0] = array[0] ^ x[0];
		(array = z)[1] = array[1] ^ x[1];
		(array = z)[2] = array[2] ^ x[2];
		(array = z)[3] = array[3] ^ x[3];
		(array = z)[4] = array[4] ^ x[4];
	}

	public static ulong[] FromBigInteger(BigInteger x)
	{
		return Nat.FromBigInteger64(283, x);
	}

	public static void HalfTrace(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat.Create64(9);
		Nat320.Copy64(x, z);
		for (int i = 1; i < 283; i += 2)
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
		if (Nat320.IsZero64(x))
		{
			throw new InvalidOperationException();
		}
		ulong[] array = Nat320.Create64();
		ulong[] array2 = Nat320.Create64();
		Square(x, array);
		Multiply(array, x, array);
		SquareN(array, 2, array2);
		Multiply(array2, array, array2);
		SquareN(array2, 4, array);
		Multiply(array, array2, array);
		SquareN(array, 8, array2);
		Multiply(array2, array, array2);
		Square(array2, array2);
		Multiply(array2, x, array2);
		SquareN(array2, 17, array);
		Multiply(array, array2, array);
		Square(array, array);
		Multiply(array, x, array);
		SquareN(array, 35, array2);
		Multiply(array2, array, array2);
		SquareN(array2, 70, array);
		Multiply(array, array2, array);
		Square(array, array);
		Multiply(array, x, array);
		SquareN(array, 141, array2);
		Multiply(array2, array, array2);
		Square(array2, z);
	}

	public static void Multiply(ulong[] x, ulong[] y, ulong[] z)
	{
		ulong[] array = Nat320.CreateExt64();
		ImplMultiply(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(ulong[] x, ulong[] y, ulong[] zz)
	{
		ulong[] array = Nat320.CreateExt64();
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
		ulong num8 = xx[7];
		ulong num9 = xx[8];
		num4 ^= (num9 << 37) ^ (num9 << 42) ^ (num9 << 44) ^ (num9 << 49);
		num5 ^= (num9 >> 27) ^ (num9 >> 22) ^ (num9 >> 20) ^ (num9 >> 15);
		num3 ^= (num8 << 37) ^ (num8 << 42) ^ (num8 << 44) ^ (num8 << 49);
		num4 ^= (num8 >> 27) ^ (num8 >> 22) ^ (num8 >> 20) ^ (num8 >> 15);
		num2 ^= (num7 << 37) ^ (num7 << 42) ^ (num7 << 44) ^ (num7 << 49);
		num3 ^= (num7 >> 27) ^ (num7 >> 22) ^ (num7 >> 20) ^ (num7 >> 15);
		num ^= (num6 << 37) ^ (num6 << 42) ^ (num6 << 44) ^ (num6 << 49);
		num2 ^= (num6 >> 27) ^ (num6 >> 22) ^ (num6 >> 20) ^ (num6 >> 15);
		ulong num10 = num5 >> 27;
		z[0] = num ^ num10 ^ (num10 << 5) ^ (num10 << 7) ^ (num10 << 12);
		z[1] = num2;
		z[2] = num3;
		z[3] = num4;
		z[4] = num5 & 0x7FFFFFF;
	}

	public static void Reduce37(ulong[] z, int zOff)
	{
		ulong num = z[zOff + 4];
		ulong num2 = num >> 27;
		ulong[] array;
		ulong[] array2 = (array = z);
		nint num3 = zOff;
		array2[zOff] = array[num3] ^ (num2 ^ (num2 << 5) ^ (num2 << 7) ^ (num2 << 12));
		z[zOff + 4] = num & 0x7FFFFFF;
	}

	public static void Sqrt(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat320.Create64();
		ulong num = Interleave.Unshuffle(x[0]);
		ulong num2 = Interleave.Unshuffle(x[1]);
		ulong num3 = (num & 0xFFFFFFFFu) | (num2 << 32);
		array[0] = (num >> 32) | (num2 & 0xFFFFFFFF00000000uL);
		num = Interleave.Unshuffle(x[2]);
		num2 = Interleave.Unshuffle(x[3]);
		ulong num4 = (num & 0xFFFFFFFFu) | (num2 << 32);
		array[1] = (num >> 32) | (num2 & 0xFFFFFFFF00000000uL);
		num = Interleave.Unshuffle(x[4]);
		ulong num5 = num & 0xFFFFFFFFu;
		array[2] = num >> 32;
		Multiply(array, ROOT_Z, z);
		ulong[] array2;
		(array2 = z)[0] = array2[0] ^ num3;
		(array2 = z)[1] = array2[1] ^ num4;
		(array2 = z)[2] = array2[2] ^ num5;
	}

	public static void Square(ulong[] x, ulong[] z)
	{
		ulong[] array = Nat.Create64(9);
		ImplSquare(x, array);
		Reduce(array, z);
	}

	public static void SquareAddToExt(ulong[] x, ulong[] zz)
	{
		ulong[] array = Nat.Create64(9);
		ImplSquare(x, array);
		AddExt(zz, array, zz);
	}

	public static void SquareN(ulong[] x, int n, ulong[] z)
	{
		ulong[] array = Nat.Create64(9);
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
		return (uint)(int)(x[0] ^ (x[4] >> 15)) & 1u;
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
		ulong num9 = zz[8];
		ulong num10 = zz[9];
		zz[0] = num ^ (num2 << 57);
		zz[1] = (num2 >> 7) ^ (num3 << 50);
		zz[2] = (num3 >> 14) ^ (num4 << 43);
		zz[3] = (num4 >> 21) ^ (num5 << 36);
		zz[4] = (num5 >> 28) ^ (num6 << 29);
		zz[5] = (num6 >> 35) ^ (num7 << 22);
		zz[6] = (num7 >> 42) ^ (num8 << 15);
		zz[7] = (num8 >> 49) ^ (num9 << 8);
		zz[8] = (num9 >> 56) ^ (num10 << 1);
		zz[9] = num10 >> 63;
	}

	protected static void ImplExpand(ulong[] x, ulong[] z)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = x[2];
		ulong num4 = x[3];
		ulong num5 = x[4];
		z[0] = num & 0x1FFFFFFFFFFFFFFuL;
		z[1] = ((num >> 57) ^ (num2 << 7)) & 0x1FFFFFFFFFFFFFFuL;
		z[2] = ((num2 >> 50) ^ (num3 << 14)) & 0x1FFFFFFFFFFFFFFuL;
		z[3] = ((num3 >> 43) ^ (num4 << 21)) & 0x1FFFFFFFFFFFFFFuL;
		z[4] = (num4 >> 36) ^ (num5 << 28);
	}

	protected static void ImplMultiply(ulong[] x, ulong[] y, ulong[] zz)
	{
		ulong[] array = new ulong[5];
		ulong[] array2 = new ulong[5];
		ImplExpand(x, array);
		ImplExpand(y, array2);
		ulong[] array3 = new ulong[26];
		ImplMulw(zz, array[0], array2[0], array3, 0);
		ImplMulw(zz, array[1], array2[1], array3, 2);
		ImplMulw(zz, array[2], array2[2], array3, 4);
		ImplMulw(zz, array[3], array2[3], array3, 6);
		ImplMulw(zz, array[4], array2[4], array3, 8);
		ulong num = array[0] ^ array[1];
		ulong num2 = array2[0] ^ array2[1];
		ulong num3 = array[0] ^ array[2];
		ulong num4 = array2[0] ^ array2[2];
		ulong num5 = array[2] ^ array[4];
		ulong num6 = array2[2] ^ array2[4];
		ulong num7 = array[3] ^ array[4];
		ulong num8 = array2[3] ^ array2[4];
		ImplMulw(zz, num3 ^ array[3], num4 ^ array2[3], array3, 18);
		ImplMulw(zz, num5 ^ array[1], num6 ^ array2[1], array3, 20);
		ulong num9 = num ^ num7;
		ulong num10 = num2 ^ num8;
		ulong x2 = num9 ^ array[2];
		ulong y2 = num10 ^ array2[2];
		ImplMulw(zz, num9, num10, array3, 22);
		ImplMulw(zz, x2, y2, array3, 24);
		ImplMulw(zz, num, num2, array3, 10);
		ImplMulw(zz, num3, num4, array3, 12);
		ImplMulw(zz, num5, num6, array3, 14);
		ImplMulw(zz, num7, num8, array3, 16);
		zz[0] = array3[0];
		zz[9] = array3[9];
		ulong num11 = array3[0] ^ array3[1];
		ulong num12 = num11 ^ array3[2];
		ulong num13 = (zz[1] = num12 ^ array3[10]);
		ulong num14 = array3[3] ^ array3[4];
		ulong num15 = array3[11] ^ array3[12];
		ulong num16 = num14 ^ num15;
		ulong num17 = (zz[2] = num12 ^ num16);
		ulong num18 = num11 ^ num14;
		ulong num19 = array3[5] ^ array3[6];
		ulong num20 = num18 ^ num19;
		ulong num21 = num20 ^ array3[8];
		ulong num22 = array3[13] ^ array3[14];
		ulong num23 = num21 ^ num22;
		ulong num24 = array3[18] ^ array3[22];
		ulong num25 = num24 ^ array3[24];
		ulong num26 = (zz[3] = num23 ^ num25);
		ulong num27 = array3[7] ^ array3[8];
		ulong num28 = num27 ^ array3[9];
		ulong num29 = (zz[8] = num28 ^ array3[17]);
		ulong num30 = num28 ^ num19;
		ulong num31 = array3[15] ^ array3[16];
		ulong num32 = (zz[7] = num30 ^ num31) ^ num13;
		ulong num33 = array3[19] ^ array3[20];
		ulong num34 = array3[25] ^ array3[24];
		ulong num35 = array3[18] ^ array3[23];
		ulong num36 = num33 ^ num34;
		ulong num37 = num36 ^ num35;
		ulong num38 = (zz[4] = num37 ^ num32);
		ulong num39 = num17 ^ num29;
		ulong num40 = num36 ^ num39;
		ulong num41 = array3[21] ^ array3[22];
		ulong num42 = (zz[5] = num40 ^ num41);
		ulong num43 = num21 ^ array3[0];
		ulong num44 = num43 ^ array3[9];
		ulong num45 = num44 ^ num22;
		ulong num46 = num45 ^ array3[21];
		ulong num47 = num46 ^ array3[23];
		ulong num48 = (zz[6] = num47 ^ array3[25]);
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
		ulong num3 = u[num & 7];
		int num4 = 48;
		do
		{
			num = (uint)(x >> num4);
			ulong num5 = u[num & 7] ^ (u[(num >> 3) & 7] << 3) ^ (u[(num >> 6) & 7] << 6);
			num3 ^= num5 << num4;
			num2 ^= num5 >> -num4;
		}
		while ((num4 -= 9) > 0);
		num2 ^= (x & 0x100804020100800L & (ulong)((long)(y << 7) >> 63)) >> 8;
		z[zOff] = num3 & 0x1FFFFFFFFFFFFFFuL;
		z[zOff + 1] = (num3 >> 57) ^ (num2 << 7);
	}

	protected static void ImplSquare(ulong[] x, ulong[] zz)
	{
		Interleave.Expand64To128(x, 0, 4, zz, 0);
		zz[8] = Interleave.Expand32to64((uint)x[4]);
	}
}
