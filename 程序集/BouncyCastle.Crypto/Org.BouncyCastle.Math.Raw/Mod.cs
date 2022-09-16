using System;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.Raw;

internal abstract class Mod
{
	private const int M30 = 1073741823;

	private const ulong M32UL = 4294967295uL;

	private static readonly SecureRandom RandomSource = new SecureRandom();

	public static void CheckedModOddInverse(uint[] m, uint[] x, uint[] z)
	{
		if (ModOddInverse(m, x, z) == 0)
		{
			throw new ArithmeticException("Inverse does not exist.");
		}
	}

	public static void CheckedModOddInverseVar(uint[] m, uint[] x, uint[] z)
	{
		if (!ModOddInverseVar(m, x, z))
		{
			throw new ArithmeticException("Inverse does not exist.");
		}
	}

	public static uint Inverse32(uint d)
	{
		uint num = d;
		num *= 2 - d * num;
		num *= 2 - d * num;
		num *= 2 - d * num;
		return num * (2 - d * num);
	}

	public static uint ModOddInverse(uint[] m, uint[] x, uint[] z)
	{
		int num = m.Length;
		int num2 = (num << 5) - Integers.NumberOfLeadingZeros((int)m[num - 1]);
		int num3 = (num2 + 29) / 30;
		int[] t = new int[4];
		int[] array = new int[num3];
		int[] array2 = new int[num3];
		int[] array3 = new int[num3];
		int[] array4 = new int[num3];
		int[] array5 = new int[num3];
		array2[0] = 1;
		Encode30(num2, x, 0, array4, 0);
		Encode30(num2, m, 0, array5, 0);
		Array.Copy(array5, 0, array3, 0, num3);
		int eta = -1;
		int m0Inv = (int)Inverse32((uint)array5[0]);
		int maximumDivsteps = GetMaximumDivsteps(num2);
		for (int i = 0; i < maximumDivsteps; i += 30)
		{
			eta = Divsteps30(eta, array3[0], array4[0], t);
			UpdateDE30(num3, array, array2, t, m0Inv, array5);
			UpdateFG30(num3, array3, array4, t);
		}
		int num4 = array3[num3 - 1] >> 31;
		CNegate30(num3, num4, array3);
		CNormalize30(num3, num4, array, array5);
		Decode30(num2, array, 0, z, 0);
		return (uint)(EqualTo(num3, array3, 1) & EqualToZero(num3, array4));
	}

	public static bool ModOddInverseVar(uint[] m, uint[] x, uint[] z)
	{
		int num = m.Length;
		int num2 = (num << 5) - Integers.NumberOfLeadingZeros((int)m[num - 1]);
		int num3 = (num2 + 29) / 30;
		int[] t = new int[4];
		int[] array = new int[num3];
		int[] array2 = new int[num3];
		int[] array3 = new int[num3];
		int[] array4 = new int[num3];
		int[] array5 = new int[num3];
		array2[0] = 1;
		Encode30(num2, x, 0, array4, 0);
		Encode30(num2, m, 0, array5, 0);
		Array.Copy(array5, 0, array3, 0, num3);
		int num4 = Integers.NumberOfLeadingZeros(array4[num3 - 1] | 1) - (num3 * 30 + 2 - num2);
		int eta = -1 - num4;
		int num5 = num3;
		int num6 = num3;
		int m0Inv = (int)Inverse32((uint)array5[0]);
		int maximumDivsteps = GetMaximumDivsteps(num2);
		int num7 = 0;
		while (!IsZero(num6, array4))
		{
			if (num7 >= maximumDivsteps)
			{
				return false;
			}
			num7 += 30;
			eta = Divsteps30Var(eta, array3[0], array4[0], t);
			UpdateDE30(num5, array, array2, t, m0Inv, array5);
			UpdateFG30(num6, array3, array4, t);
			int num8 = array3[num6 - 1];
			int num9 = array4[num6 - 1];
			int num10 = num6 - 2 >> 31;
			num10 |= num8 ^ (num8 >> 31);
			if ((num10 | (num9 ^ (num9 >> 31))) == 0)
			{
				int[] array6;
				int[] array7 = (array6 = array3);
				int num11 = num6 - 2;
				nint num12 = num11;
				array7[num11] = array6[num12] | (num8 << 30);
				int[] array8 = (array6 = array4);
				int num13 = num6 - 2;
				num12 = num13;
				array8[num13] = array6[num12] | (num9 << 30);
				num6--;
			}
		}
		int num14 = array3[num6 - 1] >> 31;
		int num15 = array[num5 - 1] >> 31;
		if (num15 < 0)
		{
			num15 = Add30(num5, array, array5);
		}
		if (num14 < 0)
		{
			num15 = Negate30(num5, array);
			num14 = Negate30(num6, array3);
		}
		if (!IsOne(num6, array3))
		{
			return false;
		}
		if (num15 < 0)
		{
			num15 = Add30(num5, array, array5);
		}
		Decode30(num2, array, 0, z, 0);
		return true;
	}

