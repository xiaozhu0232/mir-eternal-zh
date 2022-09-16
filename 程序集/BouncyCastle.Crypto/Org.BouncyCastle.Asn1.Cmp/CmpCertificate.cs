using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class CmpCertificate : Asn1Encodable, IAsn1Choice
{
	private readonly X509CertificateStructure x509v3PKCert;

	private readonly AttributeCertificate x509v2AttrCert;

	public virtual bool IsX509v3PKCert => x509v3PKCert != null;

	public virtual X509CertificateStructure X509v3PKCert => x509v3PKCert;

	public virtual AttributeCertificate X509v2AttrCert => x509v2AttrCert;

	public CmpCertificate(AttributeCertificate x509v2AttrCert)
	{
		this.x509v2AttrCert = x509v2AttrCert;
	}

	public CmpCertificate(X509CertificateStructure x509v3PKCert)
	{
		if (x509v3PKCert.Version != 3)
		{
			throw new ArgumentException("only version 3 certificates allowed", "x509v3PKCert");
		}
		this.x509v3PKCert = x509v3PKCert;
	}

	public static CmpCertificate GetInstance(object obj)
	{
		if (obj is CmpCertificate)
		{
			return (CmpCertificate)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CmpCertificate(X509CertificateStructure.GetInstance(obj));
		}
		if (obj is Asn1TaggedObject)
		{
			return new CmpCertificate(AttributeCertificate.GetInstance(((Asn1TaggedObject)obj).GetObject()));
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public override Asn1Object ToAsn1Object()
	{
		if (x509v2AttrCert != null)
		{
			return new DerTaggedObject(explicitly: true, 1, x509v2AttrCert);
		}
		return x509v3PKCert.ToAsn1Object();
	}
}
