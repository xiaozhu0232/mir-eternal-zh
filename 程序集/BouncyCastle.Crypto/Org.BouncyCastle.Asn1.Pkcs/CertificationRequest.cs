using System;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class CertificationRequest : Asn1Encodable
{
	protected CertificationRequestInfo reqInfo;

	protected AlgorithmIdentifier sigAlgId;

	protected DerBitString sigBits;

	public AlgorithmIdentifier SignatureAlgorithm => sigAlgId;

	public DerBitString Signature => sigBits;

	public static CertificationRequest GetInstance(object obj)
	{
		if (obj is CertificationRequest)
		{
			return (CertificationRequest)obj;
		}
		if (obj != null)
		{
			return new CertificationRequest((Asn1Sequence)obj);
		}
		return null;
	}

	protected CertificationRequest()
	{
	}

	public CertificationRequest(CertificationRequestInfo requestInfo, AlgorithmIdentifier algorithm, DerBitString signature)
	{
		reqInfo = requestInfo;
		sigAlgId = algorithm;
		sigBits = signature;
	}

	[Obsolete("Use 'GetInstance' instead")]
	public CertificationRequest(Asn1Sequence seq)
	{
		if (seq.Count != 3)
		{
			throw new ArgumentException("Wrong number of elements in sequence", "seq");
		}
		reqInfo = CertificationRequestInfo.GetInstance(seq[0]);
		sigAlgId = AlgorithmIdentifier.GetInstance(seq[1]);
		sigBits = DerBitString.GetInstance(seq[2]);
	}

	public CertificationRequestInfo GetCertificationRequestInfo()
	{
		return reqInfo;
	}

	public byte[] GetSignatureOctets()
	{
		return sigBits.GetOctets();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(reqInfo, sigAlgId, sigBits);
	}
}
