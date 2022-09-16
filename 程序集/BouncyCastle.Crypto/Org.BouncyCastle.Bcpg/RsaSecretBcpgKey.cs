using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg;

public class RsaSecretBcpgKey : BcpgObject, IBcpgKey
{
	private readonly MPInteger d;

	private readonly MPInteger p;

	private readonly MPInteger q;

	private readonly MPInteger u;

	private readonly BigInteger expP;

	private readonly BigInteger expQ;

	private readonly BigInteger crt;

	public BigInteger Modulus => p.Value.Multiply(q.Value);

	public BigInteger PrivateExponent => d.Value;

	public BigInteger PrimeP => p.Value;

	public BigInteger PrimeQ => q.Value;

	public BigInteger PrimeExponentP => expP;

	public BigInteger PrimeExponentQ => expQ;

	public BigInteger CrtCoefficient => crt;

	public string Format => "PGP";

	public RsaSecretBcpgKey(BcpgInputStream bcpgIn)
	{
		d = new MPInteger(bcpgIn);
		p = new MPInteger(bcpgIn);
		q = new MPInteger(bcpgIn);
		u = new MPInteger(bcpgIn);
		expP = d.Value.Remainder(p.Value.Subtract(BigInteger.One));
		expQ = d.Value.Remainder(q.Value.Subtract(BigInteger.One));
		crt = BigIntegers.ModOddInverse(p.Value, q.Value);
	}

	public RsaSecretBcpgKey(BigInteger d, BigInteger p, BigInteger q)
	{
		int num = p.CompareTo(q);
		if (num >= 0)
		{
			if (num == 0)
			{
				throw new ArgumentException("p and q cannot be equal");
			}
			BigInteger bigInteger = p;
			p = q;
			q = bigInteger;
		}
		this.d = new MPInteger(d);
		this.p = new MPInteger(p);
		this.q = new MPInteger(q);
		u = new MPInteger(BigIntegers.ModOddInverse(q, p));
		expP = d.Remainder(p.Subtract(BigInteger.One));
		expQ = d.Remainder(q.Subtract(BigInteger.One));
		crt = BigIntegers.ModOddInverse(p, q);
	}

	public override byte[] GetEncoded()
	{
		try
		{
			return base.GetEncoded();
		}
		catch (Exception)
		{
			return null;
		}
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WriteObjects(d, p, q, u);
	}
}
