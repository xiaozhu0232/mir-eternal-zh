using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP224K1Field
{
	private const uint P6 = uint.MaxValue;

	private const uint PExt13 = uint.MaxValue;

	private const uint PInv33 = 6803u;

	internal static readonly uint[] P = new uint[7] { 4294960493u, 4294967294u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u };

	private static readonly uint[] PExt = new uint[14]
	{
		46280809u, 13606u, 1u, 0u, 0u, 0u, 0u, 4294953690u, 4294967293u, 4294967295u,
		4294967295u, 4294967295u, 4294967295u, 4294967295u
	};

	private static readonly uint[] PExtInv = new uint[9] { 4248686487u, 4294953689u, 4294967294u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 13605u, 2u };

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		if (Nat224.Add(x, y, z) != 0 || (z[6] == uint.MaxValue && Nat224.Gte(z, P)))
		{
			Nat.Add33To(7, 6803u, z);
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
			Nat.Add33To(7, 6803u, z);
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
		ulong y = Nat224.Mul33Add(6803u, xx, 7, xx, 0, z, 0);
		if (Nat224.Mul33DWordAdd(6803u, y, z, 0) != 0 || (z[6] == uint.MaxValue && Nat224.Gte(z, P)))
		{
			Nat.Add33To(7, 6803u, z);
		}
	}

	public static void Reduce32(uint x, uint[] z)
	{
		if ((x != 0 && Nat224.Mul33WordAdd(6803u, x, z, 0) != 0) || (z[6] == uint.MaxValue && Nat224.Gte(z, P)))
		{
			Nat.Add33To(7, 6803u, z);
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
			Nat.Sub33From(7, 6803u, z);
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
			Nat.Add33To(7, 6803u, z);
		}
	}
}
