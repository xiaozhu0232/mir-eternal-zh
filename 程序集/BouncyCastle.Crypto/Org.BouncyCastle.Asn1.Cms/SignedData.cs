using System;
using System.Collections;

namespace Org.BouncyCastle.Asn1.Cms;

public class SignedData : Asn1Encodable
{
	private static readonly DerInteger Version1 = new DerInteger(1);

	private static readonly DerInteger Version3 = new DerInteger(3);

	private static readonly DerInteger Version4 = new DerInteger(4);

	private static readonly DerInteger Version5 = new DerInteger(5);

	private readonly DerInteger version;

	private readonly Asn1Set digestAlgorithms;

	private readonly ContentInfo contentInfo;

	private readonly Asn1Set certificates;

	private readonly Asn1Set crls;

	private readonly Asn1Set signerInfos;

	private readonly bool certsBer;

	private readonly bool crlsBer;

	public DerInteger Version => version;

	public Asn1Set DigestAlgorithms => digestAlgorithms;

	public ContentInfo EncapContentInfo => contentInfo;

	public Asn1Set Certificates => certificates;

	public Asn1Set CRLs => crls;

	public Asn1Set SignerInfos => signerInfos;

	public static SignedData GetInstance(object obj)
	{
		if (obj is SignedData)
		{
			return (SignedData)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new SignedData(Asn1Sequence.GetInstance(obj));
	}

	public SignedData(Asn1Set digestAlgorithms, ContentInfo contentInfo, Asn1Set certificates, Asn1Set crls, Asn1Set signerInfos)
	{
		version = CalculateVersion(contentInfo.ContentType, certificates, crls, signerInfos);
		this.digestAlgorithms = digestAlgorithms;
		this.contentInfo = contentInfo;
		this.certificates = certificates;
		this.crls = crls;
		this.signerInfos = signerInfos;
		crlsBer = crls is BerSet;
		certsBer = certificates is BerSet;
	}

	private DerInteger CalculateVersion(DerObjectIdentifier contentOid, Asn1Set certs, Asn1Set crls, Asn1Set signerInfs)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (certs != null)
		{
			foreach (object cert in certs)
			{
				if (cert is Asn1TaggedObject)
				{
					Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)cert;
					if (asn1TaggedObject.TagNo == 1)
					{
						flag3 = true;
					}
					else if (asn1TaggedObject.TagNo == 2)
					{
						flag4 = true;
					}
					else if (asn1TaggedObject.TagNo == 3)
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			return Version5;
		}
		if (crls != null)
		{
			foreach (object crl in crls)
			{
				if (crl is Asn1TaggedObject)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2)
		{
			return Version5;
		}
		if (flag4)
		{
			return Version4;
		}
		if (flag3 || !CmsObjectIdentifiers.Data.Equals(contentOid) || CheckForVersion3(signerInfs))
		{
			return Version3;
		}
		return Version1;
	}

	private bool CheckForVersion3(Asn1Set signerInfs)
	{
		foreach (object signerInf in signerInfs)
		{
			SignerInfo instance = SignerInfo.GetInstance(signerInf);
			if (instance.Version.IntValueExact == 3)
			{
				return true;
			}
		}
		return false;
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
					certsBer = asn1TaggedObject is BerTaggedObject;
					certificates = Asn1Set.GetInstance(asn1TaggedObject, explicitly: false);
					break;
				case 1:
					crlsBer = asn1TaggedObject is BerTaggedObject;
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
		if (certificates != null)
		{
			if (certsBer)
			{
				asn1EncodableVector.Add(new BerTaggedObject(explicitly: false, 0, certificates));
			}
			else
			{
				asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 0, certificates));
			}
		}
		if (crls != null)
		{
			if (crlsBer)
			{
				asn1EncodableVector.Add(new BerTaggedObject(explicitly: false, 1, crls));
			}
			else
			{
				asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 1, crls));
			}
		}
		asn1EncodableVector.Add(signerInfos);
		return new BerSequence(asn1EncodableVector);
	}
}
