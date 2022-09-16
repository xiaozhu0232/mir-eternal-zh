using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ess;

public class ContentIdentifier : Asn1Encodable
{
	private Asn1OctetString value;

	public Asn1OctetString Value => value;

	public static ContentIdentifier GetInstance(object o)
	{
		if (o == null || o is ContentIdentifier)
		{
			return (ContentIdentifier)o;
		}
		if (o is Asn1OctetString)
		{
			return new ContentIdentifier((Asn1OctetString)o);
		}
		throw new ArgumentException("unknown object in 'ContentIdentifier' factory : " + Platform.GetTypeName(o) + ".");
	}

	public ContentIdentifier(Asn1OctetString value)
	{
		this.value = value;
	}

	public ContentIdentifier(byte[] value)
		: this(new DerOctetString(value))
	{
	}

	public override Asn1Object ToAsn1Object()
	{
		return value;
	}
}
