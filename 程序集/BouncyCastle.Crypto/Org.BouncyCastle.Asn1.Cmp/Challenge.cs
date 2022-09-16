using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class Challenge : Asn1Encodable
{
	private readonly AlgorithmIdentifier owf;

	private readonly Asn1OctetString witness;

	private readonly Asn1OctetString challenge;

	public virtual AlgorithmIdentifier Owf => owf;

	private Challenge(Asn1Sequence seq)
	{
		int index = 0;
		if (seq.Count == 3)
		{
			owf = AlgorithmIdentifier.GetInstance(seq[index++]);
		}
		witness = Asn1OctetString.GetInstance(seq[index++]);
		challenge = Asn1OctetString.GetInstance(seq[index]);
	}

	public static Challenge GetInstance(object obj)
	{
		if (obj is Challenge)
		{
			return (Challenge)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new Challenge((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptional(owf);
		asn1EncodableVector.Add(witness, challenge);
		return new DerSequence(asn1EncodableVector);
	}
}
