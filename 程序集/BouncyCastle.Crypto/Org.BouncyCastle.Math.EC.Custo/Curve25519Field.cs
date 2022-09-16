using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Math.EC.Custom.Djb;

internal class Curve25519Field
{
	private const uint P7 = 2147483647u;

	private const uint PInv = 19u;

	internal static readonly uint[] P = new uint[8] { 4294967277u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 2147483647u };

	private static readonly uint[] PExt = new uint[16]
	{
		361u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 4294967277u, 4294967295u,
		4294967295u, 4294967295u, 4294967295u, 4294967295u, 4294967295u, 1073741823u
	};

	public static void Add(uint[] x, uint[] y, uint[] z)
	{
		Nat256.Add(x, y, z);
		if (Nat256.Gte(z, P))
		{
			SubPFrom(z);
		}
	}

	public static void AddExt(uint[] xx, uint[] yy, uint[] zz)
	{
		Nat.Add(16, xx, yy, zz);
		if (Nat.Gte(16, zz, PExt))
		{
			SubPExtFrom(zz);
		}
	}

	public static void AddOne(uint[] x, uint[] z)
	{
		Nat.Inc(8, x, z);
		if (Nat256.Gte(z, P))
		{
			SubPFrom(z);
		}
	}

	public static uint[] FromBigInteger(BigInteger x)
	{
		uint[] array = Nat.FromBigInteger(256, x);
		while (Nat256.Gte(array, P))
		{
			Nat256.SubFrom(P, array);
		}
		return array;
	}

	public static void Half(uint[] x, uint[] z)
	{
		if ((x[0] & 1) == 0)
		{
			Nat.ShiftDownBit(8, x, 0u, z);
			return;
		}
		Nat256.Add(x, P, z);
		Nat.ShiftDownBit(8, z, 0u);
	}

	public static void Inv(uint[] x, uint[] z)
	{
		Mod.CheckedModOddInverse(P, x, z);
	}

	public static int IsZero(uint[] x)
	{
		uint num = 0u;
		for (int i = 0; i < 8; i++)
		{
			num |= x[i];
		}
		num = (num >> 1) | (num & 1u);
		return (int)(num - 1) >> 31;
	}

	public static void Multiply(uint[] x, uint[] y, uint[] z)
	{
		uint[] array = Nat256.CreateExt();
		Nat256.Mul(x, y, array);
		Reduce(array, z);
	}

	public static void MultiplyAddToExt(uint[] x, uint[] y, uint[] zz)
	{
		Nat256.MulAddTo(x, y, zz);
		if (Nat.Gte(16, zz, PExt))
		{
			SubPExtFrom(zz);
		}
	}

	public static void Negate(uint[] x, uint[] z)
	{
		if (IsZero(x) != 0)
		{
			Nat256.Sub(P, P, z);
		}
		else
		{
			Nat256.Sub(P, x, z);
		}
	}

	public static void Random(SecureRandom r, uint[] z)
	{
		byte[] array = new byte[32];
		do
		{
			r.NextBytes(array);
			Pack.LE_To_UInt32(array, 0, z, 0, 8);
			uint[] array2;
			(array2 = z)[7] = array2[7] & 0x7FFFFFFFu;
		}
		while (Nat.LessThan(8, z, P) == 0);
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
		uint num = xx[7];
		Nat.ShiftUpBit(8, xx, 8, num, z, 0);
		uint num2 = Nat256.MulByWordAddTo(19u, xx, z) << 1;
		uint num3 = z[7];
		num2 += (num3 >> 31) - (num >> 31);
		num3 &= 0x7FFFFFFFu;
		if ((z[7] = num3 + Nat.AddWordTo(7, num2 * 19, z)) >= int.MaxValue && Nat256.Gte(z, P))
		{
			SubPFrom(z);
		}
	}

	public static void Reduce27(uint x, uint[] z)
	{
		uint num = z[7];
		uint num2 = (x << 1) | (num >> 31);
		num &= 0x7FFFFFFFu;
		if ((z[7] = num + Nat.AddWordTo(7, num2 * 19, z)) >= int.MaxValue && Nat256.Gte(z, P))
		{
			SubPFrom(z);
		}
	}

	public static void Square(uint[] x, uint[] z)
	{
		uint[] array = Nat256.CreateExt();
		Nat256.Square(x, array);
		Reduce(array, z);
	}

	public static void SquareN(uint[] x, int n, uint[] z)
	{
		uint[] array = Nat256.CreateExt();
		Nat256.Square(x, array);
		Reduce(array, z);
		while (--n > 0)
		{
			Nat256.Square(z, array);
			Reduce(array, z);
		}
	}

	public static void Subtract(uint[] x, uint[] y, uint[] z)
	{
		if (Nat256.Sub(x, y, z) != 0)
		{
			AddPTo(z);
		}
	}

	public static void SubtractExt(uint[] xx, uint[] yy, uint[] zz)
	{
		if (Nat.Sub(16, xx, yy, zz) != 0)
		{
			AddPExtTo(zz);
		}
	}

	public static void Twice(uint[] x, uint[] z)
	{
		Nat.ShiftUpBit(8, x, 0u, z);
		if (Nat256.Gte(z, P))
		{
			SubPFrom(z);
		}
	}

	private static uint AddPTo(uint[] z)
	{
		long num = (long)z[0] - 19L;
		z[0] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num = Nat.DecAt(7, z, 1);
		}
		num += (long)z[7] + 2147483648L;
		z[7] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	private static uint AddPExtTo(uint[] zz)
	{
		long num = (long)zz[0] + (long)PExt[0];
		zz[0] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num = Nat.IncAt(8, zz, 1);
		}
		num += (long)zz[8] - 19L;
		zz[8] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num = Nat.DecAt(15, zz, 9);
		}
		num += (long)zz[15] + (long)(PExt[15] + 1);
		zz[15] = (uint)num;
		num >>= 32;
		return (uint)num;
	}

	private static int SubPFrom(uint[] z)
	{
		long num = (long)z[0] + 19L;
		z[0] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num = Nat.IncAt(7, z, 1);
		}
		num += (long)z[7] - 2147483648L;
		z[7] = (uint)num;
		num >>= 32;
		return (int)num;
	}

	private static int SubPExtFrom(uint[] zz)
	{
		long num = (long)zz[0] - (long)PExt[0];
		zz[0] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num = Nat.DecAt(8, zz, 1);
		}
		num += (long)zz[8] + 19L;
		zz[8] = (uint)num;
		num >>= 32;
		if (num != 0)
		{
			num = Nat.IncAt(15, zz, 9);
		}
		num += (long)zz[15] - (long)(PExt[15] + 1);
		zz[15] = (uint)num;
		num >>= 32;
		return (int)num;
	}
}
