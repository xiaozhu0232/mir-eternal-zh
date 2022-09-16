using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Pkcs;

public abstract class Pkcs12Entry
{
	private readonly IDictionary attributes;

	public Asn1Encodable this[DerObjectIdentifier oid] => (Asn1Encodable)attributes[oid.Id];

	public Asn1Encodable this[string oid] => (Asn1Encodable)attributes[oid];

	public IEnumerable BagAttributeKeys => new EnumerableProxy(attributes.Keys);

	protected internal Pkcs12Entry(IDictionary attributes)
	{
		this.attributes = attributes;
		foreach (object attribute in attributes)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)attribute;
			if (!(dictionaryEntry.Key is string))
			{
				throw new ArgumentException("Attribute keys must be of type: " + typeof(string).FullName, "attributes");
			}
			if (!(dictionaryEntry.Value is Asn1Encodable))
			{
				throw new ArgumentException("Attribute values must be of type: " + typeof(Asn1Encodable).FullName, "attributes");
			}
		}
	}

	[Obsolete("Use 'object[index]' syntax instead")]
	public Asn1Encodable GetBagAttribute(DerObjectIdentifier oid)
	{
		return (Asn1Encodable)attributes[oid.Id];
	}

	[Obsolete("Use 'object[index]' syntax instead")]
	public Asn1Encodable GetBagAttribute(string oid)
	{
		return (Asn1Encodable)attributes[oid];
	}

	[Obsolete("Use 'BagAttributeKeys' property")]
	public IEnumerator GetBagAttributeKeys()
	{
		return attributes.Keys.GetEnumerator();
	}
}
