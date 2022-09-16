using System;
using System.Collections;
using System.IO;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Ess;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Tsp;

public class TimeStampTokenGenerator
{
	private class TableGen : CmsAttributeTableGenerator
	{
		private readonly SignerInfoGenerator infoGen;

		private readonly EssCertID essCertID;

		public TableGen(SignerInfoGenerator infoGen, EssCertID essCertID)
		{
			this.infoGen = infoGen;
			this.essCertID = essCertID;
		}

		public Org.BouncyCastle.Asn1.Cms.AttributeTable GetAttributes(IDictionary parameters)
		{
			Org.BouncyCastle.Asn1.Cms.AttributeTable attributes = infoGen.signedGen.GetAttributes(parameters);
			if (attributes[PkcsObjectIdentifiers.IdAASigningCertificate] == null)
			{
				return attributes.Add(PkcsObjectIdentifiers.IdAASigningCertificate, new SigningCertificate(essCertID));
			}
			return attributes;
		}
	}

	private class TableGen2 : CmsAttributeTableGenerator
	{
		private readonly SignerInfoGenerator infoGen;

		private readonly EssCertIDv2 essCertID;

		public TableGen2(SignerInfoGenerator infoGen, EssCertIDv2 essCertID)
		{
			this.infoGen = infoGen;
			this.essCertID = essCertID;
		}

		public Org.BouncyCastle.Asn1.Cms.AttributeTable GetAttributes(IDictionary parameters)
		{
			Org.BouncyCastle.Asn1.Cms.AttributeTable attributes = infoGen.signedGen.GetAttributes(parameters);
			if (attributes[PkcsObjectIdentifiers.IdAASigningCertificateV2] == null)
			{
				return attributes.Add(PkcsObjectIdentifiers.IdAASigningCertificateV2, new SigningCertificateV2(essCertID));
			}
			return attributes;
		}
	}

	private int accuracySeconds = -1;

	private int accuracyMillis = -1;

	private int accuracyMicros = -1;

	private bool ordering = false;

	private GeneralName tsa = null;

	private string tsaPolicyOID;

	private IX509Store x509Certs;

	private IX509Store x509Crls;

	private SignerInfoGenerator signerInfoGenerator;

	private IDigestFactory digestCalculator;

	private Resolution resolution = Resolution.R_SECONDS;

	public Resolution Resolution
	{
		get
		{
			return resolution;
		}
		set
		{
			resolution = value;
		}
	}

	public TimeStampTokenGenerator(AsymmetricKeyParameter key, X509Certificate cert, string digestOID, string tsaPolicyOID)
		: this(key, cert, digestOID, tsaPolicyOID, null, null)
	{
	}

	public TimeStampTokenGenerator(SignerInfoGenerator signerInfoGen, IDigestFactory digestCalculator, DerObjectIdentifier tsaPolicy, bool isIssuerSerialIncluded)
	{
		signerInfoGenerator = signerInfoGen;
		this.digestCalculator = digestCalculator;
		tsaPolicyOID = tsaPolicy.Id;
		if (signerInfoGenerator.certificate == null)
		{
			throw new ArgumentException("SignerInfoGenerator must have an associated certificate");
		}
		X509Certificate certificate = signerInfoGenerator.certificate;
		TspUtil.ValidateCertificate(certificate);
		try
		{
			IStreamCalculator streamCalculator = digestCalculator.CreateCalculator();
			Stream stream = streamCalculator.Stream;
			byte[] encoded = certificate.GetEncoded();
			stream.Write(encoded, 0, encoded.Length);
			stream.Flush();
			stream.Close();
			if (((AlgorithmIdentifier)digestCalculator.AlgorithmDetails).Algorithm.Equals(OiwObjectIdentifiers.IdSha1))
			{
				EssCertID essCertID = new EssCertID(((IBlockResult)streamCalculator.GetResult()).Collect(), isIssuerSerialIncluded ? new IssuerSerial(new GeneralNames(new GeneralName(certificate.IssuerDN)), new DerInteger(certificate.SerialNumber)) : null);
				signerInfoGenerator = signerInfoGen.NewBuilder().WithSignedAttributeGenerator(new TableGen(signerInfoGen, essCertID)).Build(signerInfoGen.contentSigner, signerInfoGen.certificate);
			}
			else
			{
				new AlgorithmIdentifier(((AlgorithmIdentifier)digestCalculator.AlgorithmDetails).Algorithm);
				EssCertIDv2 essCertID2 = new EssCertIDv2(((IBlockResult)streamCalculator.GetResult()).Collect(), isIssuerSerialIncluded ? new IssuerSerial(new GeneralNames(new GeneralName(certificate.IssuerDN)), new DerInteger(certificate.SerialNumber)) : null);
				signerInfoGenerator = signerInfoGen.NewBuilder().WithSignedAttributeGenerator(new TableGen2(signerInfoGen, essCertID2)).Build(signerInfoGen.contentSigner, signerInfoGen.certificate);
			}
		}
		catch (Exception e)
		{
			throw new TspException("Exception processing certificate", e);
		}
	}

