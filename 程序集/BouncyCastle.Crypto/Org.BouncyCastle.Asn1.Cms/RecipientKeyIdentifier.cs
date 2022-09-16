using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class RecipientKeyIdentifier : Asn1Encodable
{
	private Asn1OctetString subjectKeyIdentifier;

	private DerGeneralizedTime date;

	private OtherKeyAttribute other;

	public Asn1OctetString SubjectKeyIdentifier => subjectKeyIdentifier;

	public DerGeneralizedTime Date => date;

	public OtherKeyAttribute OtherKeyAttribute => other;

	public RecipientKeyIdentifier(Asn1OctetString subjectKeyIdentifier, DerGeneralizedTime date, OtherKeyAttribute other)
	{
		this.subjectKeyIdentifier = subjectKeyIdentifier;
		this.date = date;
		this.other = other;
	}

	public RecipientKeyIdentifier(byte[] subjectKeyIdentifier)
		: this(subjectKeyIdentifier, null, null)
	{
	}

	public RecipientKeyIdentifier(byte[] subjectKeyIdentifier, DerGeneralizedTime date, OtherKeyAttribute other)
	{
		this.subjectKeyIdentifier = new DerOctetString(subjectKeyIdentifier);
		this.date = date;
		this.other = other;
	}

	public RecipientKeyIdentifier(Asn1Sequence seq)
	{
		subjectKeyIdentifier = Asn1OctetString.GetInstance(seq[0]);
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
			throw new ArgumentException("Invalid RecipientKeyIdentifier");
		case 1:
			break;
		}
	}

	public static RecipientKeyIdentifier GetInstance(Asn1TaggedObject ato, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(ato, explicitly));
	}

	public static RecipientKeyIdentifier GetInstance(object obj)
	{
		if (obj == null || obj is RecipientKeyIdentifier)
		{
			return (RecipientKeyIdentifier)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RecipientKeyIdentifier((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid RecipientKeyIdentifier: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(subjectKeyIdentifier);
		asn1EncodableVector.AddOptional(date, other);
		return new DerSequence(asn1EncodableVector);
	}
}
