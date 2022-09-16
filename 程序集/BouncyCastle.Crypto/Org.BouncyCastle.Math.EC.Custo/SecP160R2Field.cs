using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP160R2Field
{
	private const uint P4 = uint.MaxValue;

	private const uint PExt9 = uint.MaxValue;

	private const uint PInv33 = 21389u;

	internal static readonly uint[] P = new uint[5] { 4294945907u, 4294967294u, 4294967295u, 4294967295u, 4294967295u };

	private static readonly uint[] PExt = new uint[10] { 457489321u, 42778u, 1u, 0u, 0u, 4294924518u, 4294967293u, 4294967295u, 4294967295u, 4294967295u };

	private static readonly uint[] PExtInv = new uint[7] { 3837477975u, 4294924517u, 4294967294u, 4294967295u, 4294967295u, 42777u, 2u };

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		if (Nat160.Add(x, y, z) != 0 || (z[4] == uint.MaxValue && Nat160.Gte(z, P)))
		{
			Nat.Add33To(5, 21389u, z);
		}
	}

	public static void AddExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if ((Nat.Add(10, xx, yy, zz) != 0 || (zz[9] == uint.MaxValue && Nat.Gte(10, zz, PExt))) && Nat.AddTo(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.IncAt(10, zz, PExtInv.Length);
		}
	}

	public static void AddOne(uint[] x, uint[] z)
	{
		if (Nat.Inc(5, x, z) != 0 || (z[4] == uint.MaxValue && Nat160.Gte(z, P)))
		{
			Nat.Add33To(5, 21389u, z);
		}
	}

	public static uint[] FromBigInteger(BigInteger x)
	{
		uint[] array = Nat.FromBigInteger(160, x);
		if (array[4] == uint.MaxValue && Nat160.Gte(array, P))
		{
			Nat160.SubFrom(P, array);
		}
		return array;
	}

	public static void Half(uint[] x, uint[] z)
	{
		if ((x[0] & 1) == 0)
		{
			Nat.ShiftDownBit(5, x, 0u, z);
			return;
		}
		uint c = Nat160.Add(x, P, z);
		Nat.ShiftDownBit(5, z, c);
	}

	public static void Inv(uint[] x, uint[] z)
	{
		Mod.CheckedModOddInverse(P, x, z);
	}

	public static int IsZero(uint[] x)
	{
		uint num = 0u;
		for (int i = 0; i < 5; i++)
		{
			num |= x[i];
		}
		num = (num >> 1) | (num & 1u);
		return (int)(num - 1) >> 31;
	}

	public static void Multiply(uint[] x, uint[] y, uint[] z)
	{
		uint[] array = Nat160.CreateExt();
		Nat160.Mul(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(uint[] x, uint[] y, uint[] zz)
	{
		if ((Nat160.MulAddTo(x, y, zz) != 0 || (zz[9] == uint.MaxValue && Nat.Gte(10, zz, PExt))) && Nat.AddTo(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.IncAt(10, zz, PExtInv.Length);
		}
	}

	public static void Negate(uint[] x, uint[] z)
	{
		if (IsZero(x) != 0)
		{
			Nat160.Sub(P, P, z);
		}
		else
		{
			Nat160.Sub(P, x, z);
		}
	}

	public static void Random(SecureRandom r, uint[] z)
	{
		byte[] array = new byte[20];
		do
		{
			r.NextBytes(array);
			Pack.LE_To_UInt32(array, 0, z, 0, 5);
		}
		while (Nat.LessThan(5, z, P) == 0);
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
		ulong y = Nat160.Mul33Add(21389u, xx, 5, xx, 0, z, 0);
		if (Nat160.Mul33DWordAdd(21389u, y, z, 0) != 0 || (z[4] == uint.MaxValue && Nat160.Gte(z, P)))
		{
			Nat.Add33To(5, 21389u, z);
		}
	}

	public static void Reduce32(uint x, uint[] z)
	{
		if ((x != 0 && Nat160.Mul33WordAdd(21389u, x, z, 0) != 0) || (z[4] == uint.MaxValue && Nat160.Gte(z, P)))
		{
			Nat.Add33To(5, 21389u, z);
		}
	}

	public static void Square(uint[] x, uint[] z)
	{
		uint[] array = Nat160.CreateExt();
		Nat160.Square(x, array);
		Reduce(array, z);
	}

	public static void SquareN(uint[] x, int n, uint[] z)
	{
		uint[] array = Nat160.CreateExt();
		Nat160.Square(x, array);
		Reduce(array, z);
		while (--n > 0)
		{
			Nat160.Square(z, array);
			Reduce(array, z);
		}
	}

	public static void Subtract(uint[] x, uint[] y, uint[] z)
	{
		if (Nat160.Sub(x, y, z) != 0)
		{
			Nat.Sub33From(5, 21389u, z);
		}
	}

	public static void SubtractExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if (Nat.Sub(10, xx, yy, zz) != 0 && Nat.SubFrom(PExtInv.Length, PExtInv, zz) != 0)
		{
			Nat.DecAt(10, zz, PExtInv.Length);
		}
	}

	public static void Twice(uint[] x, uint[] z)
	{
		if (Nat.ShiftUpBit(5, x, 0u, z) != 0 || (z[4] == uint.MaxValue && Nat160.Gte(z, P)))
		{
			Nat.Add33To(5, 21389u, z);
		}
	}
}
