using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class RsaPrivateKeyStructure : Asn1Encodable
{
	private readonly BigInteger modulus;

	private readonly BigInteger publicExponent;

	private readonly BigInteger privateExponent;

	private readonly BigInteger prime1;

	private readonly BigInteger prime2;

	private readonly BigInteger exponent1;

	private readonly BigInteger exponent2;

	private readonly BigInteger coefficient;

	public BigInteger Modulus => modulus;

	public BigInteger PublicExponent => publicExponent;

	public BigInteger PrivateExponent => privateExponent;

	public BigInteger Prime1 => prime1;

	public BigInteger Prime2 => prime2;

	public BigInteger Exponent1 => exponent1;

	public BigInteger Exponent2 => exponent2;

	public BigInteger Coefficient => coefficient;

	public static RsaPrivateKeyStructure GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public static RsaPrivateKeyStructure GetInstance(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is RsaPrivateKeyStructure)
		{
			return (RsaPrivateKeyStructure)obj;
		}
		return new RsaPrivateKeyStructure(Asn1Sequence.GetInstance(obj));
	}

	public RsaPrivateKeyStructure(BigInteger modulus, BigInteger publicExponent, BigInteger privateExponent, BigInteger prime1, BigInteger prime2, BigInteger exponent1, BigInteger exponent2, BigInteger coefficient)
	{
		this.modulus = modulus;
		this.publicExponent = publicExponent;
		this.privateExponent = privateExponent;
		this.prime1 = prime1;
		this.prime2 = prime2;
		this.exponent1 = exponent1;
		this.exponent2 = exponent2;
		this.coefficient = coefficient;
	}

	[Obsolete("Use 'GetInstance' method(s) instead")]
	public RsaPrivateKeyStructure(Asn1Sequence seq)
	{
		BigInteger value = ((DerInteger)seq[0]).Value;
		if (value.IntValue != 0)
		{
			throw new ArgumentException("wrong version for RSA private key");
		}
		modulus = ((DerInteger)seq[1]).Value;
		publicExponent = ((DerInteger)seq[2]).Value;
		privateExponent = ((DerInteger)seq[3]).Value;
		prime1 = ((DerInteger)seq[4]).Value;
		prime2 = ((DerInteger)seq[5]).Value;
		exponent1 = ((DerInteger)seq[6]).Value;
		exponent2 = ((DerInteger)seq[7]).Value;
		coefficient = ((DerInteger)seq[8]).Value;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(new DerInteger(0), new DerInteger(Modulus), new DerInteger(PublicExponent), new DerInteger(PrivateExponent), new DerInteger(Prime1), new DerInteger(Prime2), new DerInteger(Exponent1), new DerInteger(Exponent2), new DerInteger(Coefficient));
	}
}
