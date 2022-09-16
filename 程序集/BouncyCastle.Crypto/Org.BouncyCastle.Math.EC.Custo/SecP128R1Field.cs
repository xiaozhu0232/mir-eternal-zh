using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP128R1Field
{
	private const uint P3 = 4294967293u;

	private const uint PExt7 = 4294967292u;

	internal static readonly uint[] P = new uint[4] { 4294967295u, 4294967295u, 4294967295u, 4294967293u };

	private static readonly uint[] PExt = new uint[8] { 1u, 0u, 0u, 4u, 4294967294u, 4294967295u, 3u, 4294967292u };

	private static readonly uint[] PExtInv = new uint[8] { 4294967295u, 4294967295u, 4294967295u, 4294967291u, 1u, 0u, 4294967292u, 3u };

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		if (Nat128.Add(x, y, z) != 0 || (z[3] >= 4294967293u && Nat128.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void AddExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if (Nat256.Add(xx, yy, zz) != 0 || (zz[7] >= 4294967292u && Nat256.Gte(zz, PExt)))
		{
			Nat.AddTo(PExtInv.Length, PExtInv, zz);
		}
	}

	public static void AddOne(uint[] x, uint[] z)
	{
		if (Nat.Inc(4, x, z) != 0 || (z[3] >= 4294967293u && Nat128.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static uint[] FromBigInteger(BigInteger x)
	{
		uint[] array = Nat.FromBigInteger(128, x);
		if (array[3] >= 4294967293u && Nat128.Gte(array, P))
		{
			Nat128.SubFrom(P, array);
		}
		return array;
	}

	public static void Half(uint[] x, uint[] z)
	{
		if ((x[0] & 1) == 0)
		{
			Nat.ShiftDownBit(4, x, 0u, z);
			return;
		}
		uint c = Nat128.Add(x, P, z);
		Nat.ShiftDownBit(4, z, c);
	}

	public static void Inv(uint[] x, uint[] z)
	{
		Mod.CheckedModOddInverse(P, x, z);
	}

	public static int IsZero(uint[] x)
	{
		uint num = 0u;
		for (int i = 0; i < 4; i++)
		{
			num |= x[i];
		}
		num = (num >> 1) | (num & 1u);
		return (int)(num - 1) >> 31;
	}

	public static void Multiply(uint[] x, uint[] y, uint[] z)
	{
		uint[] array = Nat128.CreateExt();
		Nat128.Mul(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(uint[] x, uint[] y, uint[] zz)
	{
		if (Nat128.MulAddTo(x, y, zz) != 0 || (zz[7] >= 4294967292u && Nat256.Gte(zz, PExt)))
		{
			Nat.AddTo(PExtInv.Length, PExtInv, zz);
		}
	}

	public static void Negate(uint[] x, uint[] z)
	{
		if (IsZero(x) != 0)
		{
			Nat128.Sub(P, P, z);
		}
		else
		{
			Nat128.Sub(P, x, z);
		}
	}

	public static void Random(SecureRandom r, uint[] z)
	{
		byte[] array = new byte[16];
		do
		{
			r.NextBytes(array);
			Pack.LE_To_UInt32(array, 0, z, 0, 4);
		}
		while (Nat.LessThan(4, z, P) == 0);
	}

	public static void RandomMult(SecureRandom r, uint[] z)
	{
		do
		{
			Random(r, z);
		}
		while (IsZero(z) != 0);
	}

	public static void Reduce(uint[] xx, uint[] z)
	{
		ulong num = xx[0];
		ulong num2 = xx[1];
		ulong num3 = xx[2];
		ulong num4 = xx[3];
		ulong num5 = xx[4];
		ulong num6 = xx[5];
		ulong num7 = xx[6];
		ulong num8 = xx[7];
		num4 += num8;
		num7 += num8 << 1;
		num3 += num7;
		num6 += num7 << 1;
		num2 += num6;
		num5 += num6 << 1;
		num += num5;
		num4 += num5 << 1;
		z[0] = (uint)num;
		num2 += num >> 32;
		z[1] = (uint)num2;
		num3 += num2 >> 32;
		z[2] = (uint)num3;
		num4 += num3 >> 32;
		z[3] = (uint)num4;
		Reduce32((uint)(num4 >> 32), z);
	}

	public static void Reduce32(uint x, uint[] z)
	{
		while (x != 0)
		{
			ulong num = x;
			ulong num2 = z[0] + num;
			z[0] = (uint)num2;
			num2 >>= 32;
			if (num2 != 0)
			{
				num2 += z[1];
				z[1] = (uint)num2;
				num2 >>= 32;
				num2 += z[2];
				z[2] = (uint)num2;
				num2 >>= 32;
			}
			num2 += z[3] + (num << 1);
			z[3] = (uint)num2;
			num2 >>= 32;
			x = (uint)num2;
		}
		if (z[3] >= 4294967293u && Nat128.Gte(z, P))
		{
			AddPInvTo(z);
		}
	}

	public static void Square(uint[] x, uint[] z)
	{
		uint[] array = Nat128.CreateExt();
		Nat128.Square(x, array);
		Reduce(array, z);
	}

	public static void SquareN(uint[] x, int n, uint[] z)
	{
		uint[] array = Nat128.CreateExt();
		Nat128.Square(x, array);
		Reduce(array, z);
		while (--n > 0)
		{
			Nat128.Square(z, array);
			Reduce(array, z);
		}
	}

	public static void Subtract(uint[] x, uint[] y, uint[] z)
	{
		if (Nat128.Sub(x, y, z) != 0)
		{
			SubPInvFrom(z);
		}
	}

	public static void SubtractExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if (Nat.Sub(10, xx, yy, zz) != 0)
		{
			Nat.SubFrom(PExtInv.Length, PExtInv, zz);
		}
	}

	public static void Twice(uint[] x, uint[] z)
	{
		if (Nat.ShiftUpBit(4, x, 0u, z) != 0 || (z[3] >= 4294967293u && Nat128.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	private static void AddPInvTo(uint[] z)
	{
		long num = (long)z[0] + 1L;
		z[0] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num += z[1];
			z[1] = (uint)num;
			num >>= 32;
			num += z[2];
			z[2] = (uint)num;
			num >>= 32;
		}
		num += (long)z[3] + 2L;
		z[3] = (uint)num;
	}

	private static void SubPInvFrom(uint[] z)
	{
		long num = (long)z[0] - 1L;
		z[0] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num += z[1];
			z[1] = (uint)num;
			num >>= 32;
			num += z[2];
			z[2] = (uint)num;
			num >>= 32;
		}
		num += (long)z[3] - 2L;
		z[3] = (uint)num;
	}
}
