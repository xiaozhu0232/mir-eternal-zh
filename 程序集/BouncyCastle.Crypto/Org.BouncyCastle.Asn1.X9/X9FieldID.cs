using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.X9;

public class X9FieldID : Asn1Encodable
{
	private readonly DerObjectIdentifier id;

	private readonly Asn1Object parameters;

	public DerObjectIdentifier Identifier => id;

	public Asn1Object Parameters => parameters;

	public X9FieldID(BigInteger primeP)
	{
		id = X9ObjectIdentifiers.PrimeField;
		parameters = new DerInteger(primeP);
	}

	public X9FieldID(int m, int k1)
		: this(m, k1, 0, 0)
	{
	}

	public X9FieldID(int m, int k1, int k2, int k3)
	{
		id = X9ObjectIdentifiers.CharacteristicTwoField;
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new DerInteger(m));
		if (k2 == 0)
		{
			if (k3 != 0)
			{
				throw new ArgumentException("inconsistent k values");
			}
			asn1EncodableVector.Add(X9ObjectIdentifiers.TPBasis, new DerInteger(k1));
		}
		else
		{
			if (k2 <= k1 || k3 <= k2)
			{
				throw new ArgumentException("inconsistent k values");
			}
			asn1EncodableVector.Add(X9ObjectIdentifiers.PPBasis, new DerSequence(new DerInteger(k1), new DerInteger(k2), new DerInteger(k3)));
		}
		parameters = new DerSequence(asn1EncodableVector);
	}

	private X9FieldID(Asn1Sequence seq)
	{
		id = DerObjectIdentifier.GetInstance(seq[0]);
		parameters = seq[1].ToAsn1Object();
	}

	public static X9FieldID GetInstance(object obj)
	{
		if (obj is X9FieldID)
		{
			return (X9FieldID)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new X9FieldID(Asn1Sequence.GetInstance(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(id, parameters);
	}
}
