using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Rfc7748;

[CLSCompliant(false)]
public abstract class X448Field
{
	public const int Size = 16;

	private const uint M28 = 268435455u;

	private static readonly uint[] P32 = new uint[14]
	{
		4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967294u, 4294967295u, 4294967295u,
		4294967295u, 4294967295u, 4294967295u, 4294967295u
	};

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		for (int i = 0; i < 16; i++)
		{
			z[i] = x[i] + y[i];
		}
	}

	public static void AddOne(uint[] z)
	{
		uint[] array;
		(array = z)[0] = array[0] + 1;
	}

	public static void AddOne(uint[] z, int zOff)
	{
		uint[] array;
		uint[] array2 = (array = z);
		nint num = zOff;
		array2[zOff] = array[num] + 1;
	}

	public static void Carry(uint[] z)
	{
		uint num = z[0];
		uint num2 = z[1];
		uint num3 = z[2];
		uint num4 = z[3];
		uint num5 = z[4];
		uint num6 = z[5];
		uint num7 = z[6];
		uint num8 = z[7];
		uint num9 = z[8];
		uint num10 = z[9];
		uint num11 = z[10];
		uint num12 = z[11];
		uint num13 = z[12];
		uint num14 = z[13];
		uint num15 = z[14];
		uint num16 = z[15];
		num2 += num >> 28;
		num &= 0xFFFFFFFu;
		num6 += num5 >> 28;
		num5 &= 0xFFFFFFFu;
		num10 += num9 >> 28;
		num9 &= 0xFFFFFFFu;
		num14 += num13 >> 28;
		num13 &= 0xFFFFFFFu;
		num3 += num2 >> 28;
		num2 &= 0xFFFFFFFu;
		num7 += num6 >> 28;
		num6 &= 0xFFFFFFFu;
		num11 += num10 >> 28;
		num10 &= 0xFFFFFFFu;
		num15 += num14 >> 28;
		num14 &= 0xFFFFFFFu;
		num4 += num3 >> 28;
		num3 &= 0xFFFFFFFu;
		num8 += num7 >> 28;
		num7 &= 0xFFFFFFFu;
		num12 += num11 >> 28;
		num11 &= 0xFFFFFFFu;
		num16 += num15 >> 28;
		num15 &= 0xFFFFFFFu;
		uint num17 = num16 >> 28;
		num16 &= 0xFFFFFFFu;
		num += num17;
		num9 += num17;
		num5 += num4 >> 28;
		num4 &= 0xFFFFFFFu;
		num9 += num8 >> 28;
		num8 &= 0xFFFFFFFu;
		num13 += num12 >> 28;
		num12 &= 0xFFFFFFFu;
		num2 += num >> 28;
		num &= 0xFFFFFFFu;
		num6 += num5 >> 28;
		num5 &= 0xFFFFFFFu;
		num10 += num9 >> 28;
		num9 &= 0xFFFFFFFu;
		num14 += num13 >> 28;
		num13 &= 0xFFFFFFFu;
		z[0] = num;
		z[1] = num2;
		z[2] = num3;
		z[3] = num4;
		z[4] = num5;
		z[5] = num6;
		z[6] = num7;
		z[7] = num8;
		z[8] = num9;
		z[9] = num10;
		z[10] = num11;
		z[11] = num12;
		z[12] = num13;
		z[13] = num14;
		z[14] = num15;
		z[15] = num16;
	}

	public static void CMov(int cond, uint[] x, int xOff, uint[] z, int zOff)
	{
		for (int i = 0; i < 16; i++)
		{
			uint num = z[zOff + i];
			uint num2 = num ^ x[xOff + i];
			num = (z[zOff + i] = num ^ (num2 & (uint)cond));
		}
	}

	public static void CNegate(int negate, uint[] z)
	{
		uint[] array = Create();
		Sub(array, z, array);
		CMov(-negate, array, 0, z, 0);
	}

	public static void Copy(uint[] x, int xOff, uint[] z, int zOff)
	{
		for (int i = 0; i < 16; i++)
		{
			z[zOff + i] = x[xOff + i];
		}
	}

	public static uint[] Create()
	{
		return new uint[16];
	}

	public static uint[] CreateTable(int n)
	{
		return new uint[16 * n];
	}

	public static void CSwap(int swap, uint[] a, uint[] b)
	{
		uint num = (uint)(-swap);
		for (int i = 0; i < 16; i++)
		{
			uint num2 = a[i];
			uint num3 = b[i];
			uint num4 = num & (num2 ^ num3);
			a[i] = num2 ^ num4;
			b[i] = num3 ^ num4;
		}
	}

	public static void Decode(uint[] x, int xOff, uint[] z)
	{
		Decode224(x, xOff, z, 0);
		Decode224(x, xOff + 7, z, 8);
	}

	public static void Decode(byte[] x, int xOff, uint[] z)
	{
		Decode56(x, xOff, z, 0);
		Decode56(x, xOff + 7, z, 2);
		Decode56(x, xOff + 14, z, 4);
		Decode56(x, xOff + 21, z, 6);
		Decode56(x, xOff + 28, z, 8);
		Decode56(x, xOff + 35, z, 10);
		Decode56(x, xOff + 42, z, 12);
		Decode56(x, xOff + 49, z, 14);
	}

	private static void Decode224(uint[] x, int xOff, uint[] z, int zOff)
	{
		uint num = x[xOff];
		uint num2 = x[xOff + 1];
		uint num3 = x[xOff + 2];
		uint num4 = x[xOff + 3];
		uint num5 = x[xOff + 4];
		uint num6 = x[xOff + 5];
		uint num7 = x[xOff + 6];
		z[zOff] = num & 0xFFFFFFFu;
		z[zOff + 1] = ((num >> 28) | (num2 << 4)) & 0xFFFFFFFu;
		z[zOff + 2] = ((num2 >> 24) | (num3 << 8)) & 0xFFFFFFFu;
		z[zOff + 3] = ((num3 >> 20) | (num4 << 12)) & 0xFFFFFFFu;
		z[zOff + 4] = ((num4 >> 16) | (num5 << 16)) & 0xFFFFFFFu;
		z[zOff + 5] = ((num5 >> 12) | (num6 << 20)) & 0xFFFFFFFu;
		z[zOff + 6] = ((num6 >> 8) | (num7 << 24)) & 0xFFFFFFFu;
		z[zOff + 7] = num7 >> 4;
	}

	private static uint Decode24(byte[] bs, int off)
	{
		uint num = bs[off];
		num |= (uint)(bs[++off] << 8);
		return num | (uint)(bs[++off] << 16);
	}

	private static uint Decode32(byte[] bs, int off)
	{
		uint num = bs[off];
		num |= (uint)(bs[++off] << 8);
		num |= (uint)(bs[++off] << 16);
		return num | (uint)(bs[++off] << 24);
	}

	private static void Decode56(byte[] bs, int off, uint[] z, int zOff)
	{
		uint num = Decode32(bs, off);
		uint num2 = Decode24(bs, off + 4);
		z[zOff] = num & 0xFFFFFFFu;
		z[zOff + 1] = (num >> 28) | (num2 << 4);
	}

	public static void Encode(uint[] x, uint[] z, int zOff)
	{
		Encode224(x, 0, z, zOff);
		Encode224(x, 8, z, zOff + 7);
	}

	public static void Encode(uint[] x, byte[] z, int zOff)
	{
		Encode56(x, 0, z, zOff);
		Encode56(x, 2, z, zOff + 7);
		Encode56(x, 4, z, zOff + 14);
		Encode56(x, 6, z, zOff + 21);
		Encode56(x, 8, z, zOff + 28);
		Encode56(x, 10, z, zOff + 35);
		Encode56(x, 12, z, zOff + 42);
		Encode56(x, 14, z, zOff + 49);
	}

	private static void Encode224(uint[] x, int xOff, uint[] z, int zOff)
	{
		uint num = x[xOff];
		uint num2 = x[xOff + 1];
		uint num3 = x[xOff + 2];
		uint num4 = x[xOff + 3];
		uint num5 = x[xOff + 4];
		uint num6 = x[xOff + 5];
		uint num7 = x[xOff + 6];
		uint num8 = x[xOff + 7];
		z[zOff] = num | (num2 << 28);
		z[zOff + 1] = (num2 >> 4) | (num3 << 24);
		z[zOff + 2] = (num3 >> 8) | (num4 << 20);
		z[zOff + 3] = (num4 >> 12) | (num5 << 16);
		z[zOff + 4] = (num5 >> 16) | (num6 << 12);
		z[zOff + 5] = (num6 >> 20) | (num7 << 8);
		z[zOff + 6] = (num7 >> 24) | (num8 << 4);
	}

	private static void Encode24(uint n, byte[] bs, int off)
	{
		bs[off] = (byte)n;
		bs[++off] = (byte)(n >> 8);
		bs[++off] = (byte)(n >> 16);
	}

	private static void Encode32(uint n, byte[] bs, int off)
	{
		bs[off] = (byte)n;
		bs[++off] = (byte)(n >> 8);
		bs[++off] = (byte)(n >> 16);
		bs[++off] = (byte)(n >> 24);
	}

	private static void Encode56(uint[] x, int xOff, byte[] bs, int off)
	{
		uint num = x[xOff];
		uint num2 = x[xOff + 1];
		Encode32(num | (num2 << 28), bs, off);
		Encode24(num2 >> 4, bs, off + 4);
	}

	public static void Inv(uint[] x, uint[] z)
	{
		uint[] array = Create();
		uint[] array2 = new uint[14];
		Copy(x, 0, array, 0);
		Normalize(array);
		Encode(array, array2, 0);
		Mod.ModOddInverse(P32, array2, array2);
		Decode(array2, 0, z);
	}

	public static void InvVar(uint[] x, uint[] z)
	{
		uint[] array = Create();
		uint[] array2 = new uint[14];
		Copy(x, 0, array, 0);
		Normalize(array);
		Encode(array, array2, 0);
		Mod.ModOddInverseVar(P32, array2, array2);
		Decode(array2, 0, z);
	}

	public static int IsZero(uint[] x)
	{
		uint num = 0u;
		for (int i = 0; i < 16; i++)
		{
			num |= x[i];
		}
		num |= num >> 16;
		num &= 0xFFFFu;
		return (int)(num - 1) >> 31;
	}

	public static bool IsZeroVar(uint[] x)
	{
		return 0L != IsZero(x);
	}

	public static void Mul(uint[] x, uint y, uint[] z)
	{
		uint num = x[0];
		uint num2 = x[1];
		uint num3 = x[2];
		uint num4 = x[3];
		uint num5 = x[4];
		uint num6 = x[5];
		uint num7 = x[6];
		uint num8 = x[7];
		uint num9 = x[8];
		uint num10 = x[9];
		uint num11 = x[10];
		uint num12 = x[11];
		uint num13 = x[12];
		uint num14 = x[13];
		uint num15 = x[14];
		uint num16 = x[15];
		ulong num17 = (ulong)num2 * (ulong)y;
		uint num18 = (uint)(int)num17 & 0xFFFFFFFu;
		num17 >>= 28;
		ulong num19 = (ulong)num6 * (ulong)y;
		uint num20 = (uint)(int)num19 & 0xFFFFFFFu;
		num19 >>= 28;
		ulong num21 = (ulong)num10 * (ulong)y;
		uint num22 = (uint)(int)num21 & 0xFFFFFFFu;
		num21 >>= 28;
		ulong num23 = (ulong)num14 * (ulong)y;
		uint num24 = (uint)(int)num23 & 0xFFFFFFFu;
		num23 >>= 28;
		num17 += (ulong)((long)num3 * (long)y);
		z[2] = (uint)(int)num17 & 0xFFFFFFFu;
		num17 >>= 28;
		num19 += (ulong)((long)num7 * (long)y);
		z[6] = (uint)(int)num19 & 0xFFFFFFFu;
		num19 >>= 28;
		num21 += (ulong)((long)num11 * (long)y);
		z[10] = (uint)(int)num21 & 0xFFFFFFFu;
		num21 >>= 28;
		num23 += (ulong)((long)num15 * (long)y);
		z[14] = (uint)(int)num23 & 0xFFFFFFFu;
		num23 >>= 28;
		num17 += (ulong)((long)num4 * (long)y);
		z[3] = (uint)(int)num17 & 0xFFFFFFFu;
		num17 >>= 28;
		num19 += (ulong)((long)num8 * (long)y);
		z[7] = (uint)(int)num19 & 0xFFFFFFFu;
		num19 >>= 28;
		num21 += (ulong)((long)num12 * (long)y);
		z[11] = (uint)(int)num21 & 0xFFFFFFFu;
		num21 >>= 28;
		num23 += (ulong)((long)num16 * (long)y);
		z[15] = (uint)(int)num23 & 0xFFFFFFFu;
		num23 >>= 28;
		num19 += num23;
		num17 += (ulong)((long)num5 * (long)y);
		z[4] = (uint)(int)num17 & 0xFFFFFFFu;
		num17 >>= 28;
		num19 += (ulong)((long)num9 * (long)y);
		z[8] = (uint)(int)num19 & 0xFFFFFFFu;
		num19 >>= 28;
		num21 += (ulong)((long)num13 * (long)y);
		z[12] = (uint)(int)num21 & 0xFFFFFFFu;
		num21 >>= 28;
		num23 += (ulong)((long)num * (long)y);
		z[0] = (uint)(int)num23 & 0xFFFFFFFu;
		num23 >>= 28;
		z[1] = num18 + (uint)(int)num23;
		z[5] = num20 + (uint)(int)num17;
		z[9] = num22 + (uint)(int)num19;
		z[13] = num24 + (uint)(int)num21;
	}

	public static void Mul(uint[] x, uint[] y, uint[] z)
	{
		uint num = x[0];
		uint num2 = x[1];
		uint num3 = x[2];
		uint num4 = x[3];
		uint num5 = x[4];
		uint num6 = x[5];
		uint num7 = x[6];
		uint num8 = x[7];
		uint num9 = x[8];
		uint num10 = x[9];
		uint num11 = x[10];
		uint num12 = x[11];
		uint num13 = x[12];
		uint num14 = x[13];
		uint num15 = x[14];
		uint num16 = x[15];
		uint num17 = y[0];
		uint num18 = y[1];
		uint num19 = y[2];
		uint num20 = y[3];
		uint num21 = y[4];
		uint num22 = y[5];
		uint num23 = y[6];
		uint num24 = y[7];
		uint num25 = y[8];
		uint num26 = y[9];
		uint num27 = y[10];
		uint num28 = y[11];
		uint num29 = y[12];
		uint num30 = y[13];
		uint num31 = y[14];
		uint num32 = y[15];
		uint num33 = num + num9;
		uint num34 = num2 + num10;
		uint num35 = num3 + num11;
		uint num36 = num4 + num12;
		uint num37 = num5 + num13;
		uint num38 = num6 + num14;
		uint num39 = num7 + num15;
		uint num40 = num8 + num16;
		uint num41 = num17 + num25;
		uint num42 = num18 + num26;
		uint num43 = num19 + num27;
		uint num44 = num20 + num28;
		uint num45 = num21 + num29;
		uint num46 = num22 + num30;
		uint num47 = num23 + num31;
		uint num48 = num24 + num32;
		ulong num49 = (ulong)num * (ulong)num17;
		ulong num50 = (ulong)((long)num8 * (long)num18 + (long)num7 * (long)num19 + (long)num6 * (long)num20 + (long)num5 * (long)num21 + (long)num4 * (long)num22 + (long)num3 * (long)num23 + (long)num2 * (long)num24);
		ulong num51 = (ulong)num9 * (ulong)num25;
		ulong num52 = (ulong)((long)num16 * (long)num26 + (long)num15 * (long)num27 + (long)num14 * (long)num28 + (long)num13 * (long)num29 + (long)num12 * (long)num30 + (long)num11 * (long)num31 + (long)num10 * (long)num32);
		ulong num53 = (ulong)num33 * (ulong)num41;
		ulong num54 = (ulong)((long)num40 * (long)num42 + (long)num39 * (long)num43 + (long)num38 * (long)num44 + (long)num37 * (long)num45 + (long)num36 * (long)num46 + (long)num35 * (long)num47 + (long)num34 * (long)num48);
		ulong num55 = num49 + num51 + num54 - num50;
		uint num56 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		ulong num57 = num52 + num53 - num49 + num54;
		uint num58 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		ulong num59 = (ulong)((long)num2 * (long)num17 + (long)num * (long)num18);
		ulong num60 = (ulong)((long)num8 * (long)num19 + (long)num7 * (long)num20 + (long)num6 * (long)num21 + (long)num5 * (long)num22 + (long)num4 * (long)num23 + (long)num3 * (long)num24);
		ulong num61 = (ulong)((long)num10 * (long)num25 + (long)num9 * (long)num26);
		ulong num62 = (ulong)((long)num16 * (long)num27 + (long)num15 * (long)num28 + (long)num14 * (long)num29 + (long)num13 * (long)num30 + (long)num12 * (long)num31 + (long)num11 * (long)num32);
		ulong num63 = (ulong)((long)num34 * (long)num41 + (long)num33 * (long)num42);
		ulong num64 = (ulong)((long)num40 * (long)num43 + (long)num39 * (long)num44 + (long)num38 * (long)num45 + (long)num37 * (long)num46 + (long)num36 * (long)num47 + (long)num35 * (long)num48);
		num55 += num59 + num61 + num64 - num60;
		uint num65 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		num57 += num62 + num63 - num59 + num64;
		uint num66 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		ulong num67 = (ulong)((long)num3 * (long)num17 + (long)num2 * (long)num18 + (long)num * (long)num19);
		ulong num68 = (ulong)((long)num8 * (long)num20 + (long)num7 * (long)num21 + (long)num6 * (long)num22 + (long)num5 * (long)num23 + (long)num4 * (long)num24);
		ulong num69 = (ulong)((long)num11 * (long)num25 + (long)num10 * (long)num26 + (long)num9 * (long)num27);
		ulong num70 = (ulong)((long)num16 * (long)num28 + (long)num15 * (long)num29 + (long)num14 * (long)num30 + (long)num13 * (long)num31 + (long)num12 * (long)num32);
		ulong num71 = (ulong)((long)num35 * (long)num41 + (long)num34 * (long)num42 + (long)num33 * (long)num43);
		ulong num72 = (ulong)((long)num40 * (long)num44 + (long)num39 * (long)num45 + (long)num38 * (long)num46 + (long)num37 * (long)num47 + (long)num36 * (long)num48);
		num55 += num67 + num69 + num72 - num68;
		uint num73 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		num57 += num70 + num71 - num67 + num72;
		uint num74 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		ulong num75 = (ulong)((long)num4 * (long)num17 + (long)num3 * (long)num18 + (long)num2 * (long)num19 + (long)num * (long)num20);
		ulong num76 = (ulong)((long)num8 * (long)num21 + (long)num7 * (long)num22 + (long)num6 * (long)num23 + (long)num5 * (long)num24);
		ulong num77 = (ulong)((long)num12 * (long)num25 + (long)num11 * (long)num26 + (long)num10 * (long)num27 + (long)num9 * (long)num28);
		ulong num78 = (ulong)((long)num16 * (long)num29 + (long)num15 * (long)num30 + (long)num14 * (long)num31 + (long)num13 * (long)num32);
		ulong num79 = (ulong)((long)num36 * (long)num41 + (long)num35 * (long)num42 + (long)num34 * (long)num43 + (long)num33 * (long)num44);
		ulong num80 = (ulong)((long)num40 * (long)num45 + (long)num39 * (long)num46 + (long)num38 * (long)num47 + (long)num37 * (long)num48);
		num55 += num75 + num77 + num80 - num76;
		uint num81 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		num57 += num78 + num79 - num75 + num80;
		uint num82 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		ulong num83 = (ulong)((long)num5 * (long)num17 + (long)num4 * (long)num18 + (long)num3 * (long)num19 + (long)num2 * (long)num20 + (long)num * (long)num21);
		ulong num84 = (ulong)((long)num8 * (long)num22 + (long)num7 * (long)num23 + (long)num6 * (long)num24);
		ulong num85 = (ulong)((long)num13 * (long)num25 + (long)num12 * (long)num26 + (long)num11 * (long)num27 + (long)num10 * (long)num28 + (long)num9 * (long)num29);
		ulong num86 = (ulong)((long)num16 * (long)num30 + (long)num15 * (long)num31 + (long)num14 * (long)num32);
		ulong num87 = (ulong)((long)num37 * (long)num41 + (long)num36 * (long)num42 + (long)num35 * (long)num43 + (long)num34 * (long)num44 + (long)num33 * (long)num45);
		ulong num88 = (ulong)((long)num40 * (long)num46 + (long)num39 * (long)num47 + (long)num38 * (long)num48);
		num55 += num83 + num85 + num88 - num84;
		uint num89 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		num57 += num86 + num87 - num83 + num88;
		uint num90 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		ulong num91 = (ulong)((long)num6 * (long)num17 + (long)num5 * (long)num18 + (long)num4 * (long)num19 + (long)num3 * (long)num20 + (long)num2 * (long)num21 + (long)num * (long)num22);
		ulong num92 = (ulong)((long)num8 * (long)num23 + (long)num7 * (long)num24);
		ulong num93 = (ulong)((long)num14 * (long)num25 + (long)num13 * (long)num26 + (long)num12 * (long)num27 + (long)num11 * (long)num28 + (long)num10 * (long)num29 + (long)num9 * (long)num30);
		ulong num94 = (ulong)((long)num16 * (long)num31 + (long)num15 * (long)num32);
		ulong num95 = (ulong)((long)num38 * (long)num41 + (long)num37 * (long)num42 + (long)num36 * (long)num43 + (long)num35 * (long)num44 + (long)num34 * (long)num45 + (long)num33 * (long)num46);
		ulong num96 = (ulong)((long)num40 * (long)num47 + (long)num39 * (long)num48);
		num55 += num91 + num93 + num96 - num92;
		uint num97 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		num57 += num94 + num95 - num91 + num96;
		uint num98 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		ulong num99 = (ulong)((long)num7 * (long)num17 + (long)num6 * (long)num18 + (long)num5 * (long)num19 + (long)num4 * (long)num20 + (long)num3 * (long)num21 + (long)num2 * (long)num22 + (long)num * (long)num23);
		ulong num100 = (ulong)num8 * (ulong)num24;
		ulong num101 = (ulong)((long)num15 * (long)num25 + (long)num14 * (long)num26 + (long)num13 * (long)num27 + (long)num12 * (long)num28 + (long)num11 * (long)num29 + (long)num10 * (long)num30 + (long)num9 * (long)num31);
		ulong num102 = (ulong)num16 * (ulong)num32;
		ulong num103 = (ulong)((long)num39 * (long)num41 + (long)num38 * (long)num42 + (long)num37 * (long)num43 + (long)num36 * (long)num44 + (long)num35 * (long)num45 + (long)num34 * (long)num46 + (long)num33 * (long)num47);
		ulong num104 = (ulong)num40 * (ulong)num48;
		num55 += num99 + num101 + num104 - num100;
		uint num105 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		num57 += num102 + num103 - num99 + num104;
		uint num106 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		ulong num107 = (ulong)((long)num8 * (long)num17 + (long)num7 * (long)num18 + (long)num6 * (long)num19 + (long)num5 * (long)num20 + (long)num4 * (long)num21 + (long)num3 * (long)num22 + (long)num2 * (long)num23 + (long)num * (long)num24);
		ulong num108 = (ulong)((long)num16 * (long)num25 + (long)num15 * (long)num26 + (long)num14 * (long)num27 + (long)num13 * (long)num28 + (long)num12 * (long)num29 + (long)num11 * (long)num30 + (long)num10 * (long)num31 + (long)num9 * (long)num32);
		ulong num109 = (ulong)((long)num40 * (long)num41 + (long)num39 * (long)num42 + (long)num38 * (long)num43 + (long)num37 * (long)num44 + (long)num36 * (long)num45 + (long)num35 * (long)num46 + (long)num34 * (long)num47 + (long)num33 * (long)num48);
		num55 += num107 + num108;
		uint num110 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		num57 += num109 - num107;
		uint num111 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		num55 += num57;
		num55 += num58;
		num58 = (uint)(int)num55 & 0xFFFFFFFu;
		num55 >>= 28;
		num57 += num56;
		num56 = (uint)(int)num57 & 0xFFFFFFFu;
		num57 >>= 28;
		num66 += (uint)(int)num55;
		num65 += (uint)(int)num57;
		z[0] = num56;
		z[1] = num65;
		z[2] = num73;
		z[3] = num81;
		z[4] = num89;
		z[5] = num97;
		z[6] = num105;
		z[7] = num110;
		z[8] = num58;
		z[9] = num66;
		z[10] = num74;
		z[11] = num82;
		z[12] = num90;
		z[13] = num98;
		z[14] = num106;
		z[15] = num111;
	}

	public static void Negate(uint[] x, uint[] z)
	{
		uint[] x2 = Create();
		Sub(x2, x, z);
	}

	public static void Normalize(uint[] z)
	{
		Reduce(z, 1);
		Reduce(z, -1);
	}

	public static void One(uint[] z)
	{
		z[0] = 1u;
		for (int i = 1; i < 16; i++)
		{
			z[i] = 0u;
		}
	}

	private static void PowPm3d4(uint[] x, uint[] z)
	{
		uint[] array = Create();
		Sqr(x, array);
		Mul(x, array, array);
		uint[] array2 = Create();
		Sqr(array, array2);
		Mul(x, array2, array2);
		uint[] array3 = Create();
		Sqr(array2, 3, array3);
		Mul(array2, array3, array3);
		uint[] array4 = Create();
		Sqr(array3, 3, array4);
		Mul(array2, array4, array4);
		uint[] array5 = Create();
		Sqr(array4, 9, array5);
		Mul(array4, array5, array5);
		uint[] array6 = Create();
		Sqr(array5, array6);
		Mul(x, array6, array6);
		uint[] array7 = Create();
		Sqr(array6, 18, array7);
		Mul(array5, array7, array7);
		uint[] array8 = Create();
		Sqr(array7, 37, array8);
		Mul(array7, array8, array8);
		uint[] array9 = Create();
		Sqr(array8, 37, array9);
		Mul(array7, array9, array9);
		uint[] array10 = Create();
		Sqr(array9, 111, array10);
		Mul(array9, array10, array10);
		uint[] array11 = Create();
		Sqr(array10, array11);
		Mul(x, array11, array11);
		uint[] array12 = Create();
		Sqr(array11, 223, array12);
		Mul(array12, array10, z);
	}

	private static void Reduce(uint[] z, int x)
	{
		uint num = z[15];
		uint num2 = num & 0xFFFFFFFu;
		int num3 = (int)(num >> 28) + x;
		long num4 = num3;
		for (int i = 0; i < 8; i++)
		{
			num4 += z[i];
			z[i] = (uint)(int)num4 & 0xFFFFFFFu;
			num4 >>= 28;
		}
		num4 += num3;
		for (int j = 8; j < 15; j++)
		{
			num4 += z[j];
			z[j] = (uint)(int)num4 & 0xFFFFFFFu;
			num4 >>= 28;
		}
		z[15] = num2 + (uint)(int)num4;
	}

	public static void Sqr(uint[] x, uint[] z)
	{
		uint num = x[0];
		uint num2 = x[1];
		uint num3 = x[2];
		uint num4 = x[3];
		uint num5 = x[4];
		uint num6 = x[5];
		uint num7 = x[6];
		uint num8 = x[7];
		uint num9 = x[8];
		uint num10 = x[9];
		uint num11 = x[10];
		uint num12 = x[11];
		uint num13 = x[12];
		uint num14 = x[13];
		uint num15 = x[14];
		uint num16 = x[15];
		uint num17 = num * 2;
		uint num18 = num2 * 2;
		uint num19 = num3 * 2;
		uint num20 = num4 * 2;
		uint num21 = num5 * 2;
		uint num22 = num6 * 2;
		uint num23 = num7 * 2;
		uint num24 = num9 * 2;
		uint num25 = num10 * 2;
		uint num26 = num11 * 2;
		uint num27 = num12 * 2;
		uint num28 = num13 * 2;
		uint num29 = num14 * 2;
		uint num30 = num15 * 2;
		uint num31 = num + num9;
		uint num32 = num2 + num10;
		uint num33 = num3 + num11;
		uint num34 = num4 + num12;
		uint num35 = num5 + num13;
		uint num36 = num6 + num14;
		uint num37 = num7 + num15;
		uint num38 = num8 + num16;
		uint num39 = num31 * 2;
		uint num40 = num32 * 2;
		uint num41 = num33 * 2;
		uint num42 = num34 * 2;
		uint num43 = num35 * 2;
		uint num44 = num36 * 2;
		uint num45 = num37 * 2;
		ulong num46 = (ulong)num * (ulong)num;
		ulong num47 = (ulong)((long)num8 * (long)num18 + (long)num7 * (long)num19 + (long)num6 * (long)num20 + (long)num5 * (long)num5);
		ulong num48 = (ulong)num9 * (ulong)num9;
		ulong num49 = (ulong)((long)num16 * (long)num25 + (long)num15 * (long)num26 + (long)num14 * (long)num27 + (long)num13 * (long)num13);
		ulong num50 = (ulong)num31 * (ulong)num31;
		ulong num51 = (ulong)((long)num38 * (long)num40 + (long)num37 * (long)num41 + (long)num36 * (long)num42 + (long)num35 * (long)num35);
		ulong num52 = num46 + num48 + num51 - num47;
		uint num53 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		ulong num54 = num49 + num50 - num46 + num51;
		uint num55 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		ulong num56 = (ulong)num2 * (ulong)num17;
		ulong num57 = (ulong)((long)num8 * (long)num19 + (long)num7 * (long)num20 + (long)num6 * (long)num21);
		ulong num58 = (ulong)num10 * (ulong)num24;
		ulong num59 = (ulong)((long)num16 * (long)num26 + (long)num15 * (long)num27 + (long)num14 * (long)num28);
		ulong num60 = (ulong)num32 * (ulong)num39;
		ulong num61 = (ulong)((long)num38 * (long)num41 + (long)num37 * (long)num42 + (long)num36 * (long)num43);
		num52 += num56 + num58 + num61 - num57;
		uint num62 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		num54 += num59 + num60 - num56 + num61;
		uint num63 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		ulong num64 = (ulong)((long)num3 * (long)num17 + (long)num2 * (long)num2);
		ulong num65 = (ulong)((long)num8 * (long)num20 + (long)num7 * (long)num21 + (long)num6 * (long)num6);
		ulong num66 = (ulong)((long)num11 * (long)num24 + (long)num10 * (long)num10);
		ulong num67 = (ulong)((long)num16 * (long)num27 + (long)num15 * (long)num28 + (long)num14 * (long)num14);
		ulong num68 = (ulong)((long)num33 * (long)num39 + (long)num32 * (long)num32);
		ulong num69 = (ulong)((long)num38 * (long)num42 + (long)num37 * (long)num43 + (long)num36 * (long)num36);
		num52 += num64 + num66 + num69 - num65;
		uint num70 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		num54 += num67 + num68 - num64 + num69;
		uint num71 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		ulong num72 = (ulong)((long)num4 * (long)num17 + (long)num3 * (long)num18);
		ulong num73 = (ulong)((long)num8 * (long)num21 + (long)num7 * (long)num22);
		ulong num74 = (ulong)((long)num12 * (long)num24 + (long)num11 * (long)num25);
		ulong num75 = (ulong)((long)num16 * (long)num28 + (long)num15 * (long)num29);
		ulong num76 = (ulong)((long)num34 * (long)num39 + (long)num33 * (long)num40);
		ulong num77 = (ulong)((long)num38 * (long)num43 + (long)num37 * (long)num44);
		num52 += num72 + num74 + num77 - num73;
		uint num78 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		num54 += num75 + num76 - num72 + num77;
		uint num79 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		ulong num80 = (ulong)((long)num5 * (long)num17 + (long)num4 * (long)num18 + (long)num3 * (long)num3);
		ulong num81 = (ulong)((long)num8 * (long)num22 + (long)num7 * (long)num7);
		ulong num82 = (ulong)((long)num13 * (long)num24 + (long)num12 * (long)num25 + (long)num11 * (long)num11);
		ulong num83 = (ulong)((long)num16 * (long)num29 + (long)num15 * (long)num15);
		ulong num84 = (ulong)((long)num35 * (long)num39 + (long)num34 * (long)num40 + (long)num33 * (long)num33);
		ulong num85 = (ulong)((long)num38 * (long)num44 + (long)num37 * (long)num37);
		num52 += num80 + num82 + num85 - num81;
		uint num86 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		num54 += num83 + num84 - num80 + num85;
		uint num87 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		ulong num88 = (ulong)((long)num6 * (long)num17 + (long)num5 * (long)num18 + (long)num4 * (long)num19);
		ulong num89 = (ulong)num8 * (ulong)num23;
		ulong num90 = (ulong)((long)num14 * (long)num24 + (long)num13 * (long)num25 + (long)num12 * (long)num26);
		ulong num91 = (ulong)num16 * (ulong)num30;
		ulong num92 = (ulong)((long)num36 * (long)num39 + (long)num35 * (long)num40 + (long)num34 * (long)num41);
		ulong num93 = (ulong)num38 * (ulong)num45;
		num52 += num88 + num90 + num93 - num89;
		uint num94 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		num54 += num91 + num92 - num88 + num93;
		uint num95 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		ulong num96 = (ulong)((long)num7 * (long)num17 + (long)num6 * (long)num18 + (long)num5 * (long)num19 + (long)num4 * (long)num4);
		ulong num97 = (ulong)num8 * (ulong)num8;
		ulong num98 = (ulong)((long)num15 * (long)num24 + (long)num14 * (long)num25 + (long)num13 * (long)num26 + (long)num12 * (long)num12);
		ulong num99 = (ulong)num16 * (ulong)num16;
		ulong num100 = (ulong)((long)num37 * (long)num39 + (long)num36 * (long)num40 + (long)num35 * (long)num41 + (long)num34 * (long)num34);
		ulong num101 = (ulong)num38 * (ulong)num38;
		num52 += num96 + num98 + num101 - num97;
		uint num102 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		num54 += num99 + num100 - num96 + num101;
		uint num103 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		ulong num104 = (ulong)((long)num8 * (long)num17 + (long)num7 * (long)num18 + (long)num6 * (long)num19 + (long)num5 * (long)num20);
		ulong num105 = (ulong)((long)num16 * (long)num24 + (long)num15 * (long)num25 + (long)num14 * (long)num26 + (long)num13 * (long)num27);
		ulong num106 = (ulong)((long)num38 * (long)num39 + (long)num37 * (long)num40 + (long)num36 * (long)num41 + (long)num35 * (long)num42);
		num52 += num104 + num105;
		uint num107 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		num54 += num106 - num104;
		uint num108 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		num52 += num54;
		num52 += num55;
		num55 = (uint)(int)num52 & 0xFFFFFFFu;
		num52 >>= 28;
		num54 += num53;
		num53 = (uint)(int)num54 & 0xFFFFFFFu;
		num54 >>= 28;
		num63 += (uint)(int)num52;
		num62 += (uint)(int)num54;
		z[0] = num53;
		z[1] = num62;
		z[2] = num70;
		z[3] = num78;
		z[4] = num86;
		z[5] = num94;
		z[6] = num102;
		z[7] = num107;
		z[8] = num55;
		z[9] = num63;
		z[10] = num71;
		z[11] = num79;
		z[12] = num87;
		z[13] = num95;
		z[14] = num103;
		z[15] = num108;
	}

	public static void Sqr(uint[] x, int n, uint[] z)
	{
		Sqr(x, z);
		while (--n > 0)
		{
			Sqr(z, z);
		}
	}

	public static bool SqrtRatioVar(uint[] u, uint[] v, uint[] z)
	{
		uint[] array = Create();
		uint[] array2 = Create();
		Sqr(u, array);
		Mul(array, v, array);
		Sqr(array, array2);
		Mul(array, u, array);
		Mul(array2, u, array2);
		Mul(array2, v, array2);
		uint[] array3 = Create();
		PowPm3d4(array2, array3);
		Mul(array3, array, array3);
		uint[] array4 = Create();
		Sqr(array3, array4);
		Mul(array4, v, array4);
		Sub(u, array4, array4);
		Normalize(array4);
		if (IsZeroVar(array4))
		{
			Copy(array3, 0, z, 0);
			return true;
		}
		return false;
	}

	public static void Sub(uint[] x, uint[] y, uint[] z)
	{
		uint num = x[0];
		uint num2 = x[1];
		uint num3 = x[2];
		uint num4 = x[3];
		uint num5 = x[4];
		uint num6 = x[5];
		uint num7 = x[6];
		uint num8 = x[7];
		uint num9 = x[8];
		uint num10 = x[9];
		uint num11 = x[10];
		uint num12 = x[11];
		uint num13 = x[12];
		uint num14 = x[13];
		uint num15 = x[14];
		uint num16 = x[15];
		uint num17 = y[0];
		uint num18 = y[1];
		uint num19 = y[2];
		uint num20 = y[3];
		uint num21 = y[4];
		uint num22 = y[5];
		uint num23 = y[6];
		uint num24 = y[7];
		uint num25 = y[8];
		uint num26 = y[9];
		uint num27 = y[10];
		uint num28 = y[11];
		uint num29 = y[12];
		uint num30 = y[13];
		uint num31 = y[14];
		uint num32 = y[15];
		uint num33 = num + 536870910 - num17;
		uint num34 = num2 + 536870910 - num18;
		uint num35 = num3 + 536870910 - num19;
		uint num36 = num4 + 536870910 - num20;
		uint num37 = num5 + 536870910 - num21;
		uint num38 = num6 + 536870910 - num22;
		uint num39 = num7 + 536870910 - num23;
		uint num40 = num8 + 536870910 - num24;
		uint num41 = num9 + 536870908 - num25;
		uint num42 = num10 + 536870910 - num26;
		uint num43 = num11 + 536870910 - num27;
		uint num44 = num12 + 536870910 - num28;
		uint num45 = num13 + 536870910 - num29;
		uint num46 = num14 + 536870910 - num30;
		uint num47 = num15 + 536870910 - num31;
		uint num48 = num16 + 536870910 - num32;
		num35 += num34 >> 28;
		num34 &= 0xFFFFFFFu;
		num39 += num38 >> 28;
		num38 &= 0xFFFFFFFu;
		num43 += num42 >> 28;
		num42 &= 0xFFFFFFFu;
		num47 += num46 >> 28;
		num46 &= 0xFFFFFFFu;
		num36 += num35 >> 28;
		num35 &= 0xFFFFFFFu;
		num40 += num39 >> 28;
		num39 &= 0xFFFFFFFu;
		num44 += num43 >> 28;
		num43 &= 0xFFFFFFFu;
		num48 += num47 >> 28;
		num47 &= 0xFFFFFFFu;
		uint num49 = num48 >> 28;
		num48 &= 0xFFFFFFFu;
		num33 += num49;
		num41 += num49;
		num37 += num36 >> 28;
		num36 &= 0xFFFFFFFu;
		num41 += num40 >> 28;
		num40 &= 0xFFFFFFFu;
		num45 += num44 >> 28;
		num44 &= 0xFFFFFFFu;
		num34 += num33 >> 28;
		num33 &= 0xFFFFFFFu;
		num38 += num37 >> 28;
		num37 &= 0xFFFFFFFu;
		num42 += num41 >> 28;
		num41 &= 0xFFFFFFFu;
		num46 += num45 >> 28;
		num45 &= 0xFFFFFFFu;
		z[0] = num33;
		z[1] = num34;
		z[2] = num35;
		z[3] = num36;
		z[4] = num37;
		z[5] = num38;
		z[6] = num39;
		z[7] = num40;
		z[8] = num41;
		z[9] = num42;
		z[10] = num43;
		z[11] = num44;
		z[12] = num45;
		z[13] = num46;
		z[14] = num47;
		z[15] = num48;
	}

	public static void SubOne(uint[] z)
	{
		uint[] array = Create();
		array[0] = 1u;
		Sub(z, array, z);
	}

	public static void Zero(uint[] z)
	{
		for (int i = 0; i < 16; i++)
		{
			z[i] = 0u;
		}
	}
}
