using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class KekRecipientInfo : Asn1Encodable
{
	private DerInteger version;

	private KekIdentifier kekID;

	private AlgorithmIdentifier keyEncryptionAlgorithm;

	private Asn1OctetString encryptedKey;

	public DerInteger Version => version;

	public KekIdentifier KekID => kekID;

	public AlgorithmIdentifier KeyEncryptionAlgorithm => keyEncryptionAlgorithm;

	public Asn1OctetString EncryptedKey => encryptedKey;

	public KekRecipientInfo(KekIdentifier kekID, AlgorithmIdentifier keyEncryptionAlgorithm, Asn1OctetString encryptedKey)
	{
		version = new DerInteger(4);
		this.kekID = kekID;
		this.keyEncryptionAlgorithm = keyEncryptionAlgorithm;
		this.encryptedKey = encryptedKey;
	}

	public KekRecipientInfo(Asn1Sequence seq)
	{
		version = (DerInteger)seq[0];
		kekID = KekIdentifier.GetInstance(seq[1]);
		keyEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(seq[2]);
		encryptedKey = (Asn1OctetString)seq[3];
	}

	public static KekRecipientInfo GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static KekRecipientInfo GetInstance(object obj)
	{
		if (obj == null || obj is KekRecipientInfo)
		{
			return (KekRecipientInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new KekRecipientInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid KekRecipientInfo: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(version, kekID, keyEncryptionAlgorithm, encryptedKey);
	}
}
