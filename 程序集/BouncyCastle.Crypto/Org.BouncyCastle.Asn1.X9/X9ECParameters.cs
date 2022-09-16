using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.Field;

namespace Org.BouncyCastle.Asn1.X9;

public class X9ECParameters : Asn1Encodable
{
	private X9FieldID fieldID;

	private ECCurve curve;

	private X9ECPoint g;

	private BigInteger n;

	private BigInteger h;

	private byte[] seed;

	public ECCurve Curve => curve;

	public ECPoint G => g.Point;

	public BigInteger N => n;

	public BigInteger H => h;

	public X9Curve CurveEntry => new X9Curve(curve, seed);

	public X9FieldID FieldIDEntry => fieldID;

	public X9ECPoint BaseEntry => g;

	public static X9ECParameters GetInstance(object obj)
	{
		if (obj is X9ECParameters)
		{
			return (X9ECParameters)obj;
		}
		if (obj != null)
		{
			return new X9ECParameters(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public X9ECParameters(Asn1Sequence seq)
	{
		if (!(seq[0] is DerInteger) || !((DerInteger)seq[0]).Value.Equals(BigInteger.One))
		{
			throw new ArgumentException("bad version in X9ECParameters");
		}
		n = ((DerInteger)seq[4]).Value;
		if (seq.Count == 6)
		{
			h = ((DerInteger)seq[5]).Value;
		}
		X9Curve x9Curve = new X9Curve(X9FieldID.GetInstance(seq[1]), n, h, Asn1Sequence.GetInstance(seq[2]));
		curve = x9Curve.Curve;
		object obj = seq[3];
		if (obj is X9ECPoint)
		{
			g = (X9ECPoint)obj;
		}
		else
		{
			g = new X9ECPoint(curve, (Asn1OctetString)obj);
		}
		seed = x9Curve.GetSeed();
	}

	public X9ECParameters(ECCurve curve, ECPoint g, BigInteger n)
		: this(curve, g, n, null, null)
	{
	}

	public X9ECParameters(ECCurve curve, X9ECPoint g, BigInteger n, BigInteger h)
		: this(curve, g, n, h, null)
	{
	}

	public X9ECParameters(ECCurve curve, ECPoint g, BigInteger n, BigInteger h)
		: this(curve, g, n, h, null)
	{
	}

	public X9ECParameters(ECCurve curve, ECPoint g, BigInteger n, BigInteger h, byte[] seed)
		: this(curve, new X9ECPoint(g), n, h, seed)
	{
	}

	public X9ECParameters(ECCurve curve, X9ECPoint g, BigInteger n, BigInteger h, byte[] seed)
	{
		this.curve = curve;
		this.g = g;
		this.n = n;
		this.h = h;
		this.seed = seed;
		if (ECAlgorithms.IsFpCurve(curve))
		{
			fieldID = new X9FieldID(curve.Field.Characteristic);
			return;
		}
		if (ECAlgorithms.IsF2mCurve(curve))
		{
			IPolynomialExtensionField polynomialExtensionField = (IPolynomialExtensionField)curve.Field;
			int[] exponentsPresent = polynomialExtensionField.MinimalPolynomial.GetExponentsPresent();
			if (exponentsPresent.Length == 3)
			{
				fieldID = new X9FieldID(exponentsPresent[2], exponentsPresent[1]);
				return;
			}
			if (exponentsPresent.Length == 5)
			{
				fieldID = new X9FieldID(exponentsPresent[4], exponentsPresent[1], exponentsPresent[2], exponentsPresent[3]);
				return;
			}
			throw new ArgumentException("Only trinomial and pentomial curves are supported");
		}
		throw new ArgumentException("'curve' is of an unsupported type");
	}

	public byte[] GetSeed()
	{
		return seed;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new DerInteger(BigInteger.One), fieldID, new X9Curve(curve, seed), g, new DerInteger(n));
		if (h != null)
		{
			asn1EncodableVector.Add(new DerInteger(h));
		}
		return new DerSequence(asn1EncodableVector);
	}
}
