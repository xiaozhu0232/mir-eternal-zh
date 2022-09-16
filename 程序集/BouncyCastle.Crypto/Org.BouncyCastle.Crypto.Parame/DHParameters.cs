using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DHParameters : ICipherParameters
{
	private const int DefaultMinimumLength = 160;

	private readonly BigInteger p;

	private readonly BigInteger g;

	private readonly BigInteger q;

	private readonly BigInteger j;

	private readonly int m;

	private readonly int l;

	private readonly DHValidationParameters validation;

	public BigInteger P => p;

	public BigInteger G => g;

	public BigInteger Q => q;

	public BigInteger J => j;

	public int M => m;

	public int L => l;

	public DHValidationParameters ValidationParameters => validation;

	private static int GetDefaultMParam(int lParam)
	{
		if (lParam == 0)
		{
			return 160;
		}
		return System.Math.Min(lParam, 160);
	}

	public DHParameters(BigInteger p, BigInteger g)
		: this(p, g, null, 0)
	{
	}

	public DHParameters(BigInteger p, BigInteger g, BigInteger q)
		: this(p, g, q, 0)
	{
	}

	public DHParameters(BigInteger p, BigInteger g, BigInteger q, int l)
		: this(p, g, q, GetDefaultMParam(l), l, null, null)
	{
	}

	public DHParameters(BigInteger p, BigInteger g, BigInteger q, int m, int l)
		: this(p, g, q, m, l, null, null)
	{
	}

	public DHParameters(BigInteger p, BigInteger g, BigInteger q, BigInteger j, DHValidationParameters validation)
		: this(p, g, q, 160, 0, j, validation)
	{
	}

	public DHParameters(BigInteger p, BigInteger g, BigInteger q, int m, int l, BigInteger j, DHValidationParameters validation)
	{
		if (p == null)
		{
			throw new ArgumentNullException("p");
		}
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (!p.TestBit(0))
		{
			throw new ArgumentException("field must be an odd prime", "p");
		}
		if (g.CompareTo(BigInteger.Two) < 0 || g.CompareTo(p.Subtract(BigInteger.Two)) > 0)
		{
			throw new ArgumentException("generator must in the range [2, p - 2]", "g");
		}
		if (q != null && q.BitLength >= p.BitLength)
		{
			throw new ArgumentException("q too big to be a factor of (p-1)", "q");
		}
		if (m >= p.BitLength)
		{
			throw new ArgumentException("m value must be < bitlength of p", "m");
		}
		if (l != 0)
		{
			if (l >= p.BitLength)
			{
				throw new ArgumentException("when l value specified, it must be less than bitlength(p)", "l");
			}
			if (l < m)
			{
				throw new ArgumentException("when l value specified, it may not be less than m value", "l");
			}
		}
		if (j != null && j.CompareTo(BigInteger.Two) < 0)
		{
			throw new ArgumentException("subgroup factor must be >= 2", "j");
		}
		this.p = p;
		this.g = g;
		this.q = q;
		this.m = m;
		this.l = l;
		this.j = j;
		this.validation = validation;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is DHParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected virtual bool Equals(DHParameters other)
	{
		if (p.Equals(other.p) && g.Equals(other.g))
		{
			return object.Equals(q, other.q);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = p.GetHashCode() ^ g.GetHashCode();
		if (q != null)
		{
			num ^= q.GetHashCode();
		}
		return num;
	}
}
