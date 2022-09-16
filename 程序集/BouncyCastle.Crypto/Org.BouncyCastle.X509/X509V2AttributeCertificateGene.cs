using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509;

public class X509V2AttributeCertificateGenerator
{
	private readonly X509ExtensionsGenerator extGenerator = new X509ExtensionsGenerator();

	private V2AttributeCertificateInfoGenerator acInfoGen;

	private DerObjectIdentifier sigOID;

	private AlgorithmIdentifier sigAlgId;

	private string signatureAlgorithm;

	public IEnumerable SignatureAlgNames => X509Utilities.GetAlgNames();

	public X509V2AttributeCertificateGenerator()
	{
		acInfoGen = new V2AttributeCertificateInfoGenerator();
	}

	public void Reset()
	{
		acInfoGen = new V2AttributeCertificateInfoGenerator();
		extGenerator.Reset();
	}

	public void SetHolder(AttributeCertificateHolder holder)
	{
		acInfoGen.SetHolder(holder.holder);
	}

	public void SetIssuer(AttributeCertificateIssuer issuer)
	{
		acInfoGen.SetIssuer(AttCertIssuer.GetInstance(issuer.form));
	}

	public void SetSerialNumber(BigInteger serialNumber)
	{
		acInfoGen.SetSerialNumber(new DerInteger(serialNumber));
	}

	public void SetNotBefore(DateTime date)
	{
		acInfoGen.SetStartDate(new DerGeneralizedTime(date));
	}

	public void SetNotAfter(DateTime date)
	{
		acInfoGen.SetEndDate(new DerGeneralizedTime(date));
	}

	[Obsolete("Not needed if Generate used with an ISignatureFactory")]
	public void SetSignatureAlgorithm(string signatureAlgorithm)
	{
		this.signatureAlgorithm = signatureAlgorithm;
		try
		{
			sigOID = X509Utilities.GetAlgorithmOid(signatureAlgorithm);
		}
		catch (Exception)
		{
			throw new ArgumentException("Unknown signature type requested");
		}
		sigAlgId = X509Utilities.GetSigAlgID(sigOID, signatureAlgorithm);
		acInfoGen.SetSignature(sigAlgId);
	}

	public void AddAttribute(X509Attribute attribute)
	{
		acInfoGen.AddAttribute(AttributeX509.GetInstance(attribute.ToAsn1Object()));
	}

	public void SetIssuerUniqueId(bool[] iui)
	{
		throw Platform.CreateNotImplementedException("SetIssuerUniqueId()");
	}

	public void AddExtension(string oid, bool critical, Asn1Encodable extensionValue)
	{
		extGenerator.AddExtension(new DerObjectIdentifier(oid), critical, extensionValue);
	}

	public void AddExtension(string oid, bool critical, byte[] extensionValue)
	{
		extGenerator.AddExtension(new DerObjectIdentifier(oid), critical, extensionValue);
	}

	[Obsolete("Use Generate with an ISignatureFactory")]
	public IX509AttributeCertificate Generate(AsymmetricKeyParameter privateKey)
	{
		return Generate(privateKey, null);
	}

	[Obsolete("Use Generate with an ISignatureFactory")]
	public IX509AttributeCertificate Generate(AsymmetricKeyParameter privateKey, SecureRandom random)
	{
		return Generate(new Asn1SignatureFactory(signatureAlgorithm, privateKey, random));
	}

	public IX509AttributeCertificate Generate(ISignatureFactory signatureCalculatorFactory)
	{
		if (!extGenerator.IsEmpty)
		{
			acInfoGen.SetExtensions(extGenerator.Generate());
		}
		AlgorithmIdentifier signature = (AlgorithmIdentifier)signatureCalculatorFactory.AlgorithmDetails;
		acInfoGen.SetSignature(signature);
		AttributeCertificateInfo attributeCertificateInfo = acInfoGen.GenerateAttributeCertificateInfo();
		byte[] derEncoded = attributeCertificateInfo.GetDerEncoded();
		IStreamCalculator streamCalculator = signatureCalculatorFactory.CreateCalculator();
		streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		Platform.Dispose(streamCalculator.Stream);
		try
		{
			DerBitString signatureValue = new DerBitString(((IBlockResult)streamCalculator.GetResult()).Collect());
			return new X509V2AttributeCertificate(new AttributeCertificate(attributeCertificateInfo, signature, signatureValue));
		}
		catch (Exception e)
		{
			throw new CertificateEncodingException("constructed invalid certificate", e);
		}
	}
}
