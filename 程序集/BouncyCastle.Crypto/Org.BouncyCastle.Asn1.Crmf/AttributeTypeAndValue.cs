using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Crmf;

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
		if (obj is Asn1Sequence)
		{
			return new AttributeTypeAndValue((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public AttributeTypeAndValue(string oid, Asn1Encodable value)
		: this(new DerObjectIdentifier(oid), value)
	{
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
