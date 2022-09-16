using System;

namespace Org.BouncyCastle.Asn1.X500;

public class AttributeTypeAndValue : Asn1Encodable
{
	private readonly DerObjectIdentifier type;

	private readonly Asn1Encodable value;

	public virtual DerObjectIdentifier Type => type;

	public virtual Asn1Encodable Value => value;

	private AttributeTypeAndValue(Asn1Sequence seq)
	{
		type = (DerObjectIdentifier)seq[0];
		value = seq[1];
	}

	public static AttributeTypeAndValue GetInstance(object obj)
	{
		if (obj is AttributeTypeAndValue)
		{
			return (AttributeTypeAndValue)obj;
		}
		if (obj != null)
		{
			return new AttributeTypeAndValue(Asn1Sequence.GetInstance(obj));
		}
		throw new ArgumentNullException("obj");
	}

	public AttributeTypeAndValue(DerObjectIdentifier type, Asn1Encodable value)
	{
		this.type = type;
		this.value = value;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(type, value);
	}
}
