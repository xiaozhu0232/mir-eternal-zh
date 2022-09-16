using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class CertResponse : Asn1Encodable
{
	private readonly DerInteger certReqId;

	private readonly PkiStatusInfo status;

	private readonly CertifiedKeyPair certifiedKeyPair;

	private readonly Asn1OctetString rspInfo;

	public virtual DerInteger CertReqID => certReqId;

	public virtual PkiStatusInfo Status => status;

	public virtual CertifiedKeyPair CertifiedKeyPair => certifiedKeyPair;

	private CertResponse(Asn1Sequence seq)
	{
		certReqId = DerInteger.GetInstance(seq[0]);
		status = PkiStatusInfo.GetInstance(seq[1]);
		if (seq.Count < 3)
		{
			return;
		}
		if (seq.Count == 3)
		{
			Asn1Encodable asn1Encodable = seq[2];
			if (asn1Encodable is Asn1OctetString)
			{
				rspInfo = Asn1OctetString.GetInstance(asn1Encodable);
			}
			else
			{
				certifiedKeyPair = CertifiedKeyPair.GetInstance(asn1Encodable);
			}
		}
		else
		{
			certifiedKeyPair = CertifiedKeyPair.GetInstance(seq[2]);
			rspInfo = Asn1OctetString.GetInstance(seq[3]);
		}
	}

	public static CertResponse GetInstance(object obj)
	{
		if (obj is CertResponse)
		{
			return (CertResponse)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CertResponse((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public CertResponse(DerInteger certReqId, PkiStatusInfo status)
		: this(certReqId, status, null, null)
	{
	}

	public CertResponse(DerInteger certReqId, PkiStatusInfo status, CertifiedKeyPair certifiedKeyPair, Asn1OctetString rspInfo)
	{
		if (certReqId == null)
		{
			throw new ArgumentNullException("certReqId");
		}
		if (status == null)
		{
			throw new ArgumentNullException("status");
		}
		this.certReqId = certReqId;
		this.status = status;
		this.certifiedKeyPair = certifiedKeyPair;
		this.rspInfo = rspInfo;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(certReqId, status);
		asn1EncodableVector.AddOptional(certifiedKeyPair, rspInfo);
		return new DerSequence(asn1EncodableVector);
	}
}