	public TimeStampTokenGenerator(AsymmetricKeyParameter key, X509Certificate cert, string digestOID, string tsaPolicyOID, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
		: this(makeInfoGenerator(key, cert, digestOID, signedAttr, unsignedAttr), Asn1DigestFactory.Get(OiwObjectIdentifiers.IdSha1), (tsaPolicyOID != null) ? new DerObjectIdentifier(tsaPolicyOID) : null, isIssuerSerialIncluded: false)
	{
		this.tsaPolicyOID = tsaPolicyOID;
	}

	internal static SignerInfoGenerator makeInfoGenerator(AsymmetricKeyParameter key, X509Certificate cert, string digestOID, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
	{
		TspUtil.ValidateCertificate(cert);
		IDictionary attrs = ((signedAttr == null) ? Platform.CreateHashtable() : signedAttr.ToDictionary());
		string digestAlgName = CmsSignedHelper.Instance.GetDigestAlgName(digestOID);
		string algorithm = digestAlgName + "with" + CmsSignedHelper.Instance.GetEncryptionAlgName(CmsSignedHelper.Instance.GetEncOid(key, digestOID));
		Asn1SignatureFactory contentSigner = new Asn1SignatureFactory(algorithm, key);
		return new SignerInfoGeneratorBuilder().WithSignedAttributeGenerator(new DefaultSignedAttributeTableGenerator(new Org.BouncyCastle.Asn1.Cms.AttributeTable(attrs))).WithUnsignedAttributeGenerator(new SimpleAttributeTableGenerator(unsignedAttr)).Build(contentSigner, cert);
	}

	public void SetCertificates(IX509Store certificates)
	{
		x509Certs = certificates;
	}

	public void SetCrls(IX509Store crls)
	{
		x509Crls = crls;
	}

	public void SetAccuracySeconds(int accuracySeconds)
	{
		this.accuracySeconds = accuracySeconds;
	}

	public void SetAccuracyMillis(int accuracyMillis)
	{
		this.accuracyMillis = accuracyMillis;
	}

	public void SetAccuracyMicros(int accuracyMicros)
	{
		this.accuracyMicros = accuracyMicros;
	}

	public void SetOrdering(bool ordering)
	{
		this.ordering = ordering;
	}

	public void SetTsa(GeneralName tsa)
	{
		this.tsa = tsa;
	}

	public TimeStampToken Generate(TimeStampRequest request, BigInteger serialNumber, DateTime genTime)
	{
		return Generate(request, serialNumber, genTime, null);
	}

	public TimeStampToken Generate(TimeStampRequest request, BigInteger serialNumber, DateTime genTime, X509Extensions additionalExtensions)
	{
		DerObjectIdentifier algorithm = new DerObjectIdentifier(request.MessageImprintAlgOid);
		AlgorithmIdentifier hashAlgorithm = new AlgorithmIdentifier(algorithm, DerNull.Instance);
		MessageImprint messageImprint = new MessageImprint(hashAlgorithm, request.GetMessageImprintDigest());
		Accuracy accuracy = null;
		if (accuracySeconds > 0 || accuracyMillis > 0 || accuracyMicros > 0)
		{
			DerInteger seconds = null;
			if (accuracySeconds > 0)
			{
				seconds = new DerInteger(accuracySeconds);
			}
			DerInteger millis = null;
			if (accuracyMillis > 0)
			{
				millis = new DerInteger(accuracyMillis);
			}
			DerInteger micros = null;
			if (accuracyMicros > 0)
			{
				micros = new DerInteger(accuracyMicros);
			}
			accuracy = new Accuracy(seconds, millis, micros);
		}
		DerBoolean derBoolean = null;
		if (ordering)
		{
			derBoolean = DerBoolean.GetInstance(ordering);
		}
		DerInteger nonce = null;
		if (request.Nonce != null)
		{
			nonce = new DerInteger(request.Nonce);
		}
		DerObjectIdentifier tsaPolicyId = new DerObjectIdentifier(tsaPolicyOID);
		if (request.ReqPolicy != null)
		{
			tsaPolicyId = new DerObjectIdentifier(request.ReqPolicy);
		}
		X509Extensions x509Extensions = request.Extensions;
		if (additionalExtensions != null)
		{
			X509ExtensionsGenerator x509ExtensionsGenerator = new X509ExtensionsGenerator();
			if (x509Extensions != null)
			{
				foreach (object extensionOid in x509Extensions.ExtensionOids)
				{
					DerObjectIdentifier instance = DerObjectIdentifier.GetInstance(extensionOid);
					x509ExtensionsGenerator.AddExtension(instance, x509Extensions.GetExtension(DerObjectIdentifier.GetInstance(instance)));
				}
			}
			foreach (object extensionOid2 in additionalExtensions.ExtensionOids)
			{
				DerObjectIdentifier instance2 = DerObjectIdentifier.GetInstance(extensionOid2);
				x509ExtensionsGenerator.AddExtension(instance2, additionalExtensions.GetExtension(DerObjectIdentifier.GetInstance(instance2)));
			}
			x509Extensions = x509ExtensionsGenerator.Generate();
		}
		TstInfo tstInfo = new TstInfo(genTime: (resolution == Resolution.R_SECONDS) ? new DerGeneralizedTime(genTime) : new DerGeneralizedTime(createGeneralizedTime(genTime)), tsaPolicyId: tsaPolicyId, messageImprint: messageImprint, serialNumber: new DerInteger(serialNumber), accuracy: accuracy, ordering: derBoolean, nonce: nonce, tsa: tsa, extensions: x509Extensions);
		try
		{
			CmsSignedDataGenerator cmsSignedDataGenerator = new CmsSignedDataGenerator();
			byte[] derEncoded = tstInfo.GetDerEncoded();
			if (request.CertReq)
			{
				cmsSignedDataGenerator.AddCertificates(x509Certs);
			}
			cmsSignedDataGenerator.AddCrls(x509Crls);
			cmsSignedDataGenerator.AddSignerInfoGenerator(signerInfoGenerator);
			CmsSignedData signedData = cmsSignedDataGenerator.Generate(PkcsObjectIdentifiers.IdCTTstInfo.Id, new CmsProcessableByteArray(derEncoded), encapsulate: true);
			return new TimeStampToken(signedData);
		}
		catch (CmsException e)
		{
			throw new TspException("Error generating time-stamp token", e);
		}
		catch (IOException e2)
		{
			throw new TspException("Exception encoding info", e2);
		}
		catch (X509StoreException e3)
		{
			throw new TspException("Exception handling CertStore", e3);
		}
	}

	private string createGeneralizedTime(DateTime genTime)
	{
		string text = "yyyyMMddHHmmss.fff";
		StringBuilder stringBuilder = new StringBuilder(genTime.ToString(text));
		int num = stringBuilder.ToString().IndexOf(".");
		if (num < 0)
		{
			stringBuilder.Append("Z");
			return stringBuilder.ToString();
		}
		switch (resolution)
		{
		case Resolution.R_TENTHS_OF_SECONDS:
			if (stringBuilder.Length > num + 2)
			{
				stringBuilder.Remove(num + 2, stringBuilder.Length - (num + 2));
			}
			break;
		case Resolution.R_HUNDREDTHS_OF_SECONDS:
			if (stringBuilder.Length > num + 3)
			{
				stringBuilder.Remove(num + 3, stringBuilder.Length - (num + 3));
			}
			break;
		}
		while (stringBuilder[stringBuilder.Length - 1] == '0')
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		if (stringBuilder.Length - 1 == num)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		stringBuilder.Append("Z");
		return stringBuilder.ToString();
	}
}
