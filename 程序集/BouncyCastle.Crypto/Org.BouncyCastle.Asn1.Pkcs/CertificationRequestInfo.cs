using System;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class CertificationRequestInfo : Asn1Encodable
{
	internal DerInteger version = new DerInteger(0);

	internal X509Name subject;

	internal SubjectPublicKeyInfo subjectPKInfo;

	internal Asn1Set attributes;

	public DerInteger Version => version;

	public X509Name Subject => subject;

	public SubjectPublicKeyInfo SubjectPublicKeyInfo => subjectPKInfo;

	public Asn1Set Attributes => attributes;

	public static CertificationRequestInfo GetInstance(object obj)
	{
		if (obj is CertificationRequestInfo)
		{
			return (CertificationRequestInfo)obj;
		}
		if (obj != null)
		{
			return new CertificationRequestInfo(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public CertificationRequestInfo(X509Name subject, SubjectPublicKeyInfo pkInfo, Asn1Set attributes)
	{
		this.subject = subject;
		subjectPKInfo = pkInfo;
		this.attributes = attributes;
		ValidateAttributes(attributes);
		if (subject == null || version == null || subjectPKInfo == null)
		{
			throw new ArgumentException("Not all mandatory fields set in CertificationRequestInfo generator.");
		}
	}

	private CertificationRequestInfo(Asn1Sequence seq)
	{
		version = (DerInteger)seq[0];
		subject = X509Name.GetInstance(seq[1]);
		subjectPKInfo = SubjectPublicKeyInfo.GetInstance(seq[2]);
		if (seq.Count > 3)
		{
			DerTaggedObject obj = (DerTaggedObject)seq[3];
			attributes = Asn1Set.GetInstance(obj, explicitly: false);
		}
		ValidateAttributes(attributes);
		if (subject == null || version == null || subjectPKInfo == null)
		{
			throw new ArgumentException("Not all mandatory fields set in CertificationRequestInfo generator.");
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, subject, subjectPKInfo);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, attributes);
		return new DerSequence(asn1EncodableVector);
	}

	private static void ValidateAttributes(Asn1Set attributes)
	{
		if (attributes == null)
		{
			return;
		}
		foreach (Asn1Encodable attribute in attributes)
		{
			Asn1Object obj = attribute.ToAsn1Object();
			AttributePkcs instance = AttributePkcs.GetInstance(obj);
			if (instance.AttrType.Equals(PkcsObjectIdentifiers.Pkcs9AtChallengePassword) && instance.AttrValues.Count != 1)
			{
				throw new ArgumentException("challengePassword attribute must have one value");
			}
		}
	}
}
