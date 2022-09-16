using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP224R1Field
{
	private const uint P6 = uint.MaxValue;

	private const uint PExt13 = uint.MaxValue;

	internal static readonly uint[] P = new uint[7] { 1u, 0u, 0u, 4294967295u, 4294967295u, 4294967295u, 4294967295u };

	private static readonly uint[] PExt = new uint[14]
	{
		1u, 0u, 0u, 4294967294u, 4294967295u, 4294967295u, 0u, 2u, 0u, 0u,
		4294967294u, 4294967295u, 4294967295u, 4294967295u
	};

	private static readonly uint[] PExtInv = new uint[11]
	{
		4294967295u, 4294967295u, 4294967295u, 1u, 0u, 0u, 4294967295u, 4294967293u, 4294967295u, 4294967295u,
		1u
	};

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		if (Nat224.Add(x, y, z) != 0 || (z[6] == uint.MaxValue && Nat224.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void AddExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if ((Nat.Add(14, xx, yy, zz) != 0 || (zz[13] == uint.MaxValue && Nat.Gte(14, zz, PExt))) && Nat.AddTo(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.IncAt(14, zz, PExtInv.Length);
		}
	}

	public static void AddOne(uint[] x, uint[] z)
	{
		if (Nat.Inc(7, x, z) != 0 || (z[6] == uint.MaxValue && Nat224.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static uint[] FromBigInteger(BigInteger x)
	{
		uint[] array = Nat.FromBigInteger(224, x);
		if (array[6] == uint.MaxValue && Nat224.Gte(array, P))
		{
			Nat224.SubFrom(P, array);
		}
		return array;
	}

	public static void Half(uint[] x, uint[] z)
	{
		if ((x[0] & 1) == 0)
		{
			Nat.ShiftDownBit(7, x, 0u, z);
			return;
		}
		uint c = Nat224.Add(x, P, z);
		Nat.ShiftDownBit(7, z, c);
	}

	public static void Inv(uint[] x, uint[] z)
	{
		Mod.CheckedModOddInverse(P, x, z);
	}

	public static int IsZero(uint[] x)
	{
		uint num = 0u;
		for (int i = 0; i < 7; i++)
		{
			num |= x[i];
		}
		num = (num >> 1) | (num & 1u);
		return (int)(num - 1) >> 31;
	}

	public static void Multiply(uint[] x, uint[] y, uint[] z)
	{
		uint[] array = Nat224.CreateExt();
		Nat224.Mul(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(uint[] x, uint[] y, uint[] zz)
	{
		if ((Nat224.MulAddTo(x, y, zz) != 0 || (zz[13] == uint.MaxValue && Nat.Gte(14, zz, PExt))) && Nat.AddTo(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.IncAt(14, zz, PExtInv.Length);
		}
	}

	public static void Negate(uint[] x, uint[] z)
	{
		if (IsZero(x) != 0)
		{
			Nat224.Sub(P, P, z);
		}
		else
		{
			Nat224.Sub(P, x, z);
		}
	}

	public static void Random(SecureRandom r, uint[] z)
	{
		byte[] array = new byte[28];
		do
		{
			r.NextBytes(array);
			Pack.LE_To_UInt32(array, 0, z, 0, 7);
		}
		while (Nat.LessThan(7, z, P) == 0);
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
		long num = xx[10];
		long num2 = xx[11];
		long num3 = xx[12];
		long num4 = xx[13];
		long num5 = xx[7] + num2 - 1;
		long num6 = xx[8] + num3;
		long num7 = xx[9] + num4;
		long num8 = 0L;
		num8 += xx[0] - num5;
		long num9 = (uint)num8;
		num8 >>= 32;
		num8 += xx[1] - num6;
		z[1] = (uint)num8;
		num8 >>= 32;
		num8 += xx[2] - num7;
		z[2] = (uint)num8;
		num8 >>= 32;
		num8 += xx[3] + num5 - num;
		long num10 = (uint)num8;
		num8 >>= 32;
		num8 += xx[4] + num6 - num2;
		z[4] = (uint)num8;
		num8 >>= 32;
		num8 += xx[5] + num7 - num3;
		z[5] = (uint)num8;
		num8 >>= 32;
		num8 += xx[6] + num - num4;
		z[6] = (uint)num8;
		num8 >>= 32;
		num8++;
		num10 += num8;
		num9 -= num8;
		z[0] = (uint)num9;
		num8 = num9 >> 32;
		if (num8 != 0)
		{
			num8 += z[1];
			z[1] = (uint)num8;
			num8 >>= 32;
			num8 += z[2];
			z[2] = (uint)num8;
			num10 += num8 >> 32;
		}
		z[3] = (uint)num10;
		num8 = num10 >> 32;
		if ((num8 != 0 && Nat.IncAt(7, z, 4) != 0) || (z[6] == uint.MaxValue && Nat224.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void Reduce32(uint x, uint[] z)
	{
		long num = 0L;
		if (x != 0)
		{
			long num2 = x;
			num += z[0] - num2;
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
			num += z[3] + num2;
			z[3] = (uint)num;
			num >>= 32;
		}
		if ((num != 0 && Nat.IncAt(7, z, 4) != 0) || (z[6] == uint.MaxValue && Nat224.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void Square(uint[] x, uint[] z)
	{
		uint[] array = Nat224.CreateExt();
		Nat224.Square(x, array);
		Reduce(array, z);
	}

	public static void SquareN(uint[] x, int n, uint[] z)
	{
		uint[] array = Nat224.CreateExt();
		Nat224.Square(x, array);
		Reduce(array, z);
		while (--n > 0)
		{
			Nat224.Square(z, array);
			Reduce(array, z);
		}
	}

	public static void Subtract(uint[] x, uint[] y, uint[] z)
	{
		if (Nat224.Sub(x, y, z) != 0)
		{
			SubPInvFrom(z);
		}
	}

	public static void SubtractExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if (Nat.Sub(14, xx, yy, zz) != 0 && Nat.SubFrom(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.DecAt(14, zz, PExtInv.Length);
		}
	}

	public static void Twice(uint[] x, uint[] z)
	{
		if (Nat.ShiftUpBit(7, x, 0u, z) != 0 || (z[6] == uint.MaxValue && Nat224.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	private static void AddPInvTo(uint[] z)
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
		num += (long)z[3] + 1L;
		z[3] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			Nat.IncAt(7, z, 4);
		}
	}

	private static void SubPInvFrom(uint[] z)
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
		num += (long)z[3] - 1L;
		z[3] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			Nat.DecAt(7, z, 4);
		}
	}
}
