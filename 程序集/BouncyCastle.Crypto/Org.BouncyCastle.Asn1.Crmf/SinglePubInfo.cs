using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Crmf;

public class SinglePubInfo : Asn1Encodable
{
	private readonly DerInteger pubMethod;

	private readonly GeneralName pubLocation;

	public virtual GeneralName PubLocation => pubLocation;

	private SinglePubInfo(Asn1Sequence seq)
	{
		pubMethod = DerInteger.GetInstance(seq[0]);
		if (seq.Count == 2)
		{
			pubLocation = GeneralName.GetInstance(seq[1]);
		}
	}

	public static SinglePubInfo GetInstance(object obj)
	{
		if (obj is SinglePubInfo)
		{
			return (SinglePubInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new SinglePubInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(pubMethod);
		asn1EncodableVector.AddOptional(pubLocation);
		return new DerSequence(asn1EncodableVector);
	}
}
