using System;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Ocsp;

public class ResponderID : Asn1Encodable, IAsn1Choice
{
	private readonly Asn1Encodable id;

	public virtual X509Name Name
	{
		get
		{
			if (id is Asn1OctetString)
			{
				return null;
			}
			return X509Name.GetInstance(id);
		}
	}

	public static ResponderID GetInstance(object obj)
	{
		if (obj == null || obj is ResponderID)
		{
			return (ResponderID)obj;
		}
		if (obj is DerOctetString)
		{
			return new ResponderID((DerOctetString)obj);
		}
		if (obj is Asn1TaggedObject)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)obj;
			if (asn1TaggedObject.TagNo == 1)
			{
				return new ResponderID(X509Name.GetInstance(asn1TaggedObject, explicitly: true));
			}
			return new ResponderID(Asn1OctetString.GetInstance(asn1TaggedObject, isExplicit: true));
		}
		return new ResponderID(X509Name.GetInstance(obj));
	}

	public ResponderID(Asn1OctetString id)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		this.id = id;
	}

	public ResponderID(X509Name id)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		this.id = id;
	}

	public static ResponderID GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(obj.GetObject());
	}

	public virtual byte[] GetKeyHash()
	{
		if (id is Asn1OctetString)
		{
			return ((Asn1OctetString)id).GetOctets();
		}
		return null;
	}

	public override Asn1Object ToAsn1Object()
	{
		if (id is Asn1OctetString)
		{
			return new DerTaggedObject(explicitly: true, 2, id);
		}
		return new DerTaggedObject(explicitly: true, 1, id);
	}
}