	public static uint[] Random(uint[] p)
	{
		int num = p.Length;
		uint[] array = Nat.Create(num);
		uint num2 = p[num - 1];
		num2 |= num2 >> 1;
		num2 |= num2 >> 2;
		num2 |= num2 >> 4;
		num2 |= num2 >> 8;
		num2 |= num2 >> 16;
		do
		{
			byte[] array2 = new byte[num << 2];
			RandomSource.NextBytes(array2);
			Pack.BE_To_UInt32(array2, 0, array);
			uint[] array3;
			uint[] array4 = (array3 = array);
			int num3 = num - 1;
			nint num4 = num3;
			array4[num3] = array3[num4] & num2;
		}
		while (Nat.Gte(num, array, p));
		return array;
	}

	private static int Add30(int len30, int[] D, int[] M)
	{
		int num = 0;
		int num2 = len30 - 1;
		for (int i = 0; i < num2; i++)
		{
			num += D[i] + M[i];
			D[i] = num & 0x3FFFFFFF;
			num >>= 30;
		}
		return (D[num2] = num + (D[num2] + M[num2])) >> 30;
	}

	private static void CNegate30(int len30, int cond, int[] D)
	{
		int num = 0;
		int num2 = len30 - 1;
		for (int i = 0; i < num2; i++)
		{
			num += (D[i] ^ cond) - cond;
			D[i] = num & 0x3FFFFFFF;
			num >>= 30;
		}
		num = (D[num2] = num + ((D[num2] ^ cond) - cond));
	}

	private static void CNormalize30(int len30, int condNegate, int[] D, int[] M)
	{
		int num = len30 - 1;
		int num2 = 0;
		int num3 = D[num] >> 31;
		for (int i = 0; i < num; i++)
		{
			int num4 = D[i] + (M[i] & num3);
			num4 = (num4 ^ condNegate) - condNegate;
			num2 += num4;
			D[i] = num2 & 0x3FFFFFFF;
			num2 >>= 30;
		}
		int num5 = D[num] + (M[num] & num3);
		num5 = (num5 ^ condNegate) - condNegate;
		num2 = (D[num] = num2 + num5);
		int num6 = 0;
		int num7 = D[num] >> 31;
		for (int j = 0; j < num; j++)
		{
			int num8 = D[j] + (M[j] & num7);
			num6 += num8;
			D[j] = num6 & 0x3FFFFFFF;
			num6 >>= 30;
		}
		int num9 = D[num] + (M[num] & num7);
		num6 = (D[num] = num6 + num9);
	}

	private static void Decode30(int bits, int[] x, int xOff, uint[] z, int zOff)
	{
		int i = 0;
		ulong num = 0uL;
		while (bits > 0)
		{
			for (; i < System.Math.Min(32, bits); i += 30)
			{
				num |= (ulong)((long)x[xOff++] << i);
			}
			z[zOff++] = (uint)num;
			num >>= 32;
			i -= 32;
			bits -= 32;
		}
	}

	private static int Divsteps30(int eta, int f0, int g0, int[] t)
	{
		int num = 1;
		int num2 = 0;
		int num3 = 0;
		int num4 = 1;
		int num5 = f0;
		int num6 = g0;
		for (int i = 0; i < 30; i++)
		{
			int num7 = eta >> 31;
			int num8 = -(num6 & 1);
			int num9 = (num5 ^ num7) - num7;
			int num10 = (num ^ num7) - num7;
			int num11 = (num2 ^ num7) - num7;
			num6 += num9 & num8;
			num3 += num10 & num8;
			num4 += num11 & num8;
			num7 &= num8;
			eta = (eta ^ num7) - (num7 + 1);
			num5 += num6 & num7;
			num += num3 & num7;
			num2 += num4 & num7;
			num6 >>= 1;
			num <<= 1;
			num2 <<= 1;
		}
		t[0] = num;
		t[1] = num2;
		t[2] = num3;
		t[3] = num4;
		return eta;
	}

	private static int Divsteps30Var(int eta, int f0, int g0, int[] t)
	{
		int num = 1;
		int num2 = 0;
		int num3 = 0;
		int num4 = 1;
		int num5 = f0;
		int num6 = g0;
		int num7 = 30;
		while (true)
		{
			int num8 = Integers.NumberOfTrailingZeros(num6 | (-1 << num7));
			num6 >>= num8;
			num <<= num8;
			num2 <<= num8;
			eta -= num8;
			num7 -= num8;
			if (num7 <= 0)
			{
				break;
			}
			int num14;
			if (eta < 0)
			{
				eta = -eta;
				int num9 = num5;
				num5 = num6;
				num6 = -num9;
				int num10 = num;
				num = num3;
				num3 = -num10;
				int num11 = num2;
				num2 = num4;
				num4 = -num11;
				int num12 = ((eta + 1 > num7) ? num7 : (eta + 1));
				int num13 = (int)((uint.MaxValue >> 32 - num12) & 0x3F);
				num14 = (num5 * num6 * (num5 * num5 - 2)) & num13;
			}
			else
			{
				int num12 = ((eta + 1 > num7) ? num7 : (eta + 1));
				int num13 = (int)((uint.MaxValue >> 32 - num12) & 0xF);
				num14 = num5 + (((num5 + 1) & 4) << 1);
				num14 = (-num14 * num6) & num13;
			}
			num6 += num5 * num14;
			num3 += num * num14;
			num4 += num2 * num14;
		}
		t[0] = num;
		t[1] = num2;
		t[2] = num3;
		t[3] = num4;
		return eta;
	}

