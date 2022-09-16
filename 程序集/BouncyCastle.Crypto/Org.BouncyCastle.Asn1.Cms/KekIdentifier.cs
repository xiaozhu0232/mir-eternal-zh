using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class KekIdentifier : Asn1Encodable
{
	private Asn1OctetString keyIdentifier;

	private DerGeneralizedTime date;

	private OtherKeyAttribute other;

	public Asn1OctetString KeyIdentifier => keyIdentifier;

	public DerGeneralizedTime Date => date;

	public OtherKeyAttribute Other => other;

	public KekIdentifier(byte[] keyIdentifier, DerGeneralizedTime date, OtherKeyAttribute other)
	{
		this.keyIdentifier = new DerOctetString(keyIdentifier);
		this.date = date;
		this.other = other;
	}

	public KekIdentifier(Asn1Sequence seq)
	{
		keyIdentifier = (Asn1OctetString)seq[0];
		switch (seq.Count)
		{
		case 2:
			if (seq[1] is DerGeneralizedTime)
			{
				date = (DerGeneralizedTime)seq[1];
			}
			else
			{
				other = OtherKeyAttribute.GetInstance(seq[2]);
			}
			break;
		case 3:
			date = (DerGeneralizedTime)seq[1];
			other = OtherKeyAttribute.GetInstance(seq[2]);
			break;
		default:
			throw new ArgumentException("Invalid KekIdentifier");
		case 1:
			break;
		}
	}

	public static KekIdentifier GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static KekIdentifier GetInstance(object obj)
	{
		if (obj == null || obj is KekIdentifier)
		{
			return (KekIdentifier)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new KekIdentifier((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid KekIdentifier: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(keyIdentifier);
		asn1EncodableVector.AddOptional(date, other);
		return new DerSequence(asn1EncodableVector);
	}
}
