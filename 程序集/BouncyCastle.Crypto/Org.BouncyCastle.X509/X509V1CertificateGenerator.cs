using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509;

public class X509V1CertificateGenerator
{
	private V1TbsCertificateGenerator tbsGen;

	private DerObjectIdentifier sigOID;

	private AlgorithmIdentifier sigAlgId;

	private string signatureAlgorithm;

	public IEnumerable SignatureAlgNames => X509Utilities.GetAlgNames();

	public X509V1CertificateGenerator()
	{
		tbsGen = new V1TbsCertificateGenerator();
	}

	public void Reset()
	{
		tbsGen = new V1TbsCertificateGenerator();
	}

	public void SetSerialNumber(BigInteger serialNumber)
	{
		if (serialNumber.SignValue <= 0)
		{
			throw new ArgumentException("serial number must be a positive integer", "serialNumber");
		}
		tbsGen.SetSerialNumber(new DerInteger(serialNumber));
	}

	public void SetIssuerDN(X509Name issuer)
	{
		tbsGen.SetIssuer(issuer);
	}

	public void SetNotBefore(DateTime date)
	{
		tbsGen.SetStartDate(new Time(date));
	}

	public void SetNotAfter(DateTime date)
	{
		tbsGen.SetEndDate(new Time(date));
	}

	public void SetSubjectDN(X509Name subject)
	{
		tbsGen.SetSubject(subject);
	}

	public void SetPublicKey(AsymmetricKeyParameter publicKey)
	{
		try
		{
			tbsGen.SetSubjectPublicKeyInfo(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey));
		}
		catch (Exception ex)
		{
			throw new ArgumentException("unable to process key - " + ex.ToString());
		}
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
			throw new ArgumentException("Unknown signature type requested", "signatureAlgorithm");
		}
		sigAlgId = X509Utilities.GetSigAlgID(sigOID, signatureAlgorithm);
		tbsGen.SetSignature(sigAlgId);
	}

	[Obsolete("Use Generate with an ISignatureFactory")]
	public X509Certificate Generate(AsymmetricKeyParameter privateKey)
	{
		return Generate(privateKey, null);
	}

	[Obsolete("Use Generate with an ISignatureFactory")]
	public X509Certificate Generate(AsymmetricKeyParameter privateKey, SecureRandom random)
	{
		return Generate(new Asn1SignatureFactory(signatureAlgorithm, privateKey, random));
	}

	public X509Certificate Generate(ISignatureFactory signatureCalculatorFactory)
	{
		tbsGen.SetSignature((AlgorithmIdentifier)signatureCalculatorFactory.AlgorithmDetails);
		TbsCertificateStructure tbsCertificateStructure = tbsGen.GenerateTbsCertificate();
		IStreamCalculator streamCalculator = signatureCalculatorFactory.CreateCalculator();
		byte[] derEncoded = tbsCertificateStructure.GetDerEncoded();
		streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		Platform.Dispose(streamCalculator.Stream);
		return GenerateJcaObject(tbsCertificateStructure, (AlgorithmIdentifier)signatureCalculatorFactory.AlgorithmDetails, ((IBlockResult)streamCalculator.GetResult()).Collect());
	}

	private X509Certificate GenerateJcaObject(TbsCertificateStructure tbsCert, AlgorithmIdentifier sigAlg, byte[] signature)
	{
		return new X509Certificate(new X509CertificateStructure(tbsCert, sigAlg, new DerBitString(signature)));
	}
}