	private static void Encode30(int bits, uint[] x, int xOff, int[] z, int zOff)
	{
		int num = 0;
		ulong num2 = 0uL;
		while (bits > 0)
		{
			if (num < System.Math.Min(30, bits))
			{
				num2 |= ((ulong)x[xOff++] & 0xFFFFFFFFuL) << num;
				num += 32;
			}
			z[zOff++] = (int)num2 & 0x3FFFFFFF;
			num2 >>= 30;
			num -= 30;
			bits -= 30;
		}
	}

	private static int EqualTo(int len, int[] x, int y)
	{
		int num = x[0] ^ y;
		for (int i = 1; i < len; i++)
		{
			num |= x[i];
		}
		num = (int)((uint)num >> 1) | (num & 1);
		return num - 1 >> 31;
	}

	private static int EqualToZero(int len, int[] x)
	{
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			num |= x[i];
		}
		num = (int)((uint)num >> 1) | (num & 1);
		return num - 1 >> 31;
	}

	private static int GetMaximumDivsteps(int bits)
	{
		return (49 * bits + ((bits < 46) ? 80 : 47)) / 17;
	}

	private static bool IsOne(int len, int[] x)
	{
		if (x[0] != 1)
		{
			return false;
		}
		for (int i = 1; i < len; i++)
		{
			if (x[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsZero(int len, int[] x)
	{
		if (x[0] != 0)
		{
			return false;
		}
		for (int i = 1; i < len; i++)
		{
			if (x[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	private static int Negate30(int len30, int[] D)
	{
		int num = 0;
		int num2 = len30 - 1;
		for (int i = 0; i < num2; i++)
		{
			num -= D[i];
			D[i] = num & 0x3FFFFFFF;
			num >>= 30;
		}
		return (D[num2] = num - D[num2]) >> 30;
	}

	private static void UpdateDE30(int len30, int[] D, int[] E, int[] t, int m0Inv32, int[] M)
	{
		int num = t[0];
		int num2 = t[1];
		int num3 = t[2];
		int num4 = t[3];
		int num5 = D[len30 - 1] >> 31;
		int num6 = E[len30 - 1] >> 31;
		int num7 = (num & num5) + (num2 & num6);
		int num8 = (num3 & num5) + (num4 & num6);
		int num9 = M[0];
		int num10 = D[0];
		int num11 = E[0];
		long num12 = (long)num * (long)num10 + (long)num2 * (long)num11;
		long num13 = (long)num3 * (long)num10 + (long)num4 * (long)num11;
		num7 -= (m0Inv32 * (int)num12 + num7) & 0x3FFFFFFF;
		num8 -= (m0Inv32 * (int)num13 + num8) & 0x3FFFFFFF;
		num12 += (long)num9 * (long)num7;
		num13 += (long)num9 * (long)num8;
		num12 >>= 30;
		num13 >>= 30;
		for (int i = 1; i < len30; i++)
		{
			num9 = M[i];
			num10 = D[i];
			num11 = E[i];
			num12 += (long)num * (long)num10 + (long)num2 * (long)num11 + (long)num9 * (long)num7;
			num13 += (long)num3 * (long)num10 + (long)num4 * (long)num11 + (long)num9 * (long)num8;
			D[i - 1] = (int)num12 & 0x3FFFFFFF;
			num12 >>= 30;
			E[i - 1] = (int)num13 & 0x3FFFFFFF;
			num13 >>= 30;
		}
		D[len30 - 1] = (int)num12;
		E[len30 - 1] = (int)num13;
	}

	private static void UpdateFG30(int len30, int[] F, int[] G, int[] t)
	{
		int num = t[0];
		int num2 = t[1];
		int num3 = t[2];
		int num4 = t[3];
		int num5 = F[0];
		int num6 = G[0];
		long num7 = (long)num * (long)num5 + (long)num2 * (long)num6;
		long num8 = (long)num3 * (long)num5 + (long)num4 * (long)num6;
		num7 >>= 30;
		num8 >>= 30;
		for (int i = 1; i < len30; i++)
		{
			num5 = F[i];
			num6 = G[i];
			num7 += (long)num * (long)num5 + (long)num2 * (long)num6;
			num8 += (long)num3 * (long)num5 + (long)num4 * (long)num6;
			F[i - 1] = (int)num7 & 0x3FFFFFFF;
			num7 >>= 30;
			G[i - 1] = (int)num8 & 0x3FFFFFFF;
			num8 >>= 30;
		}
		F[len30 - 1] = (int)num7;
		G[len30 - 1] = (int)num8;
	}
}
