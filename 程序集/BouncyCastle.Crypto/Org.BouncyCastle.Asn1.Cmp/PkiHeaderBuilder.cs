using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Cmp;

public class PkiHeaderBuilder
{
	private DerInteger pvno;

	private GeneralName sender;

	private GeneralName recipient;

	private DerGeneralizedTime messageTime;

	private AlgorithmIdentifier protectionAlg;

	private Asn1OctetString senderKID;

	private Asn1OctetString recipKID;

	private Asn1OctetString transactionID;

	private Asn1OctetString senderNonce;

	private Asn1OctetString recipNonce;

	private PkiFreeText freeText;

	private Asn1Sequence generalInfo;

	public PkiHeaderBuilder(int pvno, GeneralName sender, GeneralName recipient)
		: this(new DerInteger(pvno), sender, recipient)
	{
	}

	private PkiHeaderBuilder(DerInteger pvno, GeneralName sender, GeneralName recipient)
	{
		this.pvno = pvno;
		this.sender = sender;
		this.recipient = recipient;
	}

	public virtual PkiHeaderBuilder SetMessageTime(DerGeneralizedTime time)
	{
		messageTime = time;
		return this;
	}

	public virtual PkiHeaderBuilder SetProtectionAlg(AlgorithmIdentifier aid)
	{
		protectionAlg = aid;
		return this;
	}

	public virtual PkiHeaderBuilder SetSenderKID(byte[] kid)
	{
		return SetSenderKID((kid == null) ? null : new DerOctetString(kid));
	}

	public virtual PkiHeaderBuilder SetSenderKID(Asn1OctetString kid)
	{
		senderKID = kid;
		return this;
	}

	public virtual PkiHeaderBuilder SetRecipKID(byte[] kid)
	{
		return SetRecipKID((kid == null) ? null : new DerOctetString(kid));
	}

	public virtual PkiHeaderBuilder SetRecipKID(Asn1OctetString kid)
	{
		recipKID = kid;
		return this;
	}

	public virtual PkiHeaderBuilder SetTransactionID(byte[] tid)
	{
		return SetTransactionID((tid == null) ? null : new DerOctetString(tid));
	}

	public virtual PkiHeaderBuilder SetTransactionID(Asn1OctetString tid)
	{
		transactionID = tid;
		return this;
	}

	public virtual PkiHeaderBuilder SetSenderNonce(byte[] nonce)
	{
		return SetSenderNonce((nonce == null) ? null : new DerOctetString(nonce));
	}

	public virtual PkiHeaderBuilder SetSenderNonce(Asn1OctetString nonce)
	{
		senderNonce = nonce;
		return this;
	}

	public virtual PkiHeaderBuilder SetRecipNonce(byte[] nonce)
	{
		return SetRecipNonce((nonce == null) ? null : new DerOctetString(nonce));
	}

	public virtual PkiHeaderBuilder SetRecipNonce(Asn1OctetString nonce)
	{
		recipNonce = nonce;
		return this;
	}

	public virtual PkiHeaderBuilder SetFreeText(PkiFreeText text)
	{
		freeText = text;
		return this;
	}

	public virtual PkiHeaderBuilder SetGeneralInfo(InfoTypeAndValue genInfo)
	{
		return SetGeneralInfo(MakeGeneralInfoSeq(genInfo));
	}

	public virtual PkiHeaderBuilder SetGeneralInfo(InfoTypeAndValue[] genInfos)
	{
		return SetGeneralInfo(MakeGeneralInfoSeq(genInfos));
	}

	public virtual PkiHeaderBuilder SetGeneralInfo(Asn1Sequence seqOfInfoTypeAndValue)
	{
		generalInfo = seqOfInfoTypeAndValue;
		return this;
	}

	private static Asn1Sequence MakeGeneralInfoSeq(InfoTypeAndValue generalInfo)
	{
		return new DerSequence(generalInfo);
	}

	private static Asn1Sequence MakeGeneralInfoSeq(InfoTypeAndValue[] generalInfos)
	{
		Asn1Sequence result = null;
		if (generalInfos != null)
		{
			Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
			for (int i = 0; i < generalInfos.Length; i++)
			{
				asn1EncodableVector.Add(generalInfos[i]);
			}
			result = new DerSequence(asn1EncodableVector);
		}
		return result;
	}

	public virtual PkiHeader Build()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(pvno, sender, recipient);
		AddOptional(asn1EncodableVector, 0, messageTime);
		AddOptional(asn1EncodableVector, 1, protectionAlg);
		AddOptional(asn1EncodableVector, 2, senderKID);
		AddOptional(asn1EncodableVector, 3, recipKID);
		AddOptional(asn1EncodableVector, 4, transactionID);
		AddOptional(asn1EncodableVector, 5, senderNonce);
		AddOptional(asn1EncodableVector, 6, recipNonce);
		AddOptional(asn1EncodableVector, 7, freeText);
		AddOptional(asn1EncodableVector, 8, generalInfo);
		messageTime = null;
		protectionAlg = null;
		senderKID = null;
		recipKID = null;
		transactionID = null;
		senderNonce = null;
		recipNonce = null;
		freeText = null;
		generalInfo = null;
		return PkiHeader.GetInstance(new DerSequence(asn1EncodableVector));
	}

	private void AddOptional(Asn1EncodableVector v, int tagNo, Asn1Encodable obj)
	{
		if (obj != null)
		{
			v.Add(new DerTaggedObject(explicitly: true, tagNo, obj));
		}
	}
}
