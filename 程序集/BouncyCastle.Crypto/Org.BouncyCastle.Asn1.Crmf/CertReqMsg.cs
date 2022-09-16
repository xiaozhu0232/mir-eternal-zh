using System;

namespace Org.BouncyCastle.Asn1.Crmf;

public class CertReqMsg : Asn1Encodable
{
	private readonly CertRequest certReq;

	private readonly ProofOfPossession popo;

	private readonly Asn1Sequence regInfo;

	public virtual CertRequest CertReq => certReq;

	public virtual ProofOfPossession Popo => popo;

	private CertReqMsg(Asn1Sequence seq)
	{
		certReq = CertRequest.GetInstance(seq[0]);
		for (int i = 1; i < seq.Count; i++)
		{
			object obj = seq[i];
			if (obj is Asn1TaggedObject || obj is ProofOfPossession)
			{
				popo = ProofOfPossession.GetInstance(obj);
			}
			else
			{
				regInfo = Asn1Sequence.GetInstance(obj);
			}
		}
	}

	public static CertReqMsg GetInstance(object obj)
	{
		if (obj is CertReqMsg)
		{
			return (CertReqMsg)obj;
		}
		if (obj != null)
		{
			return new CertReqMsg(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public static CertReqMsg GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public CertReqMsg(CertRequest certReq, ProofOfPossession popo, AttributeTypeAndValue[] regInfo)
	{
		if (certReq == null)
		{
			throw new ArgumentNullException("certReq");
		}
		this.certReq = certReq;
		this.popo = popo;
		if (regInfo != null)
		{
			this.regInfo = new DerSequence(regInfo);
		}
	}

	public virtual AttributeTypeAndValue[] GetRegInfo()
	{
		if (regInfo == null)
		{
			return null;
		}
		AttributeTypeAndValue[] array = new AttributeTypeAndValue[regInfo.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = AttributeTypeAndValue.GetInstance(regInfo[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(certReq);
		asn1EncodableVector.AddOptional(popo, regInfo);
		return new DerSequence(asn1EncodableVector);
	}
}
