using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class Attribute : Asn1Encodable
{
	private DerObjectIdentifier attrType;

	private Asn1Set attrValues;

	public DerObjectIdentifier AttrType => attrType;

	public Asn1Set AttrValues => attrValues;

	public static Attribute GetInstance(object obj)
	{
		if (obj == null || obj is Attribute)
		{
			return (Attribute)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new Attribute((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public Attribute(Asn1Sequence seq)
	{
		attrType = (DerObjectIdentifier)seq[0];
		attrValues = (Asn1Set)seq[1];
	}

	public Attribute(DerObjectIdentifier attrType, Asn1Set attrValues)
	{
		this.attrType = attrType;
		this.attrValues = attrValues;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(attrType, attrValues);
	}
}
