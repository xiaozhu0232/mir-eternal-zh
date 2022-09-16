using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Crmf;

public class PopoSigningKeyInput : Asn1Encodable
{
	private readonly GeneralName sender;

	private readonly PKMacValue publicKeyMac;

	private readonly SubjectPublicKeyInfo publicKey;

	public virtual GeneralName Sender => sender;

	public virtual PKMacValue PublicKeyMac => publicKeyMac;

	public virtual SubjectPublicKeyInfo PublicKey => publicKey;

	private PopoSigningKeyInput(Asn1Sequence seq)
	{
		Asn1Encodable asn1Encodable = seq[0];
		if (asn1Encodable is Asn1TaggedObject)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)asn1Encodable;
			if (asn1TaggedObject.TagNo != 0)
			{
				throw new ArgumentException("Unknown authInfo tag: " + asn1TaggedObject.TagNo, "seq");
			}
			sender = GeneralName.GetInstance(asn1TaggedObject.GetObject());
		}
		else
		{
			publicKeyMac = PKMacValue.GetInstance(asn1Encodable);
		}
		publicKey = SubjectPublicKeyInfo.GetInstance(seq[1]);
	}

	public static PopoSigningKeyInput GetInstance(object obj)
	{
		if (obj is PopoSigningKeyInput)
		{
			return (PopoSigningKeyInput)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PopoSigningKeyInput((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public PopoSigningKeyInput(GeneralName sender, SubjectPublicKeyInfo spki)
	{
		this.sender = sender;
		publicKey = spki;
	}

	public PopoSigningKeyInput(PKMacValue pkmac, SubjectPublicKeyInfo spki)
	{
		publicKeyMac = pkmac;
		publicKey = spki;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		if (sender != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 0, sender));
		}
		else
		{
			asn1EncodableVector.Add(publicKeyMac);
		}
		asn1EncodableVector.Add(publicKey);
		return new DerSequence(asn1EncodableVector);
	}
}
