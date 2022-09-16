using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP192K1Field
{
	private const uint P5 = uint.MaxValue;

	private const uint PExt11 = uint.MaxValue;

	private const uint PInv33 = 4553u;

	internal static readonly uint[] P = new uint[6] { 4294962743u, 4294967294u, 4294967295u, 4294967295u, 4294967295u, 4294967295u };

	private static readonly uint[] PExt = new uint[12]
	{
		20729809u, 9106u, 1u, 0u, 0u, 0u, 4294958190u, 4294967293u, 4294967295u, 4294967295u,
		4294967295u, 4294967295u
	};

	private static readonly uint[] PExtInv = new uint[8] { 4274237487u, 4294958189u, 4294967294u, 4294967295u, 4294967295u, 4294967295u, 9105u, 2u };

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		if (Nat192.Add(x, y, z) != 0 || (z[5] == uint.MaxValue && Nat192.Gte(z, P)))
		{
			Nat.Add33To(6, 4553u, z);
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
			Nat.Add33To(6, 4553u, z);
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
		ulong y = Nat192.Mul33Add(4553u, xx, 6, xx, 0, z, 0);
		if (Nat192.Mul33DWordAdd(4553u, y, z, 0) != 0 || (z[5] == uint.MaxValue && Nat192.Gte(z, P)))
		{
			Nat.Add33To(6, 4553u, z);
		}
	}

	public static void Reduce32(uint x, uint[] z)
	{
		if ((x != 0 && Nat192.Mul33WordAdd(4553u, x, z, 0) != 0) || (z[5] == uint.MaxValue && Nat192.Gte(z, P)))
		{
			Nat.Add33To(6, 4553u, z);
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
			Nat.Sub33From(6, 4553u, z);
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
			Nat.Add33To(6, 4553u, z);
		}
	}
}
