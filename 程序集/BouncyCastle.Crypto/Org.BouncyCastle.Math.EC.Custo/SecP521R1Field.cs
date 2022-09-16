using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP521R1Field
{
	private const uint P16 = 511u;

	internal static readonly uint[] P = new uint[17]
	{
		4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u,
		4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 511u
	};

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		uint num = Nat.Add(16, x, y, z) + x[16] + y[16];
		if (num > 511 || (num == 511 && Nat.Eq(16, z, P)))
		{
			num += Nat.Inc(16, z);
			num &= 0x1FFu;
		}
		z[16] = num;
	}

	public static void AddOne(uint[] x, uint[] z)
	{
		uint num = Nat.Inc(16, x, z) + x[16];
		if (num > 511 || (num == 511 && Nat.Eq(16, z, P)))
		{
			num += Nat.Inc(16, z);
			num &= 0x1FFu;
		}
		z[16] = num;
	}

	public static uint[] FromBigInteger(BigInteger x)
	{
		uint[] array = Nat.FromBigInteger(521, x);
		if (Nat.Eq(17, array, P))
		{
			Nat.Zero(17, array);
		}
		return array;
	}

	public static void Half(uint[] x, uint[] z)
	{
		uint num = x[16];
		uint num2 = Nat.ShiftDownBit(16, x, num, z);
		z[16] = (num >> 1) | (num2 >> 23);
	}

	public static void Inv(uint[] x, uint[] z)
	{
		Mod.CheckedModOddInverse(P, x, z);
	}

	public static int IsZero(uint[] x)
	{
		uint num = 0u;
		for (int i = 0; i < 17; i++)
		{
			num |= x[i];
		}
		num = (num >> 1) | (num & 1u);
		return (int)(num - 1) >> 31;
	}

	public static void Multiply(uint[] x, uint[] y, uint[] z)
	{
		uint[] array = Nat.Create(33);
		ImplMultiply(x, y, array);
		Reduce(array, z);
	}

	public static void Negate(uint[] x, uint[] z)
	{
		if (IsZero(x) != 0)
		{
			Nat.Sub(17, P, P, z);
		}
		else
		{
			Nat.Sub(17, P, x, z);
		}
	}

	public static void Random(SecureRandom r, uint[] z)
	{
		byte[] array = new byte[68];
		do
		{
			r.NextBytes(array);
			Pack.LE_To_UInt32(array, 0, z, 0, 17);
			uint[] array2;
			(array2 = z)[16] = array2[16] & 0x1FFu;
		}
		while (Nat.LessThan(17, z, P) == 0);
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
		uint num = xx[32];
		uint num2 = Nat.ShiftDownBits(16, xx, 16, 9, num, z, 0) >> 23;
		num2 += num >> 9;
		num2 += Nat.AddTo(16, xx, z);
		if (num2 > 511 || (num2 == 511 && Nat.Eq(16, z, P)))
		{
			num2 += Nat.Inc(16, z);
			num2 &= 0x1FFu;
		}
		z[16] = num2;
	}

	public static void Reduce23(uint[] z)
	{
		uint num = z[16];
		uint num2 = Nat.AddWordTo(16, num >> 9, z) + (num & 0x1FF);
		if (num2 > 511 || (num2 == 511 && Nat.Eq(16, z, P)))
		{
			num2 += Nat.Inc(16, z);
			num2 &= 0x1FFu;
		}
		z[16] = num2;
	}

	public static void Square(uint[] x, uint[] z)
	{
		uint[] array = Nat.Create(33);
		ImplSquare(x, array);
		Reduce(array, z);
	}

	public static void SquareN(uint[] x, int n, uint[] z)
	{
		uint[] array = Nat.Create(33);
		ImplSquare(x, array);
		Reduce(array, z);
		while (--n > 0)
		{
			ImplSquare(z, array);
			Reduce(array, z);
		}
	}

	public static void Subtract(uint[] x, uint[] y, uint[] z)
	{
		int num = Nat.Sub(16, x, y, z) + (int)(x[16] - y[16]);
		if (num < 0)
		{
			num += Nat.Dec(16, z);
			num &= 0x1FF;
		}
		z[16] = (uint)num;
	}

	public static void Twice(uint[] x, uint[] z)
	{
		uint num = x[16];
		uint num2 = Nat.ShiftUpBit(16, x, num << 23, z) | (num << 1);
		z[16] = num2 & 0x1FFu;
	}

	protected static void ImplMultiply(uint[] x, uint[] y, uint[] zz)
	{
		Nat512.Mul(x, y, zz);
		uint num = x[16];
		uint num2 = y[16];
		zz[32] = Nat.Mul31BothAdd(16, num, y, num2, x, zz, 16) + num * num2;
	}

	protected static void ImplSquare(uint[] x, uint[] zz)
	{
		Nat512.Square(x, zz);
		uint num = x[16];
		zz[32] = Nat.MulWordAddTo(16, num << 1, x, 0, zz, 16) + num * num;
	}
}
