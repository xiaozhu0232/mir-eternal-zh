using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class RecipientIdentifier : Asn1Encodable, IAsn1Choice
{
	private Asn1Encodable id;

	public bool IsTagged => id is Asn1TaggedObject;

	public Asn1Encodable ID
	{
		get
		{
			if (id is Asn1TaggedObject)
			{
				return Asn1OctetString.GetInstance((Asn1TaggedObject)id, isExplicit: false);
			}
			return IssuerAndSerialNumber.GetInstance(id);
		}
	}

	public RecipientIdentifier(IssuerAndSerialNumber id)
	{
		this.id = id;
	}

	public RecipientIdentifier(Asn1OctetString id)
	{
		this.id = new DerTaggedObject(explicitly: false, 0, id);
	}

	public RecipientIdentifier(Asn1Object id)
	{
		this.id = id;
	}

	public static RecipientIdentifier GetInstance(object o)
	{
		if (o == null || o is RecipientIdentifier)
		{
			return (RecipientIdentifier)o;
		}
		if (o is IssuerAndSerialNumber)
		{
			return new RecipientIdentifier((IssuerAndSerialNumber)o);
		}
		if (o is Asn1OctetString)
		{
			return new RecipientIdentifier((Asn1OctetString)o);
		}
		if (o is Asn1Object)
		{
			return new RecipientIdentifier((Asn1Object)o);
		}
		throw new ArgumentException("Illegal object in RecipientIdentifier: " + Platform.GetTypeName(o));
	}

	public override Asn1Object ToAsn1Object()
	{
		return id.ToAsn1Object();
	}
}
