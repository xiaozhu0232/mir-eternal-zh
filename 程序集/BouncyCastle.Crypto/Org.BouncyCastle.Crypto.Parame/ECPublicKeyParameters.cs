using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ECPublicKeyParameters : ECKeyParameters
{
	private readonly ECPoint q;

	public ECPoint Q => q;

	public ECPublicKeyParameters(ECPoint q, ECDomainParameters parameters)
		: this("EC", q, parameters)
	{
	}

	[Obsolete("Use version with explicit 'algorithm' parameter")]
	public ECPublicKeyParameters(ECPoint q, DerObjectIdentifier publicKeyParamSet)
		: base("ECGOST3410", isPrivate: false, publicKeyParamSet)
	{
		this.q = ECDomainParameters.ValidatePublicPoint(base.Parameters.Curve, q);
	}

	public ECPublicKeyParameters(string algorithm, ECPoint q, ECDomainParameters parameters)
		: base(algorithm, isPrivate: false, parameters)
	{
		this.q = ECDomainParameters.ValidatePublicPoint(base.Parameters.Curve, q);
	}

	public ECPublicKeyParameters(string algorithm, ECPoint q, DerObjectIdentifier publicKeyParamSet)
		: base(algorithm, isPrivate: false, publicKeyParamSet)
	{
		this.q = ECDomainParameters.ValidatePublicPoint(base.Parameters.Curve, q);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is ECPublicKeyParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(ECPublicKeyParameters other)
	{
		if (q.Equals(other.q))
		{
			return Equals((ECKeyParameters)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return q.GetHashCode() ^ base.GetHashCode();
	}
}
