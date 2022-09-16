using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms;

public class DefaultSignedAttributeTableGenerator : CmsAttributeTableGenerator
{
	private readonly IDictionary table;

	public DefaultSignedAttributeTableGenerator()
	{
		table = Platform.CreateHashtable();
	}

	public DefaultSignedAttributeTableGenerator(AttributeTable attributeTable)
	{
		if (attributeTable != null)
		{
			table = attributeTable.ToDictionary();
		}
		else
		{
			table = Platform.CreateHashtable();
		}
	}

	protected virtual Hashtable createStandardAttributeTable(IDictionary parameters)
	{
		Hashtable hashtable = new Hashtable(table);
		DoCreateStandardAttributeTable(parameters, hashtable);
		return hashtable;
	}

	private void DoCreateStandardAttributeTable(IDictionary parameters, IDictionary std)
	{
		if (parameters.Contains(CmsAttributeTableParameter.ContentType) && !std.Contains(CmsAttributes.ContentType))
		{
			DerObjectIdentifier element = (DerObjectIdentifier)parameters[CmsAttributeTableParameter.ContentType];
			Org.BouncyCastle.Asn1.Cms.Attribute attribute = new Org.BouncyCastle.Asn1.Cms.Attribute(CmsAttributes.ContentType, new DerSet(element));
			std[attribute.AttrType] = attribute;
		}
		if (!std.Contains(CmsAttributes.SigningTime))
		{
			Org.BouncyCastle.Asn1.Cms.Attribute attribute2 = new Org.BouncyCastle.Asn1.Cms.Attribute(CmsAttributes.SigningTime, new DerSet(new Time(DateTime.UtcNow)));
			std[attribute2.AttrType] = attribute2;
		}
		if (!std.Contains(CmsAttributes.MessageDigest))
		{
			byte[] str = (byte[])parameters[CmsAttributeTableParameter.Digest];
			Org.BouncyCastle.Asn1.Cms.Attribute attribute3 = new Org.BouncyCastle.Asn1.Cms.Attribute(CmsAttributes.MessageDigest, new DerSet(new DerOctetString(str)));
			std[attribute3.AttrType] = attribute3;
		}
	}

	public virtual AttributeTable GetAttributes(IDictionary parameters)
	{
		IDictionary attrs = createStandardAttributeTable(parameters);
		return new AttributeTable(attrs);
	}
}
