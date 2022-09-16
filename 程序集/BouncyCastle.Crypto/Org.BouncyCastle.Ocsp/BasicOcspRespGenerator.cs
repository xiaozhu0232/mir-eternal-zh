using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Ocsp;

public class BasicOcspRespGenerator
{
	private class ResponseObject
	{
		internal CertificateID certId;

		internal CertStatus certStatus;

		internal DerGeneralizedTime thisUpdate;

		internal DerGeneralizedTime nextUpdate;

		internal X509Extensions extensions;

		public ResponseObject(CertificateID certId, CertificateStatus certStatus, DateTime thisUpdate, X509Extensions extensions)
			: this(certId, certStatus, new DerGeneralizedTime(thisUpdate), null, extensions)
		{
		}

		public ResponseObject(CertificateID certId, CertificateStatus certStatus, DateTime thisUpdate, DateTime nextUpdate, X509Extensions extensions)
			: this(certId, certStatus, new DerGeneralizedTime(thisUpdate), new DerGeneralizedTime(nextUpdate), extensions)
		{
		}

		private ResponseObject(CertificateID certId, CertificateStatus certStatus, DerGeneralizedTime thisUpdate, DerGeneralizedTime nextUpdate, X509Extensions extensions)
		{
			this.certId = certId;
			if (certStatus == null)
			{
				this.certStatus = new CertStatus();
			}
			else if (certStatus is UnknownStatus)
			{
				this.certStatus = new CertStatus(2, DerNull.Instance);
			}
			else
			{
				RevokedStatus revokedStatus = (RevokedStatus)certStatus;
				CrlReason revocationReason = (revokedStatus.HasRevocationReason ? new CrlReason(revokedStatus.RevocationReason) : null);
				this.certStatus = new CertStatus(new RevokedInfo(new DerGeneralizedTime(revokedStatus.RevocationTime), revocationReason));
			}
			this.thisUpdate = thisUpdate;
			this.nextUpdate = nextUpdate;
			this.extensions = extensions;
		}

		public SingleResponse ToResponse()
		{
			return new SingleResponse(certId.ToAsn1Object(), certStatus, thisUpdate, nextUpdate, extensions);
		}
	}

	private readonly IList list = Platform.CreateArrayList();

	private X509Extensions responseExtensions;

	private RespID responderID;

	public IEnumerable SignatureAlgNames => OcspUtilities.AlgNames;

	public BasicOcspRespGenerator(RespID responderID)
	{
		this.responderID = responderID;
	}

	public BasicOcspRespGenerator(AsymmetricKeyParameter publicKey)
	{
		responderID = new RespID(publicKey);
	}

	public void AddResponse(CertificateID certID, CertificateStatus certStatus)
	{
		list.Add(new ResponseObject(certID, certStatus, DateTime.UtcNow, null));
	}

	public void AddResponse(CertificateID certID, CertificateStatus certStatus, X509Extensions singleExtensions)
	{
		list.Add(new ResponseObject(certID, certStatus, DateTime.UtcNow, singleExtensions));
	}

	public void AddResponse(CertificateID certID, CertificateStatus certStatus, DateTime nextUpdate, X509Extensions singleExtensions)
	{
		list.Add(new ResponseObject(certID, certStatus, DateTime.UtcNow, nextUpdate, singleExtensions));
	}

	public void AddResponse(CertificateID certID, CertificateStatus certStatus, DateTime thisUpdate, DateTime nextUpdate, X509Extensions singleExtensions)
	{
		list.Add(new ResponseObject(certID, certStatus, thisUpdate, nextUpdate, singleExtensions));
	}

	public void SetResponseExtensions(X509Extensions responseExtensions)
	{
		this.responseExtensions = responseExtensions;
	}

	private BasicOcspResp GenerateResponse(ISignatureFactory signatureCalculator, X509Certificate[] chain, DateTime producedAt)
	{
		AlgorithmIdentifier algorithmIdentifier = (AlgorithmIdentifier)signatureCalculator.AlgorithmDetails;
		DerObjectIdentifier algorithm = algorithmIdentifier.Algorithm;
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (ResponseObject item in list)
		{
			try
			{
				asn1EncodableVector.Add(item.ToResponse());
			}
			catch (Exception e)
			{
				throw new OcspException("exception creating Request", e);
			}
		}
		ResponseData responseData = new ResponseData(responderID.ToAsn1Object(), new DerGeneralizedTime(producedAt), new DerSequence(asn1EncodableVector), responseExtensions);
		DerBitString derBitString = null;
		try
		{
			IStreamCalculator streamCalculator = signatureCalculator.CreateCalculator();
			byte[] derEncoded = responseData.GetDerEncoded();
			streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
			Platform.Dispose(streamCalculator.Stream);
			derBitString = new DerBitString(((IBlockResult)streamCalculator.GetResult()).Collect());
		}
		catch (Exception ex)
		{
			throw new OcspException("exception processing TBSRequest: " + ex, ex);
		}
		AlgorithmIdentifier sigAlgID = OcspUtilities.GetSigAlgID(algorithm);
		DerSequence certs = null;
		if (chain != null && chain.Length > 0)
		{
			Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
			try
			{
				for (int i = 0; i != chain.Length; i++)
				{
					asn1EncodableVector2.Add(X509CertificateStructure.GetInstance(Asn1Object.FromByteArray(chain[i].GetEncoded())));
				}
			}
			catch (IOException e2)
			{
				throw new OcspException("error processing certs", e2);
			}
			catch (CertificateEncodingException e3)
			{
				throw new OcspException("error encoding certs", e3);
			}
			certs = new DerSequence(asn1EncodableVector2);
		}
		return new BasicOcspResp(new BasicOcspResponse(responseData, sigAlgID, derBitString, certs));
	}

	public BasicOcspResp Generate(string signingAlgorithm, AsymmetricKeyParameter privateKey, X509Certificate[] chain, DateTime thisUpdate)
	{
		return Generate(signingAlgorithm, privateKey, chain, thisUpdate, null);
	}

	public BasicOcspResp Generate(string signingAlgorithm, AsymmetricKeyParameter privateKey, X509Certificate[] chain, DateTime producedAt, SecureRandom random)
	{
		if (signingAlgorithm == null)
		{
			throw new ArgumentException("no signing algorithm specified");
		}
		return GenerateResponse(new Asn1SignatureFactory(signingAlgorithm, privateKey, random), chain, producedAt);
	}

	public BasicOcspResp Generate(ISignatureFactory signatureCalculatorFactory, X509Certificate[] chain, DateTime producedAt)
	{
		if (signatureCalculatorFactory == null)
		{
			throw new ArgumentException("no signature calculator specified");
		}
		return GenerateResponse(signatureCalculatorFactory, chain, producedAt);
	}
}
