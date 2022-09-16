namespace Org.BouncyCastle.Asn1.Cmp;

public class PkiMessage : Asn1Encodable
{
	private readonly PkiHeader header;

	private readonly PkiBody body;

	private readonly DerBitString protection;

	private readonly Asn1Sequence extraCerts;

	public virtual PkiHeader Header => header;

	public virtual PkiBody Body => body;

	public virtual DerBitString Protection => protection;

	private PkiMessage(Asn1Sequence seq)
	{
		header = PkiHeader.GetInstance(seq[0]);
		body = PkiBody.GetInstance(seq[1]);
		for (int i = 2; i < seq.Count; i++)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[i].ToAsn1Object();
			if (asn1TaggedObject.TagNo == 0)
			{
				protection = DerBitString.GetInstance(asn1TaggedObject, isExplicit: true);
			}
			else
			{
				extraCerts = Asn1Sequence.GetInstance(asn1TaggedObject, explicitly: true);
			}
		}
	}

	public static PkiMessage GetInstance(object obj)
	{
		if (obj is PkiMessage)
		{
			return (PkiMessage)obj;
		}
		if (obj != null)
		{
			return new PkiMessage(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public PkiMessage(PkiHeader header, PkiBody body, DerBitString protection, CmpCertificate[] extraCerts)
	{
		this.header = header;
		this.body = body;
		this.protection = protection;
		if (extraCerts != null)
		{
			this.extraCerts = new DerSequence(extraCerts);
		}
	}

	public PkiMessage(PkiHeader header, PkiBody body, DerBitString protection)
		: this(header, body, protection, null)
	{
	}

	public PkiMessage(PkiHeader header, PkiBody body)
		: this(header, body, null, null)
	{
	}

	public virtual CmpCertificate[] GetExtraCerts()
	{
		if (extraCerts == null)
		{
			return null;
		}
		CmpCertificate[] array = new CmpCertificate[extraCerts.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = CmpCertificate.GetInstance(extraCerts[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(header, body);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, protection);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, extraCerts);
		return new DerSequence(asn1EncodableVector);
	}
}
