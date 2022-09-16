using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class Gost3410Parameters : ICipherParameters
{
	private readonly BigInteger p;

	private readonly BigInteger q;

	private readonly BigInteger a;

	private readonly Gost3410ValidationParameters validation;

	public BigInteger P => p;

	public BigInteger Q => q;

	public BigInteger A => a;

	public Gost3410ValidationParameters ValidationParameters => validation;

	public Gost3410Parameters(BigInteger p, BigInteger q, BigInteger a)
		: this(p, q, a, null)
	{
	}

	public Gost3410Parameters(BigInteger p, BigInteger q, BigInteger a, Gost3410ValidationParameters validation)
	{
		if (p == null)
		{
			throw new ArgumentNullException("p");
		}
		if (q == null)
		{
			throw new ArgumentNullException("q");
		}
		if (a == null)
		{
			throw new ArgumentNullException("a");
		}
		this.p = p;
		this.q = q;
		this.a = a;
		this.validation = validation;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is Gost3410Parameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(Gost3410Parameters other)
	{
		if (p.Equals(other.p) && q.Equals(other.q))
		{
			return a.Equals(other.a);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return p.GetHashCode() ^ q.GetHashCode() ^ a.GetHashCode();
	}
}
