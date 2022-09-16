using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X9;

public class X9Curve : Asn1Encodable
{
	private readonly ECCurve curve;

	private readonly byte[] seed;

	private readonly DerObjectIdentifier fieldIdentifier;

	public ECCurve Curve => curve;

	public X9Curve(ECCurve curve)
		: this(curve, null)
	{
	}

	public X9Curve(ECCurve curve, byte[] seed)
	{
		if (curve == null)
		{
			throw new ArgumentNullException("curve");
		}
		this.curve = curve;
		this.seed = Arrays.Clone(seed);
		if (ECAlgorithms.IsFpCurve(curve))
		{
			fieldIdentifier = X9ObjectIdentifiers.PrimeField;
			return;
		}
		if (ECAlgorithms.IsF2mCurve(curve))
		{
			fieldIdentifier = X9ObjectIdentifiers.CharacteristicTwoField;
			return;
		}
		throw new ArgumentException("This type of ECCurve is not implemented");
	}

	[Obsolete("Use constructor including order/cofactor")]
	public X9Curve(X9FieldID fieldID, Asn1Sequence seq)
		: this(fieldID, null, null, seq)
	{
	}

	public X9Curve(X9FieldID fieldID, BigInteger order, BigInteger cofactor, Asn1Sequence seq)
	{
		if (fieldID == null)
		{
			throw new ArgumentNullException("fieldID");
		}
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		fieldIdentifier = fieldID.Identifier;
		if (fieldIdentifier.Equals(X9ObjectIdentifiers.PrimeField))
		{
			BigInteger value = ((DerInteger)fieldID.Parameters).Value;
			BigInteger a = new BigInteger(1, Asn1OctetString.GetInstance(seq[0]).GetOctets());
			BigInteger b = new BigInteger(1, Asn1OctetString.GetInstance(seq[1]).GetOctets());
			curve = new FpCurve(value, a, b, order, cofactor);
		}
		else
		{
			if (!fieldIdentifier.Equals(X9ObjectIdentifiers.CharacteristicTwoField))
			{
				throw new ArgumentException("This type of ECCurve is not implemented");
			}
			DerSequence derSequence = (DerSequence)fieldID.Parameters;
			int intValueExact = ((DerInteger)derSequence[0]).IntValueExact;
			DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)derSequence[1];
			int num = 0;
			int k = 0;
			int k2 = 0;
			if (derObjectIdentifier.Equals(X9ObjectIdentifiers.TPBasis))
			{
				num = ((DerInteger)derSequence[2]).IntValueExact;
			}
			else
			{
				DerSequence derSequence2 = (DerSequence)derSequence[2];
				num = ((DerInteger)derSequence2[0]).IntValueExact;
				k = ((DerInteger)derSequence2[1]).IntValueExact;
				k2 = ((DerInteger)derSequence2[2]).IntValueExact;
			}
			BigInteger a2 = new BigInteger(1, Asn1OctetString.GetInstance(seq[0]).GetOctets());
			BigInteger b2 = new BigInteger(1, Asn1OctetString.GetInstance(seq[1]).GetOctets());
			curve = new F2mCurve(intValueExact, num, k, k2, a2, b2, order, cofactor);
		}
		if (seq.Count == 3)
		{
			seed = ((DerBitString)seq[2]).GetBytes();
		}
	}

	public byte[] GetSeed()
	{
		return Arrays.Clone(seed);
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		if (fieldIdentifier.Equals(X9ObjectIdentifiers.PrimeField) || fieldIdentifier.Equals(X9ObjectIdentifiers.CharacteristicTwoField))
		{
			asn1EncodableVector.Add(new X9FieldElement(curve.A).ToAsn1Object());
			asn1EncodableVector.Add(new X9FieldElement(curve.B).ToAsn1Object());
		}
		if (seed != null)
		{
			asn1EncodableVector.Add(new DerBitString(seed));
		}
		return new DerSequence(asn1EncodableVector);
	}
}
