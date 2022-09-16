using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP192R1Field
{
	private const uint P5 = uint.MaxValue;

	private const uint PExt11 = uint.MaxValue;

	internal static readonly uint[] P = new uint[6] { 4294967295u, 4294967295u, 4294967294u, 4294967295u, 4294967295u, 4294967295u };

	private static readonly uint[] PExt = new uint[12]
	{
		1u, 0u, 2u, 0u, 1u, 0u, 4294967294u, 4294967295u, 4294967293u, 4294967295u,
		4294967295u, 4294967295u
	};

	private static readonly uint[] PExtInv = new uint[9] { 4294967295u, 4294967295u, 4294967293u, 4294967295u, 4294967294u, 4294967295u, 1u, 0u, 2u };

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		if (Nat192.Add(x, y, z) != 0 || (z[5] == uint.MaxValue && Nat192.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void AddExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if ((Nat.Add(12, xx, yy, zz) != 0 || (zz[11] == uint.MaxValue && Nat.Gte(12, zz, PExt))) && Nat.AddTo(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.IncAt(12, zz, PExtInv.Length);
		}
	}

	public static void AddOne(uint[] x, uint[] z)
	{
		if (Nat.Inc(6, x, z) != 0 || (z[5] == uint.MaxValue && Nat192.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static uint[] FromBigInteger(BigInteger x)
	{
		uint[] array = Nat.FromBigInteger(192, x);
		if (array[5] == uint.MaxValue && Nat192.Gte(array, P))
		{
			Nat192.SubFrom(P, array);
		}
		return array;
	}

	public static void Half(uint[] x, uint[] z)
	{
		if ((x[0] & 1) == 0)
		{
			Nat.ShiftDownBit(6, x, 0u, z);
			return;
		}
		uint c = Nat192.Add(x, P, z);
		Nat.ShiftDownBit(6, z, c);
	}

	public static void Inv(uint[] x, uint[] z)
	{
		Mod.CheckedModOddInverse(P, x, z);
	}

	public static int IsZero(uint[] x)
	{
		uint num = 0u;
		for (int i = 0; i < 6; i++)
		{
			num |= x[i];
		}
		num = (num >> 1) | (num & 1u);
		return (int)(num - 1) >> 31;
	}

	public static void Multiply(uint[] x, uint[] y, uint[] z)
	{
		uint[] array = Nat192.CreateExt();
		Nat192.Mul(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(uint[] x, uint[] y, uint[] zz)
	{
		if ((Nat192.MulAddTo(x, y, zz) != 0 || (zz[11] == uint.MaxValue && Nat.Gte(12, zz, PExt))) && Nat.AddTo(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.IncAt(12, zz, PExtInv.Length);
		}
	}

	public static void Negate(uint[] x, uint[] z)
	{
		if (IsZero(x) != 0)
		{
			Nat192.Sub(P, P, z);
		}
		else
		{
			Nat192.Sub(P, x, z);
		}
	}

	public static void Random(SecureRandom r, uint[] z)
	{
		byte[] array = new byte[24];
		do
		{
			r.NextBytes(array);
			Pack.LE_To_UInt32(array, 0, z, 0, 6);
		}
		while (Nat.LessThan(6, z, P) == 0);
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
		ulong num = xx[6];
		ulong num2 = xx[7];
		ulong num3 = xx[8];
		ulong num4 = xx[9];
		ulong num5 = xx[10];
		ulong num6 = xx[11];
		ulong num7 = num + num5;
		ulong num8 = num2 + num6;
		ulong num9 = 0uL;
		num9 += xx[0] + num7;
		uint num10 = (uint)num9;
		num9 >>= 32;
		num9 += xx[1] + num8;
		z[1] = (uint)num9;
		num9 >>= 32;
		num7 += num3;
		num8 += num4;
		num9 += xx[2] + num7;
		ulong num11 = (uint)num9;
		num9 >>= 32;
		num9 += xx[3] + num8;
		z[3] = (uint)num9;
		num9 >>= 32;
		num7 -= num;
		num8 -= num2;
		num9 += xx[4] + num7;
		z[4] = (uint)num9;
		num9 >>= 32;
		num9 += xx[5] + num8;
		z[5] = (uint)num9;
		num9 >>= 32;
		num11 += num9;
		num9 += num10;
		z[0] = (uint)num9;
		num9 >>= 32;
		if (num9 != 0)
		{
			num9 += z[1];
			z[1] = (uint)num9;
			num11 += num9 >> 32;
		}
		z[2] = (uint)num11;
		num9 = num11 >> 32;
		if ((num9 != 0 && Nat.IncAt(6, z, 3) != 0) || (z[5] == uint.MaxValue && Nat192.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void Reduce32(uint x, uint[] z)
	{
		ulong num = 0uL;
		if (x != 0)
		{
			num += (ulong)((long)z[0] + (long)x);
			z[0] = (uint)num;
			num >>= 32;
			if (num != 0)
			{
				num += z[1];
				z[1] = (uint)num;
				num >>= 32;
			}
			num += (ulong)((long)z[2] + (long)x);
			z[2] = (uint)num;
			num >>= 32;
		}
		if ((num != 0 && Nat.IncAt(6, z, 3) != 0) || (z[5] == uint.MaxValue && Nat192.Gte(z, P)))
		{
			AddPInvTo(z);
		}
	}

	public static void Square(uint[] x, uint[] z)
	{
		uint[] array = Nat192.CreateExt();
		Nat192.Square(x, array);
		Reduce(array, z);
	}

	public static void SquareN(uint[] x, int n, uint[] z)
	{
		uint[] array = Nat192.CreateExt();
		Nat192.Square(x, array);
		Reduce(array, z);
		while (--n > 0)
		{
			Nat192.Square(z, array);
			Reduce(array, z);
		}
	}

	public static void Subtract(uint[] x, uint[] y, uint[] z)
	{
		if (Nat192.Sub(x, y, z) != 0)
		{
			SubPInvFrom(z);
		}
	}

	public static void SubtractExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if (Nat.Sub(12, xx, yy, zz) != 0 && Nat.SubFrom(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.DecAt(12, zz, PExtInv.Length);
		}
	}

	public static void Twice(uint[] x, uint[] z)
	{
		if (Nat.ShiftUpBit(6, x, 0u, z) != 0 || (z[5] == uint.MaxValue && Nat192.Gte(z, P)))
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
		}
		num += (long)z[2] + 1L;
		z[2] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			Nat.IncAt(6, z, 3);
		}
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
		}
		num += (long)z[2] - 1L;
		z[2] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			Nat.DecAt(6, z, 3);
		}
	}
}
