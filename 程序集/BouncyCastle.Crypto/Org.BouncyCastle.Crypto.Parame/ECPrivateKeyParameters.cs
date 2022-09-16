using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ECPrivateKeyParameters : ECKeyParameters
{
	private readonly BigInteger d;

	public BigInteger D => d;

	public ECPrivateKeyParameters(BigInteger d, ECDomainParameters parameters)
		: this("EC", d, parameters)
	{
	}

	[Obsolete("Use version with explicit 'algorithm' parameter")]
	public ECPrivateKeyParameters(BigInteger d, DerObjectIdentifier publicKeyParamSet)
		: base("ECGOST3410", isPrivate: true, publicKeyParamSet)
	{
		this.d = base.Parameters.ValidatePrivateScalar(d);
	}

	public ECPrivateKeyParameters(string algorithm, BigInteger d, ECDomainParameters parameters)
		: base(algorithm, isPrivate: true, parameters)
	{
		this.d = base.Parameters.ValidatePrivateScalar(d);
	}

	public ECPrivateKeyParameters(string algorithm, BigInteger d, DerObjectIdentifier publicKeyParamSet)
		: base(algorithm, isPrivate: true, publicKeyParamSet)
	{
		this.d = base.Parameters.ValidatePrivateScalar(d);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is ECPrivateKeyParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(ECPrivateKeyParameters other)
	{
		if (d.Equals(other.d))
		{
			return Equals((ECKeyParameters)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return d.GetHashCode() ^ base.GetHashCode();
	}
}
