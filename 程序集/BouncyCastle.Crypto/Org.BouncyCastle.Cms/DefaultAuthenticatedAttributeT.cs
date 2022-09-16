using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms;

public class DefaultAuthenticatedAttributeTableGenerator : CmsAttributeTableGenerator
{
	private readonly IDictionary table;

	public DefaultAuthenticatedAttributeTableGenerator()
	{
		table = Platform.CreateHashtable();
	}

	public DefaultAuthenticatedAttributeTableGenerator(AttributeTable attributeTable)
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

	protected virtual IDictionary CreateStandardAttributeTable(IDictionary parameters)
	{
		IDictionary dictionary = Platform.CreateHashtable(table);
		if (!dictionary.Contains(CmsAttributes.ContentType))
		{
			DerObjectIdentifier element = (DerObjectIdentifier)parameters[CmsAttributeTableParameter.ContentType];
			Attribute attribute = new Attribute(CmsAttributes.ContentType, new DerSet(element));
			dictionary[attribute.AttrType] = attribute;
		}
		if (!dictionary.Contains(CmsAttributes.MessageDigest))
		{
			byte[] str = (byte[])parameters[CmsAttributeTableParameter.Digest];
			Attribute attribute2 = new Attribute(CmsAttributes.MessageDigest, new DerSet(new DerOctetString(str)));
			dictionary[attribute2.AttrType] = attribute2;
		}
		return dictionary;
	}

	public virtual AttributeTable GetAttributes(IDictionary parameters)
	{
		IDictionary attrs = CreateStandardAttributeTable(parameters);
		return new AttributeTable(attrs);
	}
}
