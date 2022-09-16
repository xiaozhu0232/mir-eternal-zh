using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP384R1Field
{
	private const uint P11 = uint.MaxValue;

	private const uint PExt23 = uint.MaxValue;

	internal static readonly uint[] P = new uint[12]
	{
		4294967295u, 0u, 0u, 4294967295u, 4294967294u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u,
		4294967295u, 4294967295u
	};

	private static readonly uint[] PExt = new uint[24]
	{
		1u, 4294967294u, 0u, 2u, 0u, 4294967294u, 0u, 2u, 1u, 0u,
		0u, 0u, 4294967294u, 1u, 0u, 4294967294u, 4294967293u, 4294967295u, 4294967295u, 4294967295u,
		4294967295u, 4294967295u, 4294967295u, 4294967295u
	};

	private static readonly uint[] PExtInv = new uint[17]
	{
		4294967295u, 1u, 4294967295u, 4294967293u, 4294967295u, 1u, 4294967295u, 4294967293u, 4294967294u, 4294967295u,
		4294967295u, 4294967295u, 1u, 4294967294u, 4294967295u, 1u, 2u
	};

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		if (Nat.Add(12, x, y, z) != 0 || (z[11] == uint.MaxValue && Nat.Gte(12, z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void AddExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if ((Nat.Add(24, xx, yy, zz) != 0 || (zz[23] == uint.MaxValue && Nat.Gte(24, zz, PExt))) && Nat.AddTo(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.IncAt(24, zz, PExtInv.Length);
		}
	}

	public static void AddOne(uint[] x, uint[] z)
	{
		if (Nat.Inc(12, x, z) != 0 || (z[11] == uint.MaxValue && Nat.Gte(12, z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static uint[] FromBigInteger(BigInteger x)
	{
		uint[] array = Nat.FromBigInteger(384, x);
		if (array[11] == uint.MaxValue && Nat.Gte(12, array, P))
		{
			Nat.SubFrom(12, P, array);
		}
		return array;
	}

	public static void Half(uint[] x, uint[] z)
	{
		if ((x[0] & 1) == 0)
		{
			Nat.ShiftDownBit(12, x, 0u, z);
			return;
		}
		uint c = Nat.Add(12, x, P, z);
		Nat.ShiftDownBit(12, z, c);
	}

	public static void Inv(uint[] x, uint[] z)
	{
		Mod.CheckedModOddInverse(P, x, z);
	}

	public static int IsZero(uint[] x)
	{
		uint num = 0u;
		for (int i = 0; i < 12; i++)
		{
			num |= x[i];
		}
		num = (num >> 1) | (num & 1u);
		return (int)(num - 1) >> 31;
	}

	public static void Multiply(uint[] x, uint[] y, uint[] z)
	{
		uint[] array = Nat.Create(24);
		Nat384.Mul(x, y, array);
		Reduce(array, z);
	}

	public static void Negate(uint[] x, uint[] z)
	{
		if (IsZero(x) != 0)
		{
			Nat.Sub(12, P, P, z);
		}
		else
		{
			Nat.Sub(12, P, x, z);
		}
	}

	public static void Random(SecureRandom r, uint[] z)
	{
		byte[] array = new byte[48];
		do
		{
			r.NextBytes(array);
			Pack.LE_To_UInt32(array, 0, z, 0, 12);
		}
		while (Nat.LessThan(12, z, P) == 0);
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
		long num = xx[16];
		long num2 = xx[17];
		long num3 = xx[18];
		long num4 = xx[19];
		long num5 = xx[20];
		long num6 = xx[21];
		long num7 = xx[22];
		long num8 = xx[23];
		long num9 = xx[12] + num5 - 1;
		long num10 = xx[13] + num7;
		long num11 = xx[14] + num7 + num8;
		long num12 = xx[15] + num8;
		long num13 = num2 + num6;
		long num14 = num6 - num8;
		long num15 = num7 - num8;
		long num16 = num9 + num14;
		long num17 = 0L;
		num17 += xx[0] + num16;
		z[0] = (uint)num17;
		num17 >>= 32;
		num17 += xx[1] + num8 - num9 + num10;
		z[1] = (uint)num17;
		num17 >>= 32;
		num17 += xx[2] - num6 - num10 + num11;
		z[2] = (uint)num17;
		num17 >>= 32;
		num17 += xx[3] - num11 + num12 + num16;
		z[3] = (uint)num17;
		num17 >>= 32;
		num17 += xx[4] + num + num6 + num10 - num12 + num16;
		z[4] = (uint)num17;
		num17 >>= 32;
		num17 += xx[5] - num + num10 + num11 + num13;
		z[5] = (uint)num17;
		num17 >>= 32;
		num17 += xx[6] + num3 - num2 + num11 + num12;
		z[6] = (uint)num17;
		num17 >>= 32;
		num17 += xx[7] + num + num4 - num3 + num12;
		z[7] = (uint)num17;
		num17 >>= 32;
		num17 += xx[8] + num + num2 + num5 - num4;
		z[8] = (uint)num17;
		num17 >>= 32;
		num17 += xx[9] + num3 - num5 + num13;
		z[9] = (uint)num17;
		num17 >>= 32;
		num17 += xx[10] + num3 + num4 - num14 + num15;
		z[10] = (uint)num17;
		num17 >>= 32;
		num17 += xx[11] + num4 + num5 - num15;
		z[11] = (uint)num17;
		num17 >>= 32;
		num17++;
		Reduce32((uint)num17, z);
	}

	public static void Reduce32(uint x, uint[] z)
	{
		long num = 0L;
		if (x != 0)
		{
			long num2 = x;
			num += z[0] + num2;
			z[0] = (uint)num;
			num >>= 32;
			num += z[1] - num2;
			z[1] = (uint)num;
			num >>= 32;
			if (num != 0)
			{
				num += z[2];
				z[2] = (uint)num;
				num >>= 32;
			}
			num += z[3] + num2;
			z[3] = (uint)num;
			num >>= 32;
			num += z[4] + num2;
			z[4] = (uint)num;
			num >>= 32;
		}
		if ((num != 0 && Nat.IncAt(12, z, 5) != 0) || (z[11] == uint.MaxValue && Nat.Gte(12, z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void Square(uint[] x, uint[] z)
	{
		uint[] array = Nat.Create(24);
		Nat384.Square(x, array);
		Reduce(array, z);
	}

	public static void SquareN(uint[] x, int n, uint[] z)
	{
		uint[] array = Nat.Create(24);
		Nat384.Square(x, array);
		Reduce(array, z);
		while (--n > 0)
		{
			Nat384.Square(z, array);
			Reduce(array, z);
		}
	}

	public static void Subtract(uint[] x, uint[] y, uint[] z)
	{
		if (Nat.Sub(12, x, y, z) != 0)
		{
			SubPInvFrom(z);
		}
	}

	public static void SubtractExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if (Nat.Sub(24, xx, yy, zz) != 0 && Nat.SubFrom(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.DecAt(24, zz, PExtInv.Length);
		}
	}

	public static void Twice(uint[] x, uint[] z)
	{
		if (Nat.ShiftUpBit(12, x, 0u, z) != 0 || (z[11] == uint.MaxValue && Nat.Gte(12, z, P)))
		{
			AddPInvTo(z);
		}
	}

	private static void AddPInvTo(uint[] z)
	{
		long num = (long)z[0] + 1L;
		z[0] = (uint)num;
		num >>= 32;
		num += (long)z[1] - 1L;
		z[1] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num += z[2];
			z[2] = (uint)num;
			num >>= 32;
		}
		num += (long)z[3] + 1L;
		z[3] = (uint)num;
		num >>= 32;
		num += (long)z[4] + 1L;
		z[4] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			Nat.IncAt(12, z, 5);
		}
	}

	private static void SubPInvFrom(uint[] z)
	{
		long num = (long)z[0] - 1L;
		z[0] = (uint)num;
		num >>= 32;
		num += (long)z[1] + 1L;
		z[1] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num += z[2];
			z[2] = (uint)num;
			num >>= 32;
		}
		num += (long)z[3] - 1L;
		z[3] = (uint)num;
		num >>= 32;
		num += (long)z[4] - 1L;
		z[4] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			Nat.DecAt(12, z, 5);
		}
	}
}
