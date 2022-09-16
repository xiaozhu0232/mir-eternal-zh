using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class AuthEnvelopedData : Asn1Encodable
{
	private DerInteger version;

	private OriginatorInfo originatorInfo;

	private Asn1Set recipientInfos;

	private EncryptedContentInfo authEncryptedContentInfo;

	private Asn1Set authAttrs;

	private Asn1OctetString mac;

	private Asn1Set unauthAttrs;

	public DerInteger Version => version;

	public OriginatorInfo OriginatorInfo => originatorInfo;

	public Asn1Set RecipientInfos => recipientInfos;

	public EncryptedContentInfo AuthEncryptedContentInfo => authEncryptedContentInfo;

	public Asn1Set AuthAttrs => authAttrs;

	public Asn1OctetString Mac => mac;

	public Asn1Set UnauthAttrs => unauthAttrs;

	public AuthEnvelopedData(OriginatorInfo originatorInfo, Asn1Set recipientInfos, EncryptedContentInfo authEncryptedContentInfo, Asn1Set authAttrs, Asn1OctetString mac, Asn1Set unauthAttrs)
	{
		version = new DerInteger(0);
		this.originatorInfo = originatorInfo;
		this.recipientInfos = recipientInfos;
		this.authEncryptedContentInfo = authEncryptedContentInfo;
		this.authAttrs = authAttrs;
		this.mac = mac;
		this.unauthAttrs = unauthAttrs;
	}

	private AuthEnvelopedData(Asn1Sequence seq)
	{
		int num = 0;
		Asn1Object asn1Object = seq[num++].ToAsn1Object();
		version = (DerInteger)asn1Object;
		asn1Object = seq[num++].ToAsn1Object();
		if (asn1Object is Asn1TaggedObject)
		{
			originatorInfo = OriginatorInfo.GetInstance((Asn1TaggedObject)asn1Object, explicitly: false);
			asn1Object = seq[num++].ToAsn1Object();
		}
		recipientInfos = Asn1Set.GetInstance(asn1Object);
		asn1Object = seq[num++].ToAsn1Object();
		authEncryptedContentInfo = EncryptedContentInfo.GetInstance(asn1Object);
		asn1Object = seq[num++].ToAsn1Object();
		if (asn1Object is Asn1TaggedObject)
		{
			authAttrs = Asn1Set.GetInstance((Asn1TaggedObject)asn1Object, explicitly: false);
			asn1Object = seq[num++].ToAsn1Object();
		}
		mac = Asn1OctetString.GetInstance(asn1Object);
		if (seq.Count > num)
		{
			asn1Object = seq[num++].ToAsn1Object();
			unauthAttrs = Asn1Set.GetInstance((Asn1TaggedObject)asn1Object, explicitly: false);
		}
	}

	public static AuthEnvelopedData GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public static AuthEnvelopedData GetInstance(object obj)
	{
		if (obj == null || obj is AuthEnvelopedData)
		{
			return (AuthEnvelopedData)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new AuthEnvelopedData((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid AuthEnvelopedData: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, originatorInfo);
		asn1EncodableVector.Add(recipientInfos, authEncryptedContentInfo);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, authAttrs);
		asn1EncodableVector.Add(mac);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 2, unauthAttrs);
		return new BerSequence(asn1EncodableVector);
	}
}
