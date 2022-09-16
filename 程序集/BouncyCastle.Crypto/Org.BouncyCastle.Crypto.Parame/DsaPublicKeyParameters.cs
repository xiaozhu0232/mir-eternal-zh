using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DsaPublicKeyParameters : DsaKeyParameters
{
	private readonly BigInteger y;

	public BigInteger Y => y;

	private static BigInteger Validate(BigInteger y, DsaParameters parameters)
	{
		if (parameters != null && (y.CompareTo(BigInteger.Two) < 0 || y.CompareTo(parameters.P.Subtract(BigInteger.Two)) > 0 || !y.ModPow(parameters.Q, parameters.P).Equals(BigInteger.One)))
		{
			throw new ArgumentException("y value does not appear to be in correct group");
		}
		return y;
	}

	public DsaPublicKeyParameters(BigInteger y, DsaParameters parameters)
		: base(isPrivate: false, parameters)
	{
		if (y == null)
		{
			throw new ArgumentNullException("y");
		}
		this.y = Validate(y, parameters);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is DsaPublicKeyParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(DsaPublicKeyParameters other)
	{
		if (y.Equals(other.y))
		{
			return Equals((DsaKeyParameters)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return y.GetHashCode() ^ base.GetHashCode();
	}
}
