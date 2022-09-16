using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security.Certificates;

namespace Org.BouncyCastle.X509;

public class PrincipalUtilities
{
	public static X509Name GetIssuerX509Principal(X509Certificate cert)
	{
		try
		{
			TbsCertificateStructure instance = TbsCertificateStructure.GetInstance(Asn1Object.FromByteArray(cert.GetTbsCertificate()));
			return instance.Issuer;
		}
		catch (Exception e)
		{
			throw new CertificateEncodingException("Could not extract issuer", e);
		}
	}

	public static X509Name GetSubjectX509Principal(X509Certificate cert)
	{
		try
		{
			TbsCertificateStructure instance = TbsCertificateStructure.GetInstance(Asn1Object.FromByteArray(cert.GetTbsCertificate()));
			return instance.Subject;
		}
		catch (Exception e)
		{
			throw new CertificateEncodingException("Could not extract subject", e);
		}
	}

	public static X509Name GetIssuerX509Principal(X509Crl crl)
	{
		try
		{
			TbsCertificateList instance = TbsCertificateList.GetInstance(Asn1Object.FromByteArray(crl.GetTbsCertList()));
			return instance.Issuer;
		}
		catch (Exception e)
		{
			throw new CrlException("Could not extract issuer", e);
		}
	}
}
