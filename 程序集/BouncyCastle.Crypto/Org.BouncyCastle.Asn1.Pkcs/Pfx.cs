using System;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class Pfx : Asn1Encodable
{
	private readonly ContentInfo contentInfo;

	private readonly MacData macData;

	public ContentInfo AuthSafe => contentInfo;

	public MacData MacData => macData;

	public static Pfx GetInstance(object obj)
	{
		if (obj is Pfx)
		{
			return (Pfx)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new Pfx(Asn1Sequence.GetInstance(obj));
	}

	[Obsolete("Use 'GetInstance' instead")]
	public Pfx(Asn1Sequence seq)
	{
		DerInteger instance = DerInteger.GetInstance(seq[0]);
		if (instance.IntValueExact != 3)
		{
			throw new ArgumentException("wrong version for PFX PDU");
		}
		contentInfo = ContentInfo.GetInstance(seq[1]);
		if (seq.Count == 3)
		{
			macData = MacData.GetInstance(seq[2]);
		}
	}

	public Pfx(ContentInfo contentInfo, MacData macData)
	{
		this.contentInfo = contentInfo;
		this.macData = macData;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new DerInteger(3), contentInfo);
		asn1EncodableVector.AddOptional(macData);
		return new BerSequence(asn1EncodableVector);
	}
}
