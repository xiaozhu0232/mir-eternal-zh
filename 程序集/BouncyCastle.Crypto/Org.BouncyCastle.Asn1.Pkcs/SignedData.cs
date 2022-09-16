using System;
using System.Collections;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class SignedData : Asn1Encodable
{
	private readonly DerInteger version;

	private readonly Asn1Set digestAlgorithms;

	private readonly ContentInfo contentInfo;

	private readonly Asn1Set certificates;

	private readonly Asn1Set crls;

	private readonly Asn1Set signerInfos;

	public DerInteger Version => version;

	public Asn1Set DigestAlgorithms => digestAlgorithms;

	public ContentInfo ContentInfo => contentInfo;

	public Asn1Set Certificates => certificates;

	public Asn1Set Crls => crls;

	public Asn1Set SignerInfos => signerInfos;

	public static SignedData GetInstance(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is SignedData result)
		{
			return result;
		}
		return new SignedData(Asn1Sequence.GetInstance(obj));
	}

	public SignedData(DerInteger _version, Asn1Set _digestAlgorithms, ContentInfo _contentInfo, Asn1Set _certificates, Asn1Set _crls, Asn1Set _signerInfos)
	{
		version = _version;
		digestAlgorithms = _digestAlgorithms;
		contentInfo = _contentInfo;
		certificates = _certificates;
		crls = _crls;
		signerInfos = _signerInfos;
	}

	private SignedData(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		version = (DerInteger)enumerator.Current;
		enumerator.MoveNext();
		digestAlgorithms = (Asn1Set)enumerator.Current;
		enumerator.MoveNext();
		contentInfo = ContentInfo.GetInstance(enumerator.Current);
		while (enumerator.MoveNext())
		{
			Asn1Object asn1Object = (Asn1Object)enumerator.Current;
			if (asn1Object is Asn1TaggedObject)
			{
				Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)asn1Object;
				switch (asn1TaggedObject.TagNo)
				{
				case 0:
					certificates = Asn1Set.GetInstance(asn1TaggedObject, explicitly: false);
					break;
				case 1:
					crls = Asn1Set.GetInstance(asn1TaggedObject, explicitly: false);
					break;
				default:
					throw new ArgumentException("unknown tag value " + asn1TaggedObject.TagNo);
				}
			}
			else
			{
				signerInfos = (Asn1Set)asn1Object;
			}
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, digestAlgorithms, contentInfo);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, certificates);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, crls);
		asn1EncodableVector.Add(signerInfos);
		return new BerSequence(asn1EncodableVector);
	}
}
