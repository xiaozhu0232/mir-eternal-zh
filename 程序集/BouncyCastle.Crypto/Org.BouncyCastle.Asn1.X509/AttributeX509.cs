using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class AttributeX509 : Asn1Encodable
{
	private readonly DerObjectIdentifier attrType;

	private readonly Asn1Set attrValues;

	public DerObjectIdentifier AttrType => attrType;

	public Asn1Set AttrValues => attrValues;

	public static AttributeX509 GetInstance(object obj)
	{
		if (obj == null || obj is AttributeX509)
		{
			return (AttributeX509)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new AttributeX509((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private AttributeX509(Asn1Sequence seq)
	{
		if (seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		attrType = DerObjectIdentifier.GetInstance(seq[0]);
		attrValues = Asn1Set.GetInstance(seq[1]);
	}

	public AttributeX509(DerObjectIdentifier attrType, Asn1Set attrValues)
	{
		this.attrType = attrType;
		this.attrValues = attrValues;
	}

	public Asn1Encodable[] GetAttributeValues()
	{
		return attrValues.ToArray();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(attrType, attrValues);
	}
}
