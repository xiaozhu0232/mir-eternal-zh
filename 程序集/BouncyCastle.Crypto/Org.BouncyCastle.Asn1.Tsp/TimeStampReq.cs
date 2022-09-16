using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Tsp;

public class TimeStampReq : Asn1Encodable
{
	private readonly DerInteger version;

	private readonly MessageImprint messageImprint;

	private readonly DerObjectIdentifier tsaPolicy;

	private readonly DerInteger nonce;

	private readonly DerBoolean certReq;

	private readonly X509Extensions extensions;

	public DerInteger Version => version;

	public MessageImprint MessageImprint => messageImprint;

	public DerObjectIdentifier ReqPolicy => tsaPolicy;

	public DerInteger Nonce => nonce;

	public DerBoolean CertReq => certReq;

	public X509Extensions Extensions => extensions;

	public static TimeStampReq GetInstance(object obj)
	{
		if (obj is TimeStampReq)
		{
			return (TimeStampReq)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new TimeStampReq(Asn1Sequence.GetInstance(obj));
	}

	private TimeStampReq(Asn1Sequence seq)
	{
		int count = seq.Count;
		int num = 0;
		version = DerInteger.GetInstance(seq[num++]);
		messageImprint = MessageImprint.GetInstance(seq[num++]);
		for (int i = num; i < count; i++)
		{
			if (seq[i] is DerObjectIdentifier)
			{
				tsaPolicy = DerObjectIdentifier.GetInstance(seq[i]);
			}
			else if (seq[i] is DerInteger)
			{
				nonce = DerInteger.GetInstance(seq[i]);
			}
			else if (seq[i] is DerBoolean)
			{
				certReq = DerBoolean.GetInstance(seq[i]);
			}
			else if (seq[i] is Asn1TaggedObject)
			{
				Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[i];
				if (asn1TaggedObject.TagNo == 0)
				{
					extensions = X509Extensions.GetInstance(asn1TaggedObject, explicitly: false);
				}
			}
		}
	}

	public TimeStampReq(MessageImprint messageImprint, DerObjectIdentifier tsaPolicy, DerInteger nonce, DerBoolean certReq, X509Extensions extensions)
	{
		version = new DerInteger(1);
		this.messageImprint = messageImprint;
		this.tsaPolicy = tsaPolicy;
		this.nonce = nonce;
		this.certReq = certReq;
		this.extensions = extensions;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, messageImprint);
		asn1EncodableVector.AddOptional(tsaPolicy, nonce);
		if (certReq != null && certReq.IsTrue)
		{
			asn1EncodableVector.Add(certReq);
		}
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, extensions);
		return new DerSequence(asn1EncodableVector);
	}
}
