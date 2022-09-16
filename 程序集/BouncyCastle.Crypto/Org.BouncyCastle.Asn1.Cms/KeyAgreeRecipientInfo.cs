using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class KeyAgreeRecipientInfo : Asn1Encodable
{
	private DerInteger version;

	private OriginatorIdentifierOrKey originator;

	private Asn1OctetString ukm;

	private AlgorithmIdentifier keyEncryptionAlgorithm;

	private Asn1Sequence recipientEncryptedKeys;

	public DerInteger Version => version;

	public OriginatorIdentifierOrKey Originator => originator;

	public Asn1OctetString UserKeyingMaterial => ukm;

	public AlgorithmIdentifier KeyEncryptionAlgorithm => keyEncryptionAlgorithm;

	public Asn1Sequence RecipientEncryptedKeys => recipientEncryptedKeys;

	public KeyAgreeRecipientInfo(OriginatorIdentifierOrKey originator, Asn1OctetString ukm, AlgorithmIdentifier keyEncryptionAlgorithm, Asn1Sequence recipientEncryptedKeys)
	{
		version = new DerInteger(3);
		this.originator = originator;
		this.ukm = ukm;
		this.keyEncryptionAlgorithm = keyEncryptionAlgorithm;
		this.recipientEncryptedKeys = recipientEncryptedKeys;
	}

	public KeyAgreeRecipientInfo(Asn1Sequence seq)
	{
		int index = 0;
		version = (DerInteger)seq[index++];
		originator = OriginatorIdentifierOrKey.GetInstance((Asn1TaggedObject)seq[index++], explicitly: true);
		if (seq[index] is Asn1TaggedObject)
		{
			ukm = Asn1OctetString.GetInstance((Asn1TaggedObject)seq[index++], isExplicit: true);
		}
		keyEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(seq[index++]);
		recipientEncryptedKeys = (Asn1Sequence)seq[index++];
	}

	public static KeyAgreeRecipientInfo GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static KeyAgreeRecipientInfo GetInstance(object obj)
	{
		if (obj == null || obj is KeyAgreeRecipientInfo)
		{
			return (KeyAgreeRecipientInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new KeyAgreeRecipientInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Illegal object in KeyAgreeRecipientInfo: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, new DerTaggedObject(explicitly: true, 0, originator));
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, ukm);
		asn1EncodableVector.Add(keyEncryptionAlgorithm, recipientEncryptedKeys);
		return new DerSequence(asn1EncodableVector);
	}
}
