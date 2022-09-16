using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;

namespace Org.BouncyCastle.X509.Extension;

public class AuthorityKeyIdentifierStructure : AuthorityKeyIdentifier
{
	public AuthorityKeyIdentifierStructure(Asn1OctetString encodedValue)
		: base((Asn1Sequence)X509ExtensionUtilities.FromExtensionValue(encodedValue))
	{
	}

	private static Asn1Sequence FromCertificate(X509Certificate certificate)
	{
		try
		{
			GeneralName name = new GeneralName(PrincipalUtilities.GetIssuerX509Principal(certificate));
			if (certificate.Version == 3)
			{
				Asn1OctetString extensionValue = certificate.GetExtensionValue(X509Extensions.SubjectKeyIdentifier);
				if (extensionValue != null)
				{
					Asn1OctetString asn1OctetString = (Asn1OctetString)X509ExtensionUtilities.FromExtensionValue(extensionValue);
					return (Asn1Sequence)new AuthorityKeyIdentifier(asn1OctetString.GetOctets(), new GeneralNames(name), certificate.SerialNumber).ToAsn1Object();
				}
			}
			SubjectPublicKeyInfo spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(certificate.GetPublicKey());
			return (Asn1Sequence)new AuthorityKeyIdentifier(spki, new GeneralNames(name), certificate.SerialNumber).ToAsn1Object();
		}
		catch (Exception exception)
		{
			throw new CertificateParsingException("Exception extracting certificate details", exception);
		}
	}

	private static Asn1Sequence FromKey(AsymmetricKeyParameter pubKey)
	{
		try
		{
			SubjectPublicKeyInfo spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pubKey);
			return (Asn1Sequence)new AuthorityKeyIdentifier(spki).ToAsn1Object();
		}
		catch (Exception ex)
		{
			throw new InvalidKeyException("can't process key: " + ex);
		}
	}

	public AuthorityKeyIdentifierStructure(X509Certificate certificate)
		: base(FromCertificate(certificate))
	{
	}

	public AuthorityKeyIdentifierStructure(AsymmetricKeyParameter pubKey)
		: base(FromKey(pubKey))
	{
	}
}
