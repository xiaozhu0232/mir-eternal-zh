using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms.Ecc;

public class MQVuserKeyingMaterial : Asn1Encodable
{
	private OriginatorPublicKey ephemeralPublicKey;

	private Asn1OctetString addedukm;

	public OriginatorPublicKey EphemeralPublicKey => ephemeralPublicKey;

	public Asn1OctetString AddedUkm => addedukm;

	public MQVuserKeyingMaterial(OriginatorPublicKey ephemeralPublicKey, Asn1OctetString addedukm)
	{
		this.ephemeralPublicKey = ephemeralPublicKey;
		this.addedukm = addedukm;
	}

	private MQVuserKeyingMaterial(Asn1Sequence seq)
	{
		ephemeralPublicKey = OriginatorPublicKey.GetInstance(seq[0]);
		if (seq.Count > 1)
		{
			addedukm = Asn1OctetString.GetInstance((Asn1TaggedObject)seq[1], isExplicit: true);
		}
	}

	public static MQVuserKeyingMaterial GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public static MQVuserKeyingMaterial GetInstance(object obj)
	{
		if (obj == null || obj is MQVuserKeyingMaterial)
		{
			return (MQVuserKeyingMaterial)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new MQVuserKeyingMaterial((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid MQVuserKeyingMaterial: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(ephemeralPublicKey);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, addedukm);
		return new DerSequence(asn1EncodableVector);
	}
}
