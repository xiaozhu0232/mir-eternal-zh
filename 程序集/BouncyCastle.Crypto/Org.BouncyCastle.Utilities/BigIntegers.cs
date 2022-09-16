using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Utilities;

public abstract class BigIntegers
{
	private const int MaxIterations = 1000;

	public static readonly BigInteger Zero = BigInteger.Zero;

	public static readonly BigInteger One = BigInteger.One;

	public static byte[] AsUnsignedByteArray(BigInteger n)
	{
		return n.ToByteArrayUnsigned();
	}

	public static byte[] AsUnsignedByteArray(int length, BigInteger n)
	{
		byte[] array = n.ToByteArrayUnsigned();
		if (array.Length > length)
		{
			throw new ArgumentException("standard length exceeded", "n");
		}
		if (array.Length == length)
		{
			return array;
		}
		byte[] array2 = new byte[length];
		Array.Copy(array, 0, array2, array2.Length - array.Length, array.Length);
		return array2;
	}

	public static void AsUnsignedByteArray(BigInteger value, byte[] buf, int off, int len)
	{
		byte[] array = value.ToByteArrayUnsigned();
		if (array.Length == len)
		{
			Array.Copy(array, 0, buf, off, len);
			return;
		}
		int num = ((array[0] == 0) ? 1 : 0);
		int num2 = array.Length - num;
		if (num2 > len)
		{
			throw new ArgumentException("standard length exceeded for value");
		}
		int num3 = len - num2;
		Arrays.Fill(buf, off, off + num3, 0);
		Array.Copy(array, num, buf, off + num3, num2);
	}

	public static BigInteger CreateRandomBigInteger(int bitLength, SecureRandom secureRandom)
	{
		return new BigInteger(bitLength, secureRandom);
	}

	public static BigInteger CreateRandomInRange(BigInteger min, BigInteger max, SecureRandom random)
	{
		int num = min.CompareTo(max);
		if (num >= 0)
		{
			if (num > 0)
			{
				throw new ArgumentException("'min' may not be greater than 'max'");
			}
			return min;
		}
		if (min.BitLength > max.BitLength / 2)
		{
			return CreateRandomInRange(BigInteger.Zero, max.Subtract(min), random).Add(min);
		}
		for (int i = 0; i < 1000; i++)
		{
			BigInteger bigInteger = new BigInteger(max.BitLength, random);
			if (bigInteger.CompareTo(min) >= 0 && bigInteger.CompareTo(max) <= 0)
			{
				return bigInteger;
			}
		}
		return new BigInteger(max.Subtract(min).BitLength - 1, random).Add(min);
	}

	public static BigInteger ModOddInverse(BigInteger M, BigInteger X)
	{
		if (!M.TestBit(0))
		{
			throw new ArgumentException("must be odd", "M");
		}
		if (M.SignValue != 1)
		{
			throw new ArithmeticException("BigInteger: modulus not positive");
		}
		if (X.SignValue < 0 || X.CompareTo(M) >= 0)
		{
			X = X.Mod(M);
		}
		int bitLength = M.BitLength;
		uint[] array = Nat.FromBigInteger(bitLength, M);
		uint[] x = Nat.FromBigInteger(bitLength, X);
		int len = array.Length;
		uint[] array2 = Nat.Create(len);
		if (Mod.ModOddInverse(array, x, array2) == 0)
		{
			throw new ArithmeticException("BigInteger not invertible");
		}
		return Nat.ToBigInteger(len, array2);
	}

	public static BigInteger ModOddInverseVar(BigInteger M, BigInteger X)
	{
		if (!M.TestBit(0))
		{
			throw new ArgumentException("must be odd", "M");
		}
		if (M.SignValue != 1)
		{
			throw new ArithmeticException("BigInteger: modulus not positive");
		}
		if (M.Equals(One))
		{
			return Zero;
		}
		if (X.SignValue < 0 || X.CompareTo(M) >= 0)
		{
			X = X.Mod(M);
		}
		if (X.Equals(One))
		{
			return One;
		}
		int bitLength = M.BitLength;
		uint[] array = Nat.FromBigInteger(bitLength, M);
		uint[] x = Nat.FromBigInteger(bitLength, X);
		int len = array.Length;
		uint[] array2 = Nat.Create(len);
		if (!Mod.ModOddInverseVar(array, x, array2))
		{
			throw new ArithmeticException("BigInteger not invertible");
		}
		return Nat.ToBigInteger(len, array2);
	}

	public static int GetUnsignedByteLength(BigInteger n)
	{
		return (n.BitLength + 7) / 8;
	}
}
