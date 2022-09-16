using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Math.Raw;

internal abstract class Nat256
{
	private const ulong M = 4294967295uL;

	public static uint Add(uint[] x, uint[] y, uint[] z)
	{
		ulong num = 0uL;
		num += (ulong)((long)x[0] + (long)y[0]);
		z[0] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[1] + (long)y[1]);
		z[1] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[2] + (long)y[2]);
		z[2] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[3] + (long)y[3]);
		z[3] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[4] + (long)y[4]);
		z[4] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[5] + (long)y[5]);
		z[5] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[6] + (long)y[6]);
		z[6] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[7] + (long)y[7]);
		z[7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint Add(uint[] x, int xOff, uint[] y, int yOff, uint[] z, int zOff)
	{
		ulong num = 0uL;
		num += (ulong)((long)x[xOff] + (long)y[yOff]);
		z[zOff] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 1] + (long)y[yOff + 1]);
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 2] + (long)y[yOff + 2]);
		z[zOff + 2] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 3] + (long)y[yOff + 3]);
		z[zOff + 3] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 4] + (long)y[yOff + 4]);
		z[zOff + 4] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 5] + (long)y[yOff + 5]);
		z[zOff + 5] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 6] + (long)y[yOff + 6]);
		z[zOff + 6] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 7] + (long)y[yOff + 7]);
		z[zOff + 7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint AddBothTo(uint[] x, uint[] y, uint[] z)
	{
		ulong num = 0uL;
		num += (ulong)((long)x[0] + (long)y[0] + z[0]);
		z[0] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[1] + (long)y[1] + z[1]);
		z[1] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[2] + (long)y[2] + z[2]);
		z[2] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[3] + (long)y[3] + z[3]);
		z[3] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[4] + (long)y[4] + z[4]);
		z[4] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[5] + (long)y[5] + z[5]);
		z[5] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[6] + (long)y[6] + z[6]);
		z[6] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[7] + (long)y[7] + z[7]);
		z[7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint AddBothTo(uint[] x, int xOff, uint[] y, int yOff, uint[] z, int zOff)
	{
		ulong num = 0uL;
		num += (ulong)((long)x[xOff] + (long)y[yOff] + z[zOff]);
		z[zOff] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 1] + (long)y[yOff + 1] + z[zOff + 1]);
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 2] + (long)y[yOff + 2] + z[zOff + 2]);
		z[zOff + 2] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 3] + (long)y[yOff + 3] + z[zOff + 3]);
		z[zOff + 3] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 4] + (long)y[yOff + 4] + z[zOff + 4]);
		z[zOff + 4] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 5] + (long)y[yOff + 5] + z[zOff + 5]);
		z[zOff + 5] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 6] + (long)y[yOff + 6] + z[zOff + 6]);
		z[zOff + 6] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 7] + (long)y[yOff + 7] + z[zOff + 7]);
		z[zOff + 7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint AddTo(uint[] x, uint[] z)
	{
		ulong num = 0uL;
		num += (ulong)((long)x[0] + (long)z[0]);
		z[0] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[1] + (long)z[1]);
		z[1] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[2] + (long)z[2]);
		z[2] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[3] + (long)z[3]);
		z[3] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[4] + (long)z[4]);
		z[4] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[5] + (long)z[5]);
		z[5] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[6] + (long)z[6]);
		z[6] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[7] + (long)z[7]);
		z[7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint AddTo(uint[] x, int xOff, uint[] z, int zOff, uint cIn)
	{
		ulong num = cIn;
		num += (ulong)((long)x[xOff] + (long)z[zOff]);
		z[zOff] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 1] + (long)z[zOff + 1]);
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 2] + (long)z[zOff + 2]);
		z[zOff + 2] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 3] + (long)z[zOff + 3]);
		z[zOff + 3] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 4] + (long)z[zOff + 4]);
		z[zOff + 4] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 5] + (long)z[zOff + 5]);
		z[zOff + 5] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 6] + (long)z[zOff + 6]);
		z[zOff + 6] = (uint)num;
		num >>= 32;
		num += (ulong)((long)x[xOff + 7] + (long)z[zOff + 7]);
		z[zOff + 7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint AddToEachOther(uint[] u, int uOff, uint[] v, int vOff)
	{
		ulong num = 0uL;
		num += (ulong)((long)u[uOff] + (long)v[vOff]);
		u[uOff] = (uint)num;
		v[vOff] = (uint)num;
		num >>= 32;
		num += (ulong)((long)u[uOff + 1] + (long)v[vOff + 1]);
		u[uOff + 1] = (uint)num;
		v[vOff + 1] = (uint)num;
		num >>= 32;
		num += (ulong)((long)u[uOff + 2] + (long)v[vOff + 2]);
		u[uOff + 2] = (uint)num;
		v[vOff + 2] = (uint)num;
		num >>= 32;
		num += (ulong)((long)u[uOff + 3] + (long)v[vOff + 3]);
		u[uOff + 3] = (uint)num;
		v[vOff + 3] = (uint)num;
		num >>= 32;
		num += (ulong)((long)u[uOff + 4] + (long)v[vOff + 4]);
		u[uOff + 4] = (uint)num;
		v[vOff + 4] = (uint)num;
		num >>= 32;
		num += (ulong)((long)u[uOff + 5] + (long)v[vOff + 5]);
		u[uOff + 5] = (uint)num;
		v[vOff + 5] = (uint)num;
		num >>= 32;
		num += (ulong)((long)u[uOff + 6] + (long)v[vOff + 6]);
		u[uOff + 6] = (uint)num;
		v[vOff + 6] = (uint)num;
		num >>= 32;
		num += (ulong)((long)u[uOff + 7] + (long)v[vOff + 7]);
		u[uOff + 7] = (uint)num;
		v[vOff + 7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static void Copy(uint[] x, uint[] z)
	{
		z[0] = x[0];
		z[1] = x[1];
		z[2] = x[2];
		z[3] = x[3];
		z[4] = x[4];
		z[5] = x[5];
		z[6] = x[6];
		z[7] = x[7];
	}

	public static void Copy(uint[] x, int xOff, uint[] z, int zOff)
	{
		z[zOff] = x[xOff];
		z[zOff + 1] = x[xOff + 1];
		z[zOff + 2] = x[xOff + 2];
		z[zOff + 3] = x[xOff + 3];
		z[zOff + 4] = x[xOff + 4];
		z[zOff + 5] = x[xOff + 5];
		z[zOff + 6] = x[xOff + 6];
		z[zOff + 7] = x[xOff + 7];
	}

	public static void Copy64(ulong[] x, ulong[] z)
	{
		z[0] = x[0];
		z[1] = x[1];
		z[2] = x[2];
		z[3] = x[3];
	}

	public static void Copy64(ulong[] x, int xOff, ulong[] z, int zOff)
	{
		z[zOff] = x[xOff];
		z[zOff + 1] = x[xOff + 1];
		z[zOff + 2] = x[xOff + 2];
		z[zOff + 3] = x[xOff + 3];
	}

	public static uint[] Create()
	{
		return new uint[8];
	}

	public static ulong[] Create64()
	{
		return new ulong[4];
	}

	public static uint[] CreateExt()
	{
		return new uint[16];
	}

	public static ulong[] CreateExt64()
	{
		return new ulong[8];
	}

	public static bool Diff(uint[] x, int xOff, uint[] y, int yOff, uint[] z, int zOff)
	{
		bool flag = Gte(x, xOff, y, yOff);
		if (flag)
		{
			Sub(x, xOff, y, yOff, z, zOff);
		}
		else
		{
			Sub(y, yOff, x, xOff, z, zOff);
		}
		return flag;
	}

	public static bool Eq(uint[] x, uint[] y)
	{
		for (int num = 7; num >= 0; num--)
		{
			if (x[num] != y[num])
			{
				return false;
			}
		}
		return true;
	}

	public static bool Eq64(ulong[] x, ulong[] y)
	{
		for (int num = 3; num >= 0; num--)
		{
			if (x[num] != y[num])
			{
				return false;
			}
		}
		return true;
	}

	public static uint GetBit(uint[] x, int bit)
	{
		if (bit == 0)
		{
			return x[0] & 1u;
		}
		if ((bit & 0xFF) != bit)
		{
			return 0u;
		}
		int num = bit >> 5;
		int num2 = bit & 0x1F;
		return (x[num] >> num2) & 1u;
	}

	public static bool Gte(uint[] x, uint[] y)
	{
		for (int num = 7; num >= 0; num--)
		{
			uint num2 = x[num];
			uint num3 = y[num];
			if (num2 < num3)
			{
				return false;
			}
			if (num2 > num3)
			{
				return true;
			}
		}
		return true;
	}

	public static bool Gte(uint[] x, int xOff, uint[] y, int yOff)
	{
		for (int num = 7; num >= 0; num--)
		{
			uint num2 = x[xOff + num];
			uint num3 = y[yOff + num];
			if (num2 < num3)
			{
				return false;
			}
			if (num2 > num3)
			{
				return true;
			}
		}
		return true;
	}

	public static bool IsOne(uint[] x)
	{
		if (x[0] != 1)
		{
			return false;
		}
		for (int i = 1; i < 8; i++)
		{
			if (x[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsOne64(ulong[] x)
	{
		if (x[0] != 1)
		{
			return false;
		}
		for (int i = 1; i < 4; i++)
		{
			if (x[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsZero(uint[] x)
	{
		for (int i = 0; i < 8; i++)
		{
			if (x[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsZero64(ulong[] x)
	{
		for (int i = 0; i < 4; i++)
		{
			if (x[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	public static void Mul(uint[] x, uint[] y, uint[] zz)
	{
		ulong num = y[0];
		ulong num2 = y[1];
		ulong num3 = y[2];
		ulong num4 = y[3];
		ulong num5 = y[4];
		ulong num6 = y[5];
		ulong num7 = y[6];
		ulong num8 = y[7];
		ulong num9 = 0uL;
		ulong num10 = x[0];
		num9 += num10 * num;
		zz[0] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num2;
		zz[1] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num3;
		zz[2] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num4;
		zz[3] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num5;
		zz[4] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num6;
		zz[5] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num7;
		zz[6] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num8;
		zz[7] = (uint)num9;
		num9 >>= 32;
		zz[8] = (uint)num9;
		for (int i = 1; i < 8; i++)
		{
			ulong num11 = 0uL;
			ulong num12 = x[i];
			num11 += num12 * num + zz[i];
			zz[i] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num2 + zz[i + 1];
			zz[i + 1] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num3 + zz[i + 2];
			zz[i + 2] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num4 + zz[i + 3];
			zz[i + 3] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num5 + zz[i + 4];
			zz[i + 4] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num6 + zz[i + 5];
			zz[i + 5] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num7 + zz[i + 6];
			zz[i + 6] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num8 + zz[i + 7];
			zz[i + 7] = (uint)num11;
			num11 >>= 32;
			zz[i + 8] = (uint)num11;
		}
	}

	public static void Mul(uint[] x, int xOff, uint[] y, int yOff, uint[] zz, int zzOff)
	{
		ulong num = y[yOff];
		ulong num2 = y[yOff + 1];
		ulong num3 = y[yOff + 2];
		ulong num4 = y[yOff + 3];
		ulong num5 = y[yOff + 4];
		ulong num6 = y[yOff + 5];
		ulong num7 = y[yOff + 6];
		ulong num8 = y[yOff + 7];
		ulong num9 = 0uL;
		ulong num10 = x[xOff];
		num9 += num10 * num;
		zz[zzOff] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num2;
		zz[zzOff + 1] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num3;
		zz[zzOff + 2] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num4;
		zz[zzOff + 3] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num5;
		zz[zzOff + 4] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num6;
		zz[zzOff + 5] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num7;
		zz[zzOff + 6] = (uint)num9;
		num9 >>= 32;
		num9 += num10 * num8;
		zz[zzOff + 7] = (uint)num9;
		num9 >>= 32;
		zz[zzOff + 8] = (uint)num9;
		for (int i = 1; i < 8; i++)
		{
			zzOff++;
			ulong num11 = 0uL;
			ulong num12 = x[xOff + i];
			num11 += num12 * num + zz[zzOff];
			zz[zzOff] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num2 + zz[zzOff + 1];
			zz[zzOff + 1] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num3 + zz[zzOff + 2];
			zz[zzOff + 2] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num4 + zz[zzOff + 3];
			zz[zzOff + 3] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num5 + zz[zzOff + 4];
			zz[zzOff + 4] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num6 + zz[zzOff + 5];
			zz[zzOff + 5] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num7 + zz[zzOff + 6];
			zz[zzOff + 6] = (uint)num11;
			num11 >>= 32;
			num11 += num12 * num8 + zz[zzOff + 7];
			zz[zzOff + 7] = (uint)num11;
			num11 >>= 32;
			zz[zzOff + 8] = (uint)num11;
		}
	}

	public static uint MulAddTo(uint[] x, uint[] y, uint[] zz)
	{
		ulong num = y[0];
		ulong num2 = y[1];
		ulong num3 = y[2];
		ulong num4 = y[3];
		ulong num5 = y[4];
		ulong num6 = y[5];
		ulong num7 = y[6];
		ulong num8 = y[7];
		ulong num9 = 0uL;
		for (int i = 0; i < 8; i++)
		{
			ulong num10 = 0uL;
			ulong num11 = x[i];
			num10 += num11 * num + zz[i];
			zz[i] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num2 + zz[i + 1];
			zz[i + 1] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num3 + zz[i + 2];
			zz[i + 2] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num4 + zz[i + 3];
			zz[i + 3] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num5 + zz[i + 4];
			zz[i + 4] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num6 + zz[i + 5];
			zz[i + 5] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num7 + zz[i + 6];
			zz[i + 6] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num8 + zz[i + 7];
			zz[i + 7] = (uint)num10;
			num10 >>= 32;
			num9 += num10 + zz[i + 8];
			zz[i + 8] = (uint)num9;
			num9 >>= 32;
		}
		return (uint)num9;
	}

	public static uint MulAddTo(uint[] x, int xOff, uint[] y, int yOff, uint[] zz, int zzOff)
	{
		ulong num = y[yOff];
		ulong num2 = y[yOff + 1];
		ulong num3 = y[yOff + 2];
		ulong num4 = y[yOff + 3];
		ulong num5 = y[yOff + 4];
		ulong num6 = y[yOff + 5];
		ulong num7 = y[yOff + 6];
		ulong num8 = y[yOff + 7];
		ulong num9 = 0uL;
		for (int i = 0; i < 8; i++)
		{
			ulong num10 = 0uL;
			ulong num11 = x[xOff + i];
			num10 += num11 * num + zz[zzOff];
			zz[zzOff] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num2 + zz[zzOff + 1];
			zz[zzOff + 1] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num3 + zz[zzOff + 2];
			zz[zzOff + 2] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num4 + zz[zzOff + 3];
			zz[zzOff + 3] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num5 + zz[zzOff + 4];
			zz[zzOff + 4] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num6 + zz[zzOff + 5];
			zz[zzOff + 5] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num7 + zz[zzOff + 6];
			zz[zzOff + 6] = (uint)num10;
			num10 >>= 32;
			num10 += num11 * num8 + zz[zzOff + 7];
			zz[zzOff + 7] = (uint)num10;
			num10 >>= 32;
			num9 += num10 + zz[zzOff + 8];
			zz[zzOff + 8] = (uint)num9;
			num9 >>= 32;
			zzOff++;
		}
		return (uint)num9;
	}

	public static ulong Mul33Add(uint w, uint[] x, int xOff, uint[] y, int yOff, uint[] z, int zOff)
	{
		ulong num = 0uL;
		ulong num2 = w;
		ulong num3 = x[xOff];
		num += num2 * num3 + y[yOff];
		z[zOff] = (uint)num;
		num >>= 32;
		ulong num4 = x[xOff + 1];
		num += num2 * num4 + num3 + y[yOff + 1];
		z[zOff + 1] = (uint)num;
		num >>= 32;
		ulong num5 = x[xOff + 2];
		num += num2 * num5 + num4 + y[yOff + 2];
		z[zOff + 2] = (uint)num;
		num >>= 32;
		ulong num6 = x[xOff + 3];
		num += num2 * num6 + num5 + y[yOff + 3];
		z[zOff + 3] = (uint)num;
		num >>= 32;
		ulong num7 = x[xOff + 4];
		num += num2 * num7 + num6 + y[yOff + 4];
		z[zOff + 4] = (uint)num;
		num >>= 32;
		ulong num8 = x[xOff + 5];
		num += num2 * num8 + num7 + y[yOff + 5];
		z[zOff + 5] = (uint)num;
		num >>= 32;
		ulong num9 = x[xOff + 6];
		num += num2 * num9 + num8 + y[yOff + 6];
		z[zOff + 6] = (uint)num;
		num >>= 32;
		ulong num10 = x[xOff + 7];
		num += num2 * num10 + num9 + y[yOff + 7];
		z[zOff + 7] = (uint)num;
		num >>= 32;
		return num + num10;
	}

	public static uint MulByWord(uint x, uint[] z)
	{
		ulong num = 0uL;
		ulong num2 = x;
		num += num2 * z[0];
		z[0] = (uint)num;
		num >>= 32;
		num += num2 * z[1];
		z[1] = (uint)num;
		num >>= 32;
		num += num2 * z[2];
		z[2] = (uint)num;
		num >>= 32;
		num += num2 * z[3];
		z[3] = (uint)num;
		num >>= 32;
		num += num2 * z[4];
		z[4] = (uint)num;
		num >>= 32;
		num += num2 * z[5];
		z[5] = (uint)num;
		num >>= 32;
		num += num2 * z[6];
		z[6] = (uint)num;
		num >>= 32;
		num += num2 * z[7];
		z[7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint MulByWordAddTo(uint x, uint[] y, uint[] z)
	{
		ulong num = 0uL;
		ulong num2 = x;
		num += num2 * z[0] + y[0];
		z[0] = (uint)num;
		num >>= 32;
		num += num2 * z[1] + y[1];
		z[1] = (uint)num;
		num >>= 32;
		num += num2 * z[2] + y[2];
		z[2] = (uint)num;
		num >>= 32;
		num += num2 * z[3] + y[3];
		z[3] = (uint)num;
		num >>= 32;
		num += num2 * z[4] + y[4];
		z[4] = (uint)num;
		num >>= 32;
		num += num2 * z[5] + y[5];
		z[5] = (uint)num;
		num >>= 32;
		num += num2 * z[6] + y[6];
		z[6] = (uint)num;
		num >>= 32;
		num += num2 * z[7] + y[7];
		z[7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint MulWordAddTo(uint x, uint[] y, int yOff, uint[] z, int zOff)
	{
		ulong num = 0uL;
		ulong num2 = x;
		num += num2 * y[yOff] + z[zOff];
		z[zOff] = (uint)num;
		num >>= 32;
		num += num2 * y[yOff + 1] + z[zOff + 1];
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += num2 * y[yOff + 2] + z[zOff + 2];
		z[zOff + 2] = (uint)num;
		num >>= 32;
		num += num2 * y[yOff + 3] + z[zOff + 3];
		z[zOff + 3] = (uint)num;
		num >>= 32;
		num += num2 * y[yOff + 4] + z[zOff + 4];
		z[zOff + 4] = (uint)num;
		num >>= 32;
		num += num2 * y[yOff + 5] + z[zOff + 5];
		z[zOff + 5] = (uint)num;
		num >>= 32;
		num += num2 * y[yOff + 6] + z[zOff + 6];
		z[zOff + 6] = (uint)num;
		num >>= 32;
		num += num2 * y[yOff + 7] + z[zOff + 7];
		z[zOff + 7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	public static uint Mul33DWordAdd(uint x, ulong y, uint[] z, int zOff)
	{
		ulong num = 0uL;
		ulong num2 = x;
		ulong num3 = y & 0xFFFFFFFFu;
		num += num2 * num3 + z[zOff];
		z[zOff] = (uint)num;
		num >>= 32;
		ulong num4 = y >> 32;
		num += num2 * num4 + num3 + z[zOff + 1];
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += num4 + z[zOff + 2];
		z[zOff + 2] = (uint)num;
		num >>= 32;
		num += z[zOff + 3];
		z[zOff + 3] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			return Nat.IncAt(8, z, zOff, 4);
		}
		return 0u;
	}

	public static uint Mul33WordAdd(uint x, uint y, uint[] z, int zOff)
	{
		ulong num = 0uL;
		ulong num2 = y;
		num += num2 * x + z[zOff];
		z[zOff] = (uint)num;
		num >>= 32;
		num += num2 + z[zOff + 1];
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += z[zOff + 2];
		z[zOff + 2] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			return Nat.IncAt(8, z, zOff, 3);
		}
		return 0u;
	}

	public static uint MulWordDwordAdd(uint x, ulong y, uint[] z, int zOff)
	{
		ulong num = 0uL;
		ulong num2 = x;
		num += num2 * y + z[zOff];
		z[zOff] = (uint)num;
		num >>= 32;
		num += num2 * (y >> 32) + z[zOff + 1];
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += z[zOff + 2];
		z[zOff + 2] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			return Nat.IncAt(8, z, zOff, 3);
		}
		return 0u;
	}

	public static uint MulWord(uint x, uint[] y, uint[] z, int zOff)
	{
		ulong num = 0uL;
		ulong num2 = x;
		int num3 = 0;
		do
		{
			num += num2 * y[num3];
			z[zOff + num3] = (uint)num;
			num >>= 32;
		}
		while (++num3 < 8);
		return (uint)num;
	}

	public static void Square(uint[] x, uint[] zz)
	{
		ulong num = x[0];
		uint num2 = 0u;
		int num3 = 7;
		int num4 = 16;
		do
		{
			ulong num5 = x[num3--];
			ulong num6 = num5 * num5;
			zz[--num4] = (num2 << 31) | (uint)(int)(num6 >> 33);
			zz[--num4] = (uint)(num6 >> 1);
			num2 = (uint)num6;
		}
		while (num3 > 0);
		ulong num7 = num * num;
		ulong num8 = (num2 << 31) | (num7 >> 33);
		zz[0] = (uint)num7;
		num2 = (uint)(int)(num7 >> 32) & 1u;
		ulong num9 = x[1];
		ulong num10 = zz[2];
		num8 += num9 * num;
		uint num11 = (uint)num8;
		zz[1] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num10 += num8 >> 32;
		ulong num12 = x[2];
		ulong num13 = zz[3];
		ulong num14 = zz[4];
		num10 += num12 * num;
		num11 = (uint)num10;
		zz[2] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num13 += (num10 >> 32) + num12 * num9;
		num14 += num13 >> 32;
		num13 &= 0xFFFFFFFFu;
		ulong num15 = x[3];
		ulong num16 = zz[5] + (num14 >> 32);
		num14 &= 0xFFFFFFFFu;
		ulong num17 = zz[6] + (num16 >> 32);
		num16 &= 0xFFFFFFFFu;
		num13 += num15 * num;
		num11 = (uint)num13;
		zz[3] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num14 += (num13 >> 32) + num15 * num9;
		num16 += (num14 >> 32) + num15 * num12;
		num14 &= 0xFFFFFFFFu;
		num17 += num16 >> 32;
		num16 &= 0xFFFFFFFFu;
		ulong num18 = x[4];
		ulong num19 = zz[7] + (num17 >> 32);
		num17 &= 0xFFFFFFFFu;
		ulong num20 = zz[8] + (num19 >> 32);
		num19 &= 0xFFFFFFFFu;
		num14 += num18 * num;
		num11 = (uint)num14;
		zz[4] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num16 += (num14 >> 32) + num18 * num9;
		num17 += (num16 >> 32) + num18 * num12;
		num16 &= 0xFFFFFFFFu;
		num19 += (num17 >> 32) + num18 * num15;
		num17 &= 0xFFFFFFFFu;
		num20 += num19 >> 32;
		num19 &= 0xFFFFFFFFu;
		ulong num21 = x[5];
		ulong num22 = zz[9] + (num20 >> 32);
		num20 &= 0xFFFFFFFFu;
		ulong num23 = zz[10] + (num22 >> 32);
		num22 &= 0xFFFFFFFFu;
		num16 += num21 * num;
		num11 = (uint)num16;
		zz[5] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num17 += (num16 >> 32) + num21 * num9;
		num19 += (num17 >> 32) + num21 * num12;
		num17 &= 0xFFFFFFFFu;
		num20 += (num19 >> 32) + num21 * num15;
		num19 &= 0xFFFFFFFFu;
		num22 += (num20 >> 32) + num21 * num18;
		num20 &= 0xFFFFFFFFu;
		num23 += num22 >> 32;
		num22 &= 0xFFFFFFFFu;
		ulong num24 = x[6];
		ulong num25 = zz[11] + (num23 >> 32);
		num23 &= 0xFFFFFFFFu;
		ulong num26 = zz[12] + (num25 >> 32);
		num25 &= 0xFFFFFFFFu;
		num17 += num24 * num;
		num11 = (uint)num17;
		zz[6] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num19 += (num17 >> 32) + num24 * num9;
		num20 += (num19 >> 32) + num24 * num12;
		num19 &= 0xFFFFFFFFu;
		num22 += (num20 >> 32) + num24 * num15;
		num20 &= 0xFFFFFFFFu;
		num23 += (num22 >> 32) + num24 * num18;
		num22 &= 0xFFFFFFFFu;
		num25 += (num23 >> 32) + num24 * num21;
		num23 &= 0xFFFFFFFFu;
		num26 += num25 >> 32;
		num25 &= 0xFFFFFFFFu;
		ulong num27 = x[7];
		ulong num28 = zz[13] + (num26 >> 32);
		num26 &= 0xFFFFFFFFu;
		ulong num29 = zz[14] + (num28 >> 32);
		num28 &= 0xFFFFFFFFu;
		num19 += num27 * num;
		num11 = (uint)num19;
		zz[7] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num20 += (num19 >> 32) + num27 * num9;
		num22 += (num20 >> 32) + num27 * num12;
		num23 += (num22 >> 32) + num27 * num15;
		num25 += (num23 >> 32) + num27 * num18;
		num26 += (num25 >> 32) + num27 * num21;
		num28 += (num26 >> 32) + num27 * num24;
		num29 += num28 >> 32;
		num11 = (uint)num20;
		zz[8] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num22;
		zz[9] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num23;
		zz[10] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num25;
		zz[11] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num26;
		zz[12] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num28;
		zz[13] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num29;
		zz[14] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = zz[15] + (uint)(int)(num29 >> 32);
		zz[15] = (num11 << 1) | num2;
	}

	public static void Square(uint[] x, int xOff, uint[] zz, int zzOff)
	{
		ulong num = x[xOff];
		uint num2 = 0u;
		int num3 = 7;
		int num4 = 16;
		do
		{
			ulong num5 = x[xOff + num3--];
			ulong num6 = num5 * num5;
			zz[zzOff + --num4] = (num2 << 31) | (uint)(int)(num6 >> 33);
			zz[zzOff + --num4] = (uint)(num6 >> 1);
			num2 = (uint)num6;
		}
		while (num3 > 0);
		ulong num7 = num * num;
		ulong num8 = (num2 << 31) | (num7 >> 33);
		zz[zzOff] = (uint)num7;
		num2 = (uint)(int)(num7 >> 32) & 1u;
		ulong num9 = x[xOff + 1];
		ulong num10 = zz[zzOff + 2];
		num8 += num9 * num;
		uint num11 = (uint)num8;
		zz[zzOff + 1] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num10 += num8 >> 32;
		ulong num12 = x[xOff + 2];
		ulong num13 = zz[zzOff + 3];
		ulong num14 = zz[zzOff + 4];
		num10 += num12 * num;
		num11 = (uint)num10;
		zz[zzOff + 2] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num13 += (num10 >> 32) + num12 * num9;
		num14 += num13 >> 32;
		num13 &= 0xFFFFFFFFu;
		ulong num15 = x[xOff + 3];
		ulong num16 = zz[zzOff + 5] + (num14 >> 32);
		num14 &= 0xFFFFFFFFu;
		ulong num17 = zz[zzOff + 6] + (num16 >> 32);
		num16 &= 0xFFFFFFFFu;
		num13 += num15 * num;
		num11 = (uint)num13;
		zz[zzOff + 3] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num14 += (num13 >> 32) + num15 * num9;
		num16 += (num14 >> 32) + num15 * num12;
		num14 &= 0xFFFFFFFFu;
		num17 += num16 >> 32;
		num16 &= 0xFFFFFFFFu;
		ulong num18 = x[xOff + 4];
		ulong num19 = zz[zzOff + 7] + (num17 >> 32);
		num17 &= 0xFFFFFFFFu;
		ulong num20 = zz[zzOff + 8] + (num19 >> 32);
		num19 &= 0xFFFFFFFFu;
		num14 += num18 * num;
		num11 = (uint)num14;
		zz[zzOff + 4] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num16 += (num14 >> 32) + num18 * num9;
		num17 += (num16 >> 32) + num18 * num12;
		num16 &= 0xFFFFFFFFu;
		num19 += (num17 >> 32) + num18 * num15;
		num17 &= 0xFFFFFFFFu;
		num20 += num19 >> 32;
		num19 &= 0xFFFFFFFFu;
		ulong num21 = x[xOff + 5];
		ulong num22 = zz[zzOff + 9] + (num20 >> 32);
		num20 &= 0xFFFFFFFFu;
		ulong num23 = zz[zzOff + 10] + (num22 >> 32);
		num22 &= 0xFFFFFFFFu;
		num16 += num21 * num;
		num11 = (uint)num16;
		zz[zzOff + 5] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num17 += (num16 >> 32) + num21 * num9;
		num19 += (num17 >> 32) + num21 * num12;
		num17 &= 0xFFFFFFFFu;
		num20 += (num19 >> 32) + num21 * num15;
		num19 &= 0xFFFFFFFFu;
		num22 += (num20 >> 32) + num21 * num18;
		num20 &= 0xFFFFFFFFu;
		num23 += num22 >> 32;
		num22 &= 0xFFFFFFFFu;
		ulong num24 = x[xOff + 6];
		ulong num25 = zz[zzOff + 11] + (num23 >> 32);
		num23 &= 0xFFFFFFFFu;
		ulong num26 = zz[zzOff + 12] + (num25 >> 32);
		num25 &= 0xFFFFFFFFu;
		num17 += num24 * num;
		num11 = (uint)num17;
		zz[zzOff + 6] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num19 += (num17 >> 32) + num24 * num9;
		num20 += (num19 >> 32) + num24 * num12;
		num19 &= 0xFFFFFFFFu;
		num22 += (num20 >> 32) + num24 * num15;
		num20 &= 0xFFFFFFFFu;
		num23 += (num22 >> 32) + num24 * num18;
		num22 &= 0xFFFFFFFFu;
		num25 += (num23 >> 32) + num24 * num21;
		num23 &= 0xFFFFFFFFu;
		num26 += num25 >> 32;
		num25 &= 0xFFFFFFFFu;
		ulong num27 = x[xOff + 7];
		ulong num28 = zz[zzOff + 13] + (num26 >> 32);
		num26 &= 0xFFFFFFFFu;
		ulong num29 = zz[zzOff + 14] + (num28 >> 32);
		num28 &= 0xFFFFFFFFu;
		num19 += num27 * num;
		num11 = (uint)num19;
		zz[zzOff + 7] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num20 += (num19 >> 32) + num27 * num9;
		num22 += (num20 >> 32) + num27 * num12;
		num23 += (num22 >> 32) + num27 * num15;
		num25 += (num23 >> 32) + num27 * num18;
		num26 += (num25 >> 32) + num27 * num21;
		num28 += (num26 >> 32) + num27 * num24;
		num29 += num28 >> 32;
		num11 = (uint)num20;
		zz[zzOff + 8] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num22;
		zz[zzOff + 9] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num23;
		zz[zzOff + 10] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num25;
		zz[zzOff + 11] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num26;
		zz[zzOff + 12] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num28;
		zz[zzOff + 13] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = (uint)num29;
		zz[zzOff + 14] = (num11 << 1) | num2;
		num2 = num11 >> 31;
		num11 = zz[zzOff + 15] + (uint)(int)(num29 >> 32);
		zz[zzOff + 15] = (num11 << 1) | num2;
	}

	public static int Sub(uint[] x, uint[] y, uint[] z)
	{
		long num = 0L;
		num += (long)x[0] - (long)y[0];
		z[0] = (uint)num;
		num >>= 32;
		num += (long)x[1] - (long)y[1];
		z[1] = (uint)num;
		num >>= 32;
		num += (long)x[2] - (long)y[2];
		z[2] = (uint)num;
		num >>= 32;
		num += (long)x[3] - (long)y[3];
		z[3] = (uint)num;
		num >>= 32;
		num += (long)x[4] - (long)y[4];
		z[4] = (uint)num;
		num >>= 32;
		num += (long)x[5] - (long)y[5];
		z[5] = (uint)num;
		num >>= 32;
		num += (long)x[6] - (long)y[6];
		z[6] = (uint)num;
		num >>= 32;
		num += (long)x[7] - (long)y[7];
		z[7] = (uint)num;
		num >>= 32;
		return (int)num;
	}

	public static int Sub(uint[] x, int xOff, uint[] y, int yOff, uint[] z, int zOff)
	{
		long num = 0L;
		num += (long)x[xOff] - (long)y[yOff];
		z[zOff] = (uint)num;
		num >>= 32;
		num += (long)x[xOff + 1] - (long)y[yOff + 1];
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += (long)x[xOff + 2] - (long)y[yOff + 2];
		z[zOff + 2] = (uint)num;
		num >>= 32;
		num += (long)x[xOff + 3] - (long)y[yOff + 3];
		z[zOff + 3] = (uint)num;
		num >>= 32;
		num += (long)x[xOff + 4] - (long)y[yOff + 4];
		z[zOff + 4] = (uint)num;
		num >>= 32;
		num += (long)x[xOff + 5] - (long)y[yOff + 5];
		z[zOff + 5] = (uint)num;
		num >>= 32;
		num += (long)x[xOff + 6] - (long)y[yOff + 6];
		z[zOff + 6] = (uint)num;
		num >>= 32;
		num += (long)x[xOff + 7] - (long)y[yOff + 7];
		z[zOff + 7] = (uint)num;
		num >>= 32;
		return (int)num;
	}

	public static int SubBothFrom(uint[] x, uint[] y, uint[] z)
	{
		long num = 0L;
		num += (long)z[0] - (long)x[0] - y[0];
		z[0] = (uint)num;
		num >>= 32;
		num += (long)z[1] - (long)x[1] - y[1];
		z[1] = (uint)num;
		num >>= 32;
		num += (long)z[2] - (long)x[2] - y[2];
		z[2] = (uint)num;
		num >>= 32;
		num += (long)z[3] - (long)x[3] - y[3];
		z[3] = (uint)num;
		num >>= 32;
		num += (long)z[4] - (long)x[4] - y[4];
		z[4] = (uint)num;
		num >>= 32;
		num += (long)z[5] - (long)x[5] - y[5];
		z[5] = (uint)num;
		num >>= 32;
		num += (long)z[6] - (long)x[6] - y[6];
		z[6] = (uint)num;
		num >>= 32;
		num += (long)z[7] - (long)x[7] - y[7];
		z[7] = (uint)num;
		num >>= 32;
		return (int)num;
	}

	public static int SubFrom(uint[] x, uint[] z)
	{
		long num = 0L;
		num += (long)z[0] - (long)x[0];
		z[0] = (uint)num;
		num >>= 32;
		num += (long)z[1] - (long)x[1];
		z[1] = (uint)num;
		num >>= 32;
		num += (long)z[2] - (long)x[2];
		z[2] = (uint)num;
		num >>= 32;
		num += (long)z[3] - (long)x[3];
		z[3] = (uint)num;
		num >>= 32;
		num += (long)z[4] - (long)x[4];
		z[4] = (uint)num;
		num >>= 32;
		num += (long)z[5] - (long)x[5];
		z[5] = (uint)num;
		num >>= 32;
		num += (long)z[6] - (long)x[6];
		z[6] = (uint)num;
		num >>= 32;
		num += (long)z[7] - (long)x[7];
		z[7] = (uint)num;
		num >>= 32;
		return (int)num;
	}

	public static int SubFrom(uint[] x, int xOff, uint[] z, int zOff)
	{
		long num = 0L;
		num += (long)z[zOff] - (long)x[xOff];
		z[zOff] = (uint)num;
		num >>= 32;
		num += (long)z[zOff + 1] - (long)x[xOff + 1];
		z[zOff + 1] = (uint)num;
		num >>= 32;
		num += (long)z[zOff + 2] - (long)x[xOff + 2];
		z[zOff + 2] = (uint)num;
		num >>= 32;
		num += (long)z[zOff + 3] - (long)x[xOff + 3];
		z[zOff + 3] = (uint)num;
		num >>= 32;
		num += (long)z[zOff + 4] - (long)x[xOff + 4];
		z[zOff + 4] = (uint)num;
		num >>= 32;
		num += (long)z[zOff + 5] - (long)x[xOff + 5];
		z[zOff + 5] = (uint)num;
		num >>= 32;
		num += (long)z[zOff + 6] - (long)x[xOff + 6];
		z[zOff + 6] = (uint)num;
		num >>= 32;
		num += (long)z[zOff + 7] - (long)x[xOff + 7];
		z[zOff + 7] = (uint)num;
		num >>= 32;
		return (int)num;
	}

	public static BigInteger ToBigInteger(uint[] x)
	{
		byte[] array = new byte[32];
		for (int i = 0; i < 8; i++)
		{
			uint num = x[i];
			if (num != 0)
			{
				Pack.UInt32_To_BE(num, array, 7 - i << 2);
			}
		}
		return new BigInteger(1, array);
	}

	public static BigInteger ToBigInteger64(ulong[] x)
	{
		byte[] array = new byte[32];
		for (int i = 0; i < 4; i++)
		{
			ulong num = x[i];
			if (num != 0)
			{
				Pack.UInt64_To_BE(num, array, 3 - i << 3);
			}
		}
		return new BigInteger(1, array);
	}

	public static void Zero(uint[] z)
	{
		z[0] = 0u;
		z[1] = 0u;
		z[2] = 0u;
		z[3] = 0u;
		z[4] = 0u;
		z[5] = 0u;
		z[6] = 0u;
		z[7] = 0u;
	}
}
