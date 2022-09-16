using System;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ECDomainParameters
{
	private readonly ECCurve curve;

	private readonly byte[] seed;

	private readonly ECPoint g;

	private readonly BigInteger n;

	private readonly BigInteger h;

	private BigInteger hInv;

	public ECCurve Curve => curve;

	public ECPoint G => g;

	public BigInteger N => n;

	public BigInteger H => h;

	public BigInteger HInv
	{
		get
		{
			lock (this)
			{
				if (hInv == null)
				{
					hInv = BigIntegers.ModOddInverseVar(n, h);
				}
				return hInv;
			}
		}
	}

	public ECDomainParameters(X9ECParameters x9)
		: this(x9.Curve, x9.G, x9.N, x9.H, x9.GetSeed())
	{
	}

	public ECDomainParameters(ECCurve curve, ECPoint g, BigInteger n)
		: this(curve, g, n, BigInteger.One, null)
	{
	}

	public ECDomainParameters(ECCurve curve, ECPoint g, BigInteger n, BigInteger h)
		: this(curve, g, n, h, null)
	{
	}

	public ECDomainParameters(ECCurve curve, ECPoint g, BigInteger n, BigInteger h, byte[] seed)
	{
		if (curve == null)
		{
			throw new ArgumentNullException("curve");
		}
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (n == null)
		{
			throw new ArgumentNullException("n");
		}
		this.curve = curve;
		this.g = ValidatePublicPoint(curve, g);
		this.n = n;
		this.h = h;
		this.seed = Arrays.Clone(seed);
	}

	public byte[] GetSeed()
	{
		return Arrays.Clone(seed);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is ECDomainParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected virtual bool Equals(ECDomainParameters other)
	{
		if (curve.Equals(other.curve) && g.Equals(other.g))
		{
			return n.Equals(other.n);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 4;
		num *= 257;
		num ^= curve.GetHashCode();
		num *= 257;
		num ^= g.GetHashCode();
		num *= 257;
		return num ^ n.GetHashCode();
	}

	public BigInteger ValidatePrivateScalar(BigInteger d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d", "Scalar cannot be null");
		}
		if (d.CompareTo(BigInteger.One) < 0 || d.CompareTo(N) >= 0)
		{
			throw new ArgumentException("Scalar is not in the interval [1, n - 1]", "d");
		}
		return d;
	}

	public ECPoint ValidatePublicPoint(ECPoint q)
	{
		return ValidatePublicPoint(Curve, q);
	}

	internal static ECPoint ValidatePublicPoint(ECCurve c, ECPoint q)
	{
		if (q == null)
		{
			throw new ArgumentNullException("q", "Point cannot be null");
		}
		q = ECAlgorithms.ImportPoint(c, q).Normalize();
		if (q.IsInfinity)
		{
			throw new ArgumentException("Point at infinity", "q");
		}
		if (!q.IsValid())
		{
			throw new ArgumentException("Point not on curve", "q");
		}
		return q;
	}
}
