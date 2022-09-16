using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security.Certificates;

namespace Org.BouncyCastle.X509.Extension;

public class SubjectKeyIdentifierStructure : SubjectKeyIdentifier
{
	public SubjectKeyIdentifierStructure(Asn1OctetString encodedValue)
		: base((Asn1OctetString)X509ExtensionUtilities.FromExtensionValue(encodedValue))
	{
	}

	private static Asn1OctetString FromPublicKey(AsymmetricKeyParameter pubKey)
	{
		try
		{
			SubjectPublicKeyInfo spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pubKey);
			return (Asn1OctetString)new SubjectKeyIdentifier(spki).ToAsn1Object();
		}
		catch (Exception ex)
		{
			throw new CertificateParsingException("Exception extracting certificate details: " + ex.ToString());
		}
	}

	public SubjectKeyIdentifierStructure(AsymmetricKeyParameter pubKey)
		: base(FromPublicKey(pubKey))
	{
	}
}
