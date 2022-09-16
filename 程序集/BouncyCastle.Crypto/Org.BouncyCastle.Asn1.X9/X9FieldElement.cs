using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Asn1.X9;

public class X9FieldElement : Asn1Encodable
{
	private ECFieldElement f;

	public ECFieldElement Value => f;

	public X9FieldElement(ECFieldElement f)
	{
		this.f = f;
	}

	[Obsolete("Will be removed")]
	public X9FieldElement(BigInteger p, Asn1OctetString s)
		: this(new FpFieldElement(p, new BigInteger(1, s.GetOctets())))
	{
	}

	[Obsolete("Will be removed")]
	public X9FieldElement(int m, int k1, int k2, int k3, Asn1OctetString s)
		: this(new F2mFieldElement(m, k1, k2, k3, new BigInteger(1, s.GetOctets())))
	{
	}

	public override Asn1Object ToAsn1Object()
	{
		int byteLength = X9IntegerConverter.GetByteLength(f);
		byte[] str = X9IntegerConverter.IntegerToBytes(f.ToBigInteger(), byteLength);
		return new DerOctetString(str);
	}
}
