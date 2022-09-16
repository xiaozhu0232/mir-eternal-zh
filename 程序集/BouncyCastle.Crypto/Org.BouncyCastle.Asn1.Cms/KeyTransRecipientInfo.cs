using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class KeyTransRecipientInfo : Asn1Encodable
{
	private DerInteger version;

	private RecipientIdentifier rid;

	private AlgorithmIdentifier keyEncryptionAlgorithm;

	private Asn1OctetString encryptedKey;

	public DerInteger Version => version;

	public RecipientIdentifier RecipientIdentifier => rid;

	public AlgorithmIdentifier KeyEncryptionAlgorithm => keyEncryptionAlgorithm;

	public Asn1OctetString EncryptedKey => encryptedKey;

	public KeyTransRecipientInfo(RecipientIdentifier rid, AlgorithmIdentifier keyEncryptionAlgorithm, Asn1OctetString encryptedKey)
	{
		if (rid.ToAsn1Object() is Asn1TaggedObject)
		{
			version = new DerInteger(2);
		}
		else
		{
			version = new DerInteger(0);
		}
		this.rid = rid;
		this.keyEncryptionAlgorithm = keyEncryptionAlgorithm;
		this.encryptedKey = encryptedKey;
	}

	public KeyTransRecipientInfo(Asn1Sequence seq)
	{
		version = (DerInteger)seq[0];
		rid = RecipientIdentifier.GetInstance(seq[1]);
		keyEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(seq[2]);
		encryptedKey = (Asn1OctetString)seq[3];
	}

	public static KeyTransRecipientInfo GetInstance(object obj)
	{
		if (obj == null || obj is KeyTransRecipientInfo)
		{
			return (KeyTransRecipientInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new KeyTransRecipientInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Illegal object in KeyTransRecipientInfo: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(version, rid, keyEncryptionAlgorithm, encryptedKey);
	}
}
