namespace Org.BouncyCastle.Asn1.Crmf;

public class CertRequest : Asn1Encodable
{
	private readonly DerInteger certReqId;

	private readonly CertTemplate certTemplate;

	private readonly Controls controls;

	public virtual DerInteger CertReqID => certReqId;

	public virtual CertTemplate CertTemplate => certTemplate;

	public virtual Controls Controls => controls;

	private CertRequest(Asn1Sequence seq)
	{
		certReqId = DerInteger.GetInstance(seq[0]);
		certTemplate = CertTemplate.GetInstance(seq[1]);
		if (seq.Count > 2)
		{
			controls = Controls.GetInstance(seq[2]);
		}
	}

	public static CertRequest GetInstance(object obj)
	{
		if (obj is CertRequest)
		{
			return (CertRequest)obj;
		}
		if (obj != null)
		{
			return new CertRequest(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public CertRequest(int certReqId, CertTemplate certTemplate, Controls controls)
		: this(new DerInteger(certReqId), certTemplate, controls)
	{
	}

	public CertRequest(DerInteger certReqId, CertTemplate certTemplate, Controls controls)
	{
		this.certReqId = certReqId;
		this.certTemplate = certTemplate;
		this.controls = controls;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(certReqId, certTemplate);
		asn1EncodableVector.AddOptional(controls);
		return new DerSequence(asn1EncodableVector);
	}
}
