using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms;

public class SignerInfoGeneratorBuilder
{
	private bool directSignature;

	private CmsAttributeTableGenerator signedGen;

	private CmsAttributeTableGenerator unsignedGen;

	public SignerInfoGeneratorBuilder SetDirectSignature(bool hasNoSignedAttributes)
	{
		directSignature = hasNoSignedAttributes;
		return this;
	}

	public SignerInfoGeneratorBuilder WithSignedAttributeGenerator(CmsAttributeTableGenerator signedGen)
	{
		this.signedGen = signedGen;
		return this;
	}

	public SignerInfoGeneratorBuilder WithUnsignedAttributeGenerator(CmsAttributeTableGenerator unsignedGen)
	{
		this.unsignedGen = unsignedGen;
		return this;
	}

	public SignerInfoGenerator Build(ISignatureFactory contentSigner, X509Certificate certificate)
	{
		SignerIdentifier sigId = new SignerIdentifier(new IssuerAndSerialNumber(certificate.IssuerDN, new DerInteger(certificate.SerialNumber)));
		SignerInfoGenerator signerInfoGenerator = CreateGenerator(contentSigner, sigId);
		signerInfoGenerator.setAssociatedCertificate(certificate);
		return signerInfoGenerator;
	}

	public SignerInfoGenerator Build(ISignatureFactory signerFactory, byte[] subjectKeyIdentifier)
	{
		SignerIdentifier sigId = new SignerIdentifier(new DerOctetString(subjectKeyIdentifier));
		return CreateGenerator(signerFactory, sigId);
	}

	private SignerInfoGenerator CreateGenerator(ISignatureFactory contentSigner, SignerIdentifier sigId)
	{
		if (directSignature)
		{
			return new SignerInfoGenerator(sigId, contentSigner, isDirectSignature: true);
		}
		if (signedGen != null || unsignedGen != null)
		{
			if (signedGen == null)
			{
				signedGen = new DefaultSignedAttributeTableGenerator();
			}
			return new SignerInfoGenerator(sigId, contentSigner, signedGen, unsignedGen);
		}
		return new SignerInfoGenerator(sigId, contentSigner);
	}
}
