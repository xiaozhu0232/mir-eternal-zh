using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DHPrivateKeyParameters : DHKeyParameters
{
	private readonly BigInteger x;

	public BigInteger X => x;

	public DHPrivateKeyParameters(BigInteger x, DHParameters parameters)
		: base(isPrivate: true, parameters)
	{
		this.x = x;
	}

	public DHPrivateKeyParameters(BigInteger x, DHParameters parameters, DerObjectIdentifier algorithmOid)
		: base(isPrivate: true, parameters, algorithmOid)
	{
		this.x = x;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is DHPrivateKeyParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(DHPrivateKeyParameters other)
	{
		if (x.Equals(other.x))
		{
			return Equals((DHKeyParameters)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ base.GetHashCode();
	}
}
