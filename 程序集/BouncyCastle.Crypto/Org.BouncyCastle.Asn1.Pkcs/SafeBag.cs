using System;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class SafeBag : Asn1Encodable
{
	private readonly DerObjectIdentifier bagID;

	private readonly Asn1Object bagValue;

	private readonly Asn1Set bagAttributes;

	public DerObjectIdentifier BagID => bagID;

	public Asn1Object BagValue => bagValue;

	public Asn1Set BagAttributes => bagAttributes;

	public static SafeBag GetInstance(object obj)
	{
		if (obj is SafeBag)
		{
			return (SafeBag)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new SafeBag(Asn1Sequence.GetInstance(obj));
	}

	public SafeBag(DerObjectIdentifier oid, Asn1Object obj)
	{
		bagID = oid;
		bagValue = obj;
		bagAttributes = null;
	}

	public SafeBag(DerObjectIdentifier oid, Asn1Object obj, Asn1Set bagAttributes)
	{
		bagID = oid;
		bagValue = obj;
		this.bagAttributes = bagAttributes;
	}

	[Obsolete("Use 'GetInstance' instead")]
	public SafeBag(Asn1Sequence seq)
	{
		bagID = (DerObjectIdentifier)seq[0];
		bagValue = ((DerTaggedObject)seq[1]).GetObject();
		if (seq.Count == 3)
		{
			bagAttributes = (Asn1Set)seq[2];
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(bagID, new DerTaggedObject(0, bagValue));
		asn1EncodableVector.AddOptional(bagAttributes);
		return new DerSequence(asn1EncodableVector);
	}
}
