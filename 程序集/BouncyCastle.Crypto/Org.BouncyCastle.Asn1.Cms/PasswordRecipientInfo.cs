using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class PasswordRecipientInfo : Asn1Encodable
{
	private readonly DerInteger version;

	private readonly AlgorithmIdentifier keyDerivationAlgorithm;

	private readonly AlgorithmIdentifier keyEncryptionAlgorithm;

	private readonly Asn1OctetString encryptedKey;

	public DerInteger Version => version;

	public AlgorithmIdentifier KeyDerivationAlgorithm => keyDerivationAlgorithm;

	public AlgorithmIdentifier KeyEncryptionAlgorithm => keyEncryptionAlgorithm;

	public Asn1OctetString EncryptedKey => encryptedKey;

	public PasswordRecipientInfo(AlgorithmIdentifier keyEncryptionAlgorithm, Asn1OctetString encryptedKey)
	{
		version = new DerInteger(0);
		this.keyEncryptionAlgorithm = keyEncryptionAlgorithm;
		this.encryptedKey = encryptedKey;
	}

	public PasswordRecipientInfo(AlgorithmIdentifier keyDerivationAlgorithm, AlgorithmIdentifier keyEncryptionAlgorithm, Asn1OctetString encryptedKey)
	{
		version = new DerInteger(0);
		this.keyDerivationAlgorithm = keyDerivationAlgorithm;
		this.keyEncryptionAlgorithm = keyEncryptionAlgorithm;
		this.encryptedKey = encryptedKey;
	}

	public PasswordRecipientInfo(Asn1Sequence seq)
	{
		version = (DerInteger)seq[0];
		if (seq[1] is Asn1TaggedObject)
		{
			keyDerivationAlgorithm = AlgorithmIdentifier.GetInstance((Asn1TaggedObject)seq[1], explicitly: false);
			keyEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(seq[2]);
			encryptedKey = (Asn1OctetString)seq[3];
		}
		else
		{
			keyEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(seq[1]);
			encryptedKey = (Asn1OctetString)seq[2];
		}
	}

	public static PasswordRecipientInfo GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static PasswordRecipientInfo GetInstance(object obj)
	{
		if (obj == null || obj is PasswordRecipientInfo)
		{
			return (PasswordRecipientInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PasswordRecipientInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid PasswordRecipientInfo: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, keyDerivationAlgorithm);
		asn1EncodableVector.Add(keyEncryptionAlgorithm, encryptedKey);
		return new DerSequence(asn1EncodableVector);
	}
}
