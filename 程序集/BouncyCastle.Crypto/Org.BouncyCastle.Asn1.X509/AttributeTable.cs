using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class AttributeTable
{
	private readonly IDictionary attributes;

	public AttributeTable(IDictionary attrs)
	{
		attributes = Platform.CreateHashtable(attrs);
	}

	[Obsolete]
	public AttributeTable(Hashtable attrs)
	{
		attributes = Platform.CreateHashtable(attrs);
	}

	public AttributeTable(Asn1EncodableVector v)
	{
		attributes = Platform.CreateHashtable(v.Count);
		for (int i = 0; i != v.Count; i++)
		{
			AttributeX509 instance = AttributeX509.GetInstance(v[i]);
			attributes.Add(instance.AttrType, instance);
		}
	}

	public AttributeTable(Asn1Set s)
	{
		attributes = Platform.CreateHashtable(s.Count);
		for (int i = 0; i != s.Count; i++)
		{
			AttributeX509 instance = AttributeX509.GetInstance(s[i]);
			attributes.Add(instance.AttrType, instance);
		}
	}

	public AttributeX509 Get(DerObjectIdentifier oid)
	{
		return (AttributeX509)attributes[oid];
	}

	[Obsolete("Use 'ToDictionary' instead")]
	public Hashtable ToHashtable()
	{
		return new Hashtable(attributes);
	}

	public IDictionary ToDictionary()
	{
		return Platform.CreateHashtable(attributes);
	}
}
