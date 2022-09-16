using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Crmf;

public class PopoSigningKey : Asn1Encodable
{
	private readonly PopoSigningKeyInput poposkInput;

	private readonly AlgorithmIdentifier algorithmIdentifier;

	private readonly DerBitString signature;

	public virtual PopoSigningKeyInput PoposkInput => poposkInput;

	public virtual AlgorithmIdentifier AlgorithmIdentifier => algorithmIdentifier;

	public virtual DerBitString Signature => signature;

	private PopoSigningKey(Asn1Sequence seq)
	{
		int index = 0;
		if (seq[index] is Asn1TaggedObject)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[index++];
			if (asn1TaggedObject.TagNo != 0)
			{
				throw new ArgumentException("Unknown PopoSigningKeyInput tag: " + asn1TaggedObject.TagNo, "seq");
			}
			poposkInput = PopoSigningKeyInput.GetInstance(asn1TaggedObject.GetObject());
		}
		algorithmIdentifier = AlgorithmIdentifier.GetInstance(seq[index++]);
		signature = DerBitString.GetInstance(seq[index]);
	}

	public static PopoSigningKey GetInstance(object obj)
	{
		if (obj is PopoSigningKey)
		{
			return (PopoSigningKey)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PopoSigningKey((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public static PopoSigningKey GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public PopoSigningKey(PopoSigningKeyInput poposkIn, AlgorithmIdentifier aid, DerBitString signature)
	{
		poposkInput = poposkIn;
		algorithmIdentifier = aid;
		this.signature = signature;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, poposkInput);
		asn1EncodableVector.Add(algorithmIdentifier);
		asn1EncodableVector.Add(signature);
		return new DerSequence(asn1EncodableVector);
	}
}
