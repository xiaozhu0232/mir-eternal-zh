using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class SignerIdentifier : Asn1Encodable, IAsn1Choice
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
			return id;
		}
	}

	public SignerIdentifier(IssuerAndSerialNumber id)
	{
		this.id = id;
	}

	public SignerIdentifier(Asn1OctetString id)
	{
		this.id = new DerTaggedObject(explicitly: false, 0, id);
	}

	public SignerIdentifier(Asn1Object id)
	{
		this.id = id;
	}

	public static SignerIdentifier GetInstance(object o)
	{
		if (o == null || o is SignerIdentifier)
		{
			return (SignerIdentifier)o;
		}
		if (o is IssuerAndSerialNumber)
		{
			return new SignerIdentifier((IssuerAndSerialNumber)o);
		}
		if (o is Asn1OctetString)
		{
			return new SignerIdentifier((Asn1OctetString)o);
		}
		if (o is Asn1Object)
		{
			return new SignerIdentifier((Asn1Object)o);
		}
		throw new ArgumentException("Illegal object in SignerIdentifier: " + Platform.GetTypeName(o));
	}

	public override Asn1Object ToAsn1Object()
	{
		return id.ToAsn1Object();
	}
}
