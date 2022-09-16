using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class InfoTypeAndValue : Asn1Encodable
{
	private readonly DerObjectIdentifier infoType;

	private readonly Asn1Encodable infoValue;

	public virtual DerObjectIdentifier InfoType => infoType;

	public virtual Asn1Encodable InfoValue => infoValue;

	private InfoTypeAndValue(Asn1Sequence seq)
	{
		infoType = DerObjectIdentifier.GetInstance(seq[0]);
		if (seq.Count > 1)
		{
			infoValue = seq[1];
		}
	}

	public static InfoTypeAndValue GetInstance(object obj)
	{
		if (obj is InfoTypeAndValue)
		{
			return (InfoTypeAndValue)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new InfoTypeAndValue((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public InfoTypeAndValue(DerObjectIdentifier infoType)
	{
		this.infoType = infoType;
		infoValue = null;
	}

	public InfoTypeAndValue(DerObjectIdentifier infoType, Asn1Encodable optionalValue)
	{
		this.infoType = infoType;
		infoValue = optionalValue;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(infoType);
		asn1EncodableVector.AddOptional(infoValue);
		return new DerSequence(asn1EncodableVector);
	}
}
