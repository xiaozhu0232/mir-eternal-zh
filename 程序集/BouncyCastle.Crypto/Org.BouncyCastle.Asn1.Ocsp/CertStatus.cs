using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ocsp;

public class CertStatus : Asn1Encodable, IAsn1Choice
{
	private readonly int tagNo;

	private readonly Asn1Encodable value;

	public int TagNo => tagNo;

	public Asn1Encodable Status => value;

	public CertStatus()
	{
		tagNo = 0;
		value = DerNull.Instance;
	}

	public CertStatus(RevokedInfo info)
	{
		tagNo = 1;
		value = info;
	}

	public CertStatus(int tagNo, Asn1Encodable value)
	{
		this.tagNo = tagNo;
		this.value = value;
	}

	public CertStatus(Asn1TaggedObject choice)
	{
		tagNo = choice.TagNo;
		switch (choice.TagNo)
		{
		case 1:
			value = RevokedInfo.GetInstance(choice, explicitly: false);
			break;
		case 0:
		case 2:
			value = DerNull.Instance;
			break;
		default:
			throw new ArgumentException("Unknown tag encountered: " + choice.TagNo);
		}
	}

	public static CertStatus GetInstance(object obj)
	{
		if (obj == null || obj is CertStatus)
		{
			return (CertStatus)obj;
		}
		if (obj is Asn1TaggedObject)
		{
			return new CertStatus((Asn1TaggedObject)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerTaggedObject(explicitly: false, tagNo, value);
	}
}
