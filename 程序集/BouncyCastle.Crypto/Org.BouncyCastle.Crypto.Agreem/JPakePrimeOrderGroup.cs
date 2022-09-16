using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Agreement.JPake;

public class JPakePrimeOrderGroup
{
	private readonly BigInteger p;

	private readonly BigInteger q;

	private readonly BigInteger g;

	public virtual BigInteger P => p;

	public virtual BigInteger Q => q;

	public virtual BigInteger G => g;

	public JPakePrimeOrderGroup(BigInteger p, BigInteger q, BigInteger g)
		: this(p, q, g, skipChecks: false)
	{
	}

	public JPakePrimeOrderGroup(BigInteger p, BigInteger q, BigInteger g, bool skipChecks)
	{
		JPakeUtilities.ValidateNotNull(p, "p");
		JPakeUtilities.ValidateNotNull(q, "q");
		JPakeUtilities.ValidateNotNull(g, "g");
		if (!skipChecks)
		{
			if (!p.Subtract(JPakeUtilities.One).Mod(q).Equals(JPakeUtilities.Zero))
			{
				throw new ArgumentException("p-1 must be evenly divisible by q");
			}
			if (g.CompareTo(BigInteger.Two) == -1 || g.CompareTo(p.Subtract(JPakeUtilities.One)) == 1)
			{
				throw new ArgumentException("g must be in [2, p-1]");
			}
			if (!g.ModPow(q, p).Equals(JPakeUtilities.One))
			{
				throw new ArgumentException("g^q mod p must equal 1");
			}
			if (!p.IsProbablePrime(20))
			{
				throw new ArgumentException("p must be prime");
			}
			if (!q.IsProbablePrime(20))
			{
				throw new ArgumentException("q must be prime");
			}
		}
		this.p = p;
		this.q = q;
		this.g = g;
	}
}
