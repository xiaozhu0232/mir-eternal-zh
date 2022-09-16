using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class OriginatorInfo : Asn1Encodable
{
	private Asn1Set certs;

	private Asn1Set crls;

	public Asn1Set Certificates => certs;

	public Asn1Set Crls => crls;

	public OriginatorInfo(Asn1Set certs, Asn1Set crls)
	{
		this.certs = certs;
		this.crls = crls;
	}

	public OriginatorInfo(Asn1Sequence seq)
	{
		switch (seq.Count)
		{
		case 1:
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[0];
			switch (asn1TaggedObject.TagNo)
			{
			case 0:
				certs = Asn1Set.GetInstance(asn1TaggedObject, explicitly: false);
				break;
			case 1:
				crls = Asn1Set.GetInstance(asn1TaggedObject, explicitly: false);
				break;
			default:
				throw new ArgumentException("Bad tag in OriginatorInfo: " + asn1TaggedObject.TagNo);
			}
			break;
		}
		case 2:
			certs = Asn1Set.GetInstance((Asn1TaggedObject)seq[0], explicitly: false);
			crls = Asn1Set.GetInstance((Asn1TaggedObject)seq[1], explicitly: false);
			break;
		default:
			throw new ArgumentException("OriginatorInfo too big");
		case 0:
			break;
		}
	}

	public static OriginatorInfo GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static OriginatorInfo GetInstance(object obj)
	{
		if (obj == null || obj is OriginatorInfo)
		{
			return (OriginatorInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OriginatorInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid OriginatorInfo: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, certs);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, crls);
		return new DerSequence(asn1EncodableVector);
	}
}
