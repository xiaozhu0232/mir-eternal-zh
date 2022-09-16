using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Ess;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Tsp;

public class TimeStampToken
{
	private class CertID
	{
		private EssCertID certID;

		private EssCertIDv2 certIDv2;

		public IssuerSerial IssuerSerial
		{
			get
			{
				if (certID == null)
				{
					return certIDv2.IssuerSerial;
				}
				return certID.IssuerSerial;
			}
		}

		internal CertID(EssCertID certID)
		{
			this.certID = certID;
			certIDv2 = null;
		}

		internal CertID(EssCertIDv2 certID)
		{
			certIDv2 = certID;
			this.certID = null;
		}

		public string GetHashAlgorithmName()
		{
			if (certID != null)
			{
				return "SHA-1";
			}
			if (NistObjectIdentifiers.IdSha256.Equals(certIDv2.HashAlgorithm.Algorithm))
			{
				return "SHA-256";
			}
			return certIDv2.HashAlgorithm.Algorithm.Id;
		}

		public AlgorithmIdentifier GetHashAlgorithm()
		{
			if (certID == null)
			{
				return certIDv2.HashAlgorithm;
			}
			return new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1);
		}

		public byte[] GetCertHash()
		{
			if (certID == null)
			{
				return certIDv2.GetCertHash();
			}
			return certID.GetCertHash();
		}
	}

	private readonly CmsSignedData tsToken;

	private readonly SignerInformation tsaSignerInfo;

	private readonly TimeStampTokenInfo tstInfo;

	private readonly CertID certID;

	public TimeStampTokenInfo TimeStampInfo => tstInfo;

	public SignerID SignerID => tsaSignerInfo.SignerID;

	public Org.BouncyCastle.Asn1.Cms.AttributeTable SignedAttributes => tsaSignerInfo.SignedAttributes;

	public Org.BouncyCastle.Asn1.Cms.AttributeTable UnsignedAttributes => tsaSignerInfo.UnsignedAttributes;

	public TimeStampToken(Org.BouncyCastle.Asn1.Cms.ContentInfo contentInfo)
		: this(new CmsSignedData(contentInfo))
	{
	}

	public TimeStampToken(CmsSignedData signedData)
	{
		tsToken = signedData;
		if (!tsToken.SignedContentType.Equals(PkcsObjectIdentifiers.IdCTTstInfo))
		{
			throw new TspValidationException("ContentInfo object not for a time stamp.");
		}
		ICollection signers = tsToken.GetSignerInfos().GetSigners();
		if (signers.Count != 1)
		{
			throw new ArgumentException("Time-stamp token signed by " + signers.Count + " signers, but it must contain just the TSA signature.");
		}
		IEnumerator enumerator = signers.GetEnumerator();
		enumerator.MoveNext();
		tsaSignerInfo = (SignerInformation)enumerator.Current;
		try
		{
			CmsProcessable signedContent = tsToken.SignedContent;
			MemoryStream memoryStream = new MemoryStream();
			signedContent.Write(memoryStream);
			tstInfo = new TimeStampTokenInfo(TstInfo.GetInstance(Asn1Object.FromByteArray(memoryStream.ToArray())));
			Org.BouncyCastle.Asn1.Cms.Attribute attribute = tsaSignerInfo.SignedAttributes[PkcsObjectIdentifiers.IdAASigningCertificate];
			if (attribute != null)
			{
				if (attribute.AttrValues[0] is SigningCertificateV2)
				{
					SigningCertificateV2 instance = SigningCertificateV2.GetInstance(attribute.AttrValues[0]);
					certID = new CertID(EssCertIDv2.GetInstance(instance.GetCerts()[0]));
				}
				else
				{
					SigningCertificate instance2 = SigningCertificate.GetInstance(attribute.AttrValues[0]);
					certID = new CertID(EssCertID.GetInstance(instance2.GetCerts()[0]));
				}
				return;
			}
			attribute = tsaSignerInfo.SignedAttributes[PkcsObjectIdentifiers.IdAASigningCertificateV2];
			if (attribute == null)
			{
				throw new TspValidationException("no signing certificate attribute found, time stamp invalid.");
			}
			SigningCertificateV2 instance3 = SigningCertificateV2.GetInstance(attribute.AttrValues[0]);
			certID = new CertID(EssCertIDv2.GetInstance(instance3.GetCerts()[0]));
		}
		catch (CmsException ex)
		{
			throw new TspException(ex.Message, ex.InnerException);
		}
	}

	public IX509Store GetCertificates(string type)
	{
		return tsToken.GetCertificates(type);
	}

	public IX509Store GetCrls(string type)
	{
		return tsToken.GetCrls(type);
	}

	public IX509Store GetCertificates()
	{
		return tsToken.GetCertificates();
	}

	public IX509Store GetAttributeCertificates(string type)
	{
		return tsToken.GetAttributeCertificates(type);
	}

	public void Validate(X509Certificate cert)
	{
		try
		{
			byte[] b = DigestUtilities.CalculateDigest(certID.GetHashAlgorithmName(), cert.GetEncoded());
			if (!Arrays.ConstantTimeAreEqual(certID.GetCertHash(), b))
			{
				throw new TspValidationException("certificate hash does not match certID hash.");
			}
			if (certID.IssuerSerial != null)
			{
				if (!certID.IssuerSerial.Serial.Value.Equals(cert.SerialNumber))
				{
					throw new TspValidationException("certificate serial number does not match certID for signature.");
				}
				GeneralName[] names = certID.IssuerSerial.Issuer.GetNames();
				X509Name issuerX509Principal = PrincipalUtilities.GetIssuerX509Principal(cert);
				bool flag = false;
				for (int i = 0; i != names.Length; i++)
				{
					if (names[i].TagNo == 4 && X509Name.GetInstance(names[i].Name).Equivalent(issuerX509Principal))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					throw new TspValidationException("certificate name does not match certID for signature. ");
				}
			}
			TspUtil.ValidateCertificate(cert);
			cert.CheckValidity(tstInfo.GenTime);
			if (!tsaSignerInfo.Verify(cert))
			{
				throw new TspValidationException("signature not created by certificate.");
			}
		}
		catch (CmsException ex)
		{
			if (ex.InnerException != null)
			{
				throw new TspException(ex.Message, ex.InnerException);
			}
			throw new TspException("CMS exception: " + ex, ex);
		}
		catch (CertificateEncodingException ex2)
		{
			throw new TspException("problem processing certificate: " + ex2, ex2);
		}
		catch (SecurityUtilityException ex3)
		{
			throw new TspException("cannot find algorithm: " + ex3.Message, ex3);
		}
	}

	public CmsSignedData ToCmsSignedData()
	{
		return tsToken;
	}

	public byte[] GetEncoded()
	{
		return tsToken.GetEncoded("DER");
	}

	public byte[] GetEncoded(string encoding)
	{
		return tsToken.GetEncoded(encoding);
	}
}
