using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class OtherKeyAttribute : Asn1Encodable
{
	private DerObjectIdentifier keyAttrId;

	private Asn1Encodable keyAttr;

	public DerObjectIdentifier KeyAttrId => keyAttrId;

	public Asn1Encodable KeyAttr => keyAttr;

	public static OtherKeyAttribute GetInstance(object obj)
	{
		if (obj == null || obj is OtherKeyAttribute)
		{
			return (OtherKeyAttribute)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OtherKeyAttribute((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public OtherKeyAttribute(Asn1Sequence seq)
	{
		keyAttrId = (DerObjectIdentifier)seq[0];
		keyAttr = seq[1];
	}

	public OtherKeyAttribute(DerObjectIdentifier keyAttrId, Asn1Encodable keyAttr)
	{
		this.keyAttrId = keyAttrId;
		this.keyAttr = keyAttr;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(keyAttrId, keyAttr);
	}
}
