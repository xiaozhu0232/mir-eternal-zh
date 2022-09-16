using System;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class CertOrEncCert : Asn1Encodable, IAsn1Choice
{
	private readonly CmpCertificate certificate;

	private readonly EncryptedValue encryptedCert;

	public virtual CmpCertificate Certificate => certificate;

	public virtual EncryptedValue EncryptedCert => encryptedCert;

	private CertOrEncCert(Asn1TaggedObject tagged)
	{
		if (tagged.TagNo == 0)
		{
			certificate = CmpCertificate.GetInstance(tagged.GetObject());
			return;
		}
		if (tagged.TagNo == 1)
		{
			encryptedCert = EncryptedValue.GetInstance(tagged.GetObject());
			return;
		}
		throw new ArgumentException("unknown tag: " + tagged.TagNo, "tagged");
	}

	public static CertOrEncCert GetInstance(object obj)
	{
		if (obj is CertOrEncCert)
		{
			return (CertOrEncCert)obj;
		}
		if (obj is Asn1TaggedObject)
		{
			return new CertOrEncCert((Asn1TaggedObject)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public CertOrEncCert(CmpCertificate certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		this.certificate = certificate;
	}

	public CertOrEncCert(EncryptedValue encryptedCert)
	{
		if (encryptedCert == null)
		{
			throw new ArgumentNullException("encryptedCert");
		}
		this.encryptedCert = encryptedCert;
	}

	public override Asn1Object ToAsn1Object()
	{
		if (certificate != null)
		{
			return new DerTaggedObject(explicitly: true, 0, certificate);
		}
		return new DerTaggedObject(explicitly: true, 1, encryptedCert);
	}
}
