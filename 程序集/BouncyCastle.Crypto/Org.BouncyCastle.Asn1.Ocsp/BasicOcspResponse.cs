using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ocsp;

public class BasicOcspResponse : Asn1Encodable
{
	private readonly ResponseData tbsResponseData;

	private readonly AlgorithmIdentifier signatureAlgorithm;

	private readonly DerBitString signature;

	private readonly Asn1Sequence certs;

	public ResponseData TbsResponseData => tbsResponseData;

	public AlgorithmIdentifier SignatureAlgorithm => signatureAlgorithm;

	public DerBitString Signature => signature;

	public Asn1Sequence Certs => certs;

	public static BasicOcspResponse GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static BasicOcspResponse GetInstance(object obj)
	{
		if (obj == null || obj is BasicOcspResponse)
		{
			return (BasicOcspResponse)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new BasicOcspResponse((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public BasicOcspResponse(ResponseData tbsResponseData, AlgorithmIdentifier signatureAlgorithm, DerBitString signature, Asn1Sequence certs)
	{
		this.tbsResponseData = tbsResponseData;
		this.signatureAlgorithm = signatureAlgorithm;
		this.signature = signature;
		this.certs = certs;
	}

	private BasicOcspResponse(Asn1Sequence seq)
	{
		tbsResponseData = ResponseData.GetInstance(seq[0]);
		signatureAlgorithm = AlgorithmIdentifier.GetInstance(seq[1]);
		signature = (DerBitString)seq[2];
		if (seq.Count > 3)
		{
			certs = Asn1Sequence.GetInstance((Asn1TaggedObject)seq[3], explicitly: true);
		}
	}

	[Obsolete("Use TbsResponseData property instead")]
	public ResponseData GetTbsResponseData()
	{
		return tbsResponseData;
	}

	[Obsolete("Use SignatureAlgorithm property instead")]
	public AlgorithmIdentifier GetSignatureAlgorithm()
	{
		return signatureAlgorithm;
	}

	[Obsolete("Use Signature property instead")]
	public DerBitString GetSignature()
	{
		return signature;
	}

	public byte[] GetSignatureOctets()
	{
		return signature.GetOctets();
	}

	[Obsolete("Use Certs property instead")]
	public Asn1Sequence GetCerts()
	{
		return certs;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(tbsResponseData, signatureAlgorithm, signature);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, certs);
		return new DerSequence(asn1EncodableVector);
	}
}
