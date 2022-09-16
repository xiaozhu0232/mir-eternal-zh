using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class RecipientEncryptedKey : Asn1Encodable
{
	private readonly KeyAgreeRecipientIdentifier identifier;

	private readonly Asn1OctetString encryptedKey;

	public KeyAgreeRecipientIdentifier Identifier => identifier;

	public Asn1OctetString EncryptedKey => encryptedKey;

	private RecipientEncryptedKey(Asn1Sequence seq)
	{
		identifier = KeyAgreeRecipientIdentifier.GetInstance(seq[0]);
		encryptedKey = (Asn1OctetString)seq[1];
	}

	public static RecipientEncryptedKey GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public static RecipientEncryptedKey GetInstance(object obj)
	{
		if (obj == null || obj is RecipientEncryptedKey)
		{
			return (RecipientEncryptedKey)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RecipientEncryptedKey((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid RecipientEncryptedKey: " + Platform.GetTypeName(obj), "obj");
	}

	public RecipientEncryptedKey(KeyAgreeRecipientIdentifier id, Asn1OctetString encryptedKey)
	{
		identifier = id;
		this.encryptedKey = encryptedKey;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(identifier, encryptedKey);
	}
}
