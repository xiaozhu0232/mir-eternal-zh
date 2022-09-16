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
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509;

public class X509V3CertificateGenerator
{
	private readonly X509ExtensionsGenerator extGenerator = new X509ExtensionsGenerator();

	private V3TbsCertificateGenerator tbsGen;

	private DerObjectIdentifier sigOid;

	private AlgorithmIdentifier sigAlgId;

	private string signatureAlgorithm;

	public IEnumerable SignatureAlgNames => X509Utilities.GetAlgNames();

	public X509V3CertificateGenerator()
	{
		tbsGen = new V3TbsCertificateGenerator();
	}

	public void Reset()
	{
		tbsGen = new V3TbsCertificateGenerator();
		extGenerator.Reset();
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
		tbsGen.SetSubjectPublicKeyInfo(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey));
	}

	[Obsolete("Not needed if Generate used with an ISignatureFactory")]
	public void SetSignatureAlgorithm(string signatureAlgorithm)
	{
		this.signatureAlgorithm = signatureAlgorithm;
		try
		{
			sigOid = X509Utilities.GetAlgorithmOid(signatureAlgorithm);
		}
		catch (Exception)
		{
			throw new ArgumentException("Unknown signature type requested: " + signatureAlgorithm);
		}
		sigAlgId = X509Utilities.GetSigAlgID(sigOid, signatureAlgorithm);
		tbsGen.SetSignature(sigAlgId);
	}

	public void SetSubjectUniqueID(bool[] uniqueID)
	{
		tbsGen.SetSubjectUniqueID(booleanToBitString(uniqueID));
	}

	public void SetIssuerUniqueID(bool[] uniqueID)
	{
		tbsGen.SetIssuerUniqueID(booleanToBitString(uniqueID));
	}

	private DerBitString booleanToBitString(bool[] id)
	{
		byte[] array = new byte[(id.Length + 7) / 8];
		for (int i = 0; i != id.Length; i++)
		{
			if (id[i])
			{
				byte[] array2;
				byte[] array3 = (array2 = array);
				int num = i / 8;
				nint num2 = num;
				array3[num] = (byte)(array2[num2] | (byte)(1 << 7 - i % 8));
			}
		}
		int num3 = id.Length % 8;
		if (num3 == 0)
		{
			return new DerBitString(array);
		}
		return new DerBitString(array, 8 - num3);
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

	public void CopyAndAddExtension(string oid, bool critical, X509Certificate cert)
	{
		CopyAndAddExtension(new DerObjectIdentifier(oid), critical, cert);
	}

	public void CopyAndAddExtension(DerObjectIdentifier oid, bool critical, X509Certificate cert)
	{
		Asn1OctetString extensionValue = cert.GetExtensionValue(oid);
		if (extensionValue == null)
		{
			throw new CertificateParsingException(string.Concat("extension ", oid, " not present"));
		}
		try
		{
			Asn1Encodable extensionValue2 = X509ExtensionUtilities.FromExtensionValue(extensionValue);
			AddExtension(oid, critical, extensionValue2);
		}
		catch (Exception ex)
		{
			throw new CertificateParsingException(ex.Message, ex);
		}
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
		if (!extGenerator.IsEmpty)
		{
			tbsGen.SetExtensions(extGenerator.Generate());
		}
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
