using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ElGamalParameters : ICipherParameters
{
	private readonly BigInteger p;

	private readonly BigInteger g;

	private readonly int l;

	public BigInteger P => p;

	public BigInteger G => g;

	public int L => l;

	public ElGamalParameters(BigInteger p, BigInteger g)
		: this(p, g, 0)
	{
	}

	public ElGamalParameters(BigInteger p, BigInteger g, int l)
	{
		if (p == null)
		{
			throw new ArgumentNullException("p");
		}
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		this.p = p;
		this.g = g;
		this.l = l;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is ElGamalParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(ElGamalParameters other)
	{
		if (p.Equals(other.p) && g.Equals(other.g))
		{
			return l == other.l;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return p.GetHashCode() ^ g.GetHashCode() ^ l;
	}
}
