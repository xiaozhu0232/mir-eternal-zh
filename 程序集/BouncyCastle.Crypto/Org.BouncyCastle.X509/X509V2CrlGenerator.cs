using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.X509;

public class X509V2CrlGenerator
{
	private readonly X509ExtensionsGenerator extGenerator = new X509ExtensionsGenerator();

	private V2TbsCertListGenerator tbsGen;

	private DerObjectIdentifier sigOID;

	private AlgorithmIdentifier sigAlgId;

	private string signatureAlgorithm;

	public IEnumerable SignatureAlgNames => X509Utilities.GetAlgNames();

	public X509V2CrlGenerator()
	{
		tbsGen = new V2TbsCertListGenerator();
	}

	public void Reset()
	{
		tbsGen = new V2TbsCertListGenerator();
		extGenerator.Reset();
	}

	public void SetIssuerDN(X509Name issuer)
	{
		tbsGen.SetIssuer(issuer);
	}

	public void SetThisUpdate(DateTime date)
	{
		tbsGen.SetThisUpdate(new Time(date));
	}

	public void SetNextUpdate(DateTime date)
	{
		tbsGen.SetNextUpdate(new Time(date));
	}

	public void AddCrlEntry(BigInteger userCertificate, DateTime revocationDate, int reason)
	{
		tbsGen.AddCrlEntry(new DerInteger(userCertificate), new Time(revocationDate), reason);
	}

	public void AddCrlEntry(BigInteger userCertificate, DateTime revocationDate, int reason, DateTime invalidityDate)
	{
		tbsGen.AddCrlEntry(new DerInteger(userCertificate), new Time(revocationDate), reason, new DerGeneralizedTime(invalidityDate));
	}

	public void AddCrlEntry(BigInteger userCertificate, DateTime revocationDate, X509Extensions extensions)
	{
		tbsGen.AddCrlEntry(new DerInteger(userCertificate), new Time(revocationDate), extensions);
	}

	public void AddCrl(X509Crl other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		ISet revokedCertificates = other.GetRevokedCertificates();
		if (revokedCertificates == null)
		{
			return;
		}
		foreach (X509CrlEntry item in revokedCertificates)
		{
			try
			{
				tbsGen.AddCrlEntry(Asn1Sequence.GetInstance(Asn1Object.FromByteArray(item.GetEncoded())));
			}
			catch (IOException e)
			{
				throw new CrlException("exception processing encoding of CRL", e);
			}
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
		catch (Exception innerException)
		{
			throw new ArgumentException("Unknown signature type requested", innerException);
		}
		sigAlgId = X509Utilities.GetSigAlgID(sigOID, signatureAlgorithm);
		tbsGen.SetSignature(sigAlgId);
	}

	public void AddExtension(string oid, bool critical, Asn1Encodable extensionValue)
	{
		extGenerator.AddExtension(new DerObjectIdentifier(oid), critical, extensionValue);
	}

	public void AddExtension(DerObjectIdentifier oid, bool critical, Asn1Encodable extensionValue)
	{
		extGenerator.AddExtension(oid, critical, extensionValue);
	}

	public void AddExtension(string oid, bool critical, byte[] extensionValue)
	{
		extGenerator.AddExtension(new DerObjectIdentifier(oid), critical, new DerOctetString(extensionValue));
	}

	public void AddExtension(DerObjectIdentifier oid, bool critical, byte[] extensionValue)
	{
		extGenerator.AddExtension(oid, critical, new DerOctetString(extensionValue));
	}

	[Obsolete("Use Generate with an ISignatureFactory")]
	public X509Crl Generate(AsymmetricKeyParameter privateKey)
	{
		return Generate(privateKey, null);
	}

	[Obsolete("Use Generate with an ISignatureFactory")]
	public X509Crl Generate(AsymmetricKeyParameter privateKey, SecureRandom random)
	{
		return Generate(new Asn1SignatureFactory(signatureAlgorithm, privateKey, random));
	}

	public X509Crl Generate(ISignatureFactory signatureCalculatorFactory)
	{
		tbsGen.SetSignature((AlgorithmIdentifier)signatureCalculatorFactory.AlgorithmDetails);
		TbsCertificateList tbsCertificateList = GenerateCertList();
		IStreamCalculator streamCalculator = signatureCalculatorFactory.CreateCalculator();
		byte[] derEncoded = tbsCertificateList.GetDerEncoded();
		streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		Platform.Dispose(streamCalculator.Stream);
		return GenerateJcaObject(tbsCertificateList, (AlgorithmIdentifier)signatureCalculatorFactory.AlgorithmDetails, ((IBlockResult)streamCalculator.GetResult()).Collect());
	}

	private TbsCertificateList GenerateCertList()
	{
		if (!extGenerator.IsEmpty)
		{
			tbsGen.SetExtensions(extGenerator.Generate());
		}
		return tbsGen.GenerateTbsCertList();
	}

	private X509Crl GenerateJcaObject(TbsCertificateList tbsCrl, AlgorithmIdentifier algId, byte[] signature)
	{
		return new X509Crl(CertificateList.GetInstance(new DerSequence(tbsCrl, algId, new DerBitString(signature))));
	}
}
