using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DHPublicKeyParameters : DHKeyParameters
{
	private readonly BigInteger y;

	public virtual BigInteger Y => y;

	private static BigInteger Validate(BigInteger y, DHParameters dhParams)
	{
		if (y == null)
		{
			throw new ArgumentNullException("y");
		}
		BigInteger p = dhParams.P;
		if (y.CompareTo(BigInteger.Two) < 0 || y.CompareTo(p.Subtract(BigInteger.Two)) > 0)
		{
			throw new ArgumentException("invalid DH public key", "y");
		}
		BigInteger q = dhParams.Q;
		if (q == null)
		{
			return y;
		}
		if (p.TestBit(0) && p.BitLength - 1 == q.BitLength && p.ShiftRight(1).Equals(q))
		{
			if (1 == Legendre(y, p))
			{
				return y;
			}
		}
		else if (BigInteger.One.Equals(y.ModPow(q, p)))
		{
			return y;
		}
		throw new ArgumentException("value does not appear to be in correct group", "y");
	}

	public DHPublicKeyParameters(BigInteger y, DHParameters parameters)
		: base(isPrivate: false, parameters)
	{
		this.y = Validate(y, parameters);
	}

	public DHPublicKeyParameters(BigInteger y, DHParameters parameters, DerObjectIdentifier algorithmOid)
		: base(isPrivate: false, parameters, algorithmOid)
	{
		this.y = Validate(y, parameters);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is DHPublicKeyParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(DHPublicKeyParameters other)
	{
		if (y.Equals(other.y))
		{
			return Equals((DHKeyParameters)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return y.GetHashCode() ^ base.GetHashCode();
	}

	private static int Legendre(BigInteger a, BigInteger b)
	{
		int bitLength = b.BitLength;
		uint[] array = Nat.FromBigInteger(bitLength, a);
		uint[] array2 = Nat.FromBigInteger(bitLength, b);
		int num = 0;
		int num2 = array2.Length;
		while (true)
		{
			if (array[0] == 0)
			{
				Nat.ShiftDownWord(num2, array, 0u);
				continue;
			}
			int num3 = Integers.NumberOfTrailingZeros((int)array[0]);
			if (num3 > 0)
			{
				Nat.ShiftDownBits(num2, array, num3, 0u);
				int num4 = (int)array2[0];
				num ^= (num4 ^ (num4 >> 1)) & (num3 << 1);
			}
			int num5 = Nat.Compare(num2, array, array2);
			if (num5 == 0)
			{
				break;
			}
			if (num5 < 0)
			{
				num ^= (int)(array[0] & array2[0]);
				uint[] array3 = array;
				array = array2;
				array2 = array3;
			}
			while (array[num2 - 1] == 0)
			{
				num2--;
			}
			Nat.Sub(num2, array, array2, array);
		}
		if (!Nat.IsOne(num2, array2))
		{
			return 0;
		}
		return 1 - (num & 2);
	}
}
