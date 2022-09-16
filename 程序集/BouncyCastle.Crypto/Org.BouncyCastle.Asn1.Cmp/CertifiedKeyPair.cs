using System;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class CertifiedKeyPair : Asn1Encodable
{
	private readonly CertOrEncCert certOrEncCert;

	private readonly EncryptedValue privateKey;

	private readonly PkiPublicationInfo publicationInfo;

	public virtual CertOrEncCert CertOrEncCert => certOrEncCert;

	public virtual EncryptedValue PrivateKey => privateKey;

	public virtual PkiPublicationInfo PublicationInfo => publicationInfo;

	private CertifiedKeyPair(Asn1Sequence seq)
	{
		certOrEncCert = CertOrEncCert.GetInstance(seq[0]);
		if (seq.Count < 2)
		{
			return;
		}
		if (seq.Count == 2)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[1]);
			if (instance.TagNo == 0)
			{
				privateKey = EncryptedValue.GetInstance(instance.GetObject());
			}
			else
			{
				publicationInfo = PkiPublicationInfo.GetInstance(instance.GetObject());
			}
		}
		else
		{
			privateKey = EncryptedValue.GetInstance(Asn1TaggedObject.GetInstance(seq[1]));
			publicationInfo = PkiPublicationInfo.GetInstance(Asn1TaggedObject.GetInstance(seq[2]));
		}
	}

	public static CertifiedKeyPair GetInstance(object obj)
	{
		if (obj is CertifiedKeyPair)
		{
			return (CertifiedKeyPair)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CertifiedKeyPair((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public CertifiedKeyPair(CertOrEncCert certOrEncCert)
		: this(certOrEncCert, null, null)
	{
	}

	public CertifiedKeyPair(CertOrEncCert certOrEncCert, EncryptedValue privateKey, PkiPublicationInfo publicationInfo)
	{
		if (certOrEncCert == null)
		{
			throw new ArgumentNullException("certOrEncCert");
		}
		this.certOrEncCert = certOrEncCert;
		this.privateKey = privateKey;
		this.publicationInfo = publicationInfo;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(certOrEncCert);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, privateKey);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, publicationInfo);
		return new DerSequence(asn1EncodableVector);
	}
}
