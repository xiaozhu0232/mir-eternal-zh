using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Crmf;

public class PkiArchiveOptions : Asn1Encodable, IAsn1Choice
{
	public const int encryptedPrivKey = 0;

	public const int keyGenParameters = 1;

	public const int archiveRemGenPrivKey = 2;

	private readonly Asn1Encodable value;

	public virtual int Type
	{
		get
		{
			if (value is EncryptedKey)
			{
				return 0;
			}
			if (value is Asn1OctetString)
			{
				return 1;
			}
			return 2;
		}
	}

	public virtual Asn1Encodable Value => value;

	public static PkiArchiveOptions GetInstance(object obj)
	{
		if (obj is PkiArchiveOptions)
		{
			return (PkiArchiveOptions)obj;
		}
		if (obj is Asn1TaggedObject)
		{
			return new PkiArchiveOptions((Asn1TaggedObject)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	private PkiArchiveOptions(Asn1TaggedObject tagged)
	{
		switch (tagged.TagNo)
		{
		case 0:
			value = EncryptedKey.GetInstance(tagged.GetObject());
			break;
		case 1:
			value = Asn1OctetString.GetInstance(tagged, isExplicit: false);
			break;
		case 2:
			value = DerBoolean.GetInstance(tagged, isExplicit: false);
			break;
		default:
			throw new ArgumentException("unknown tag number: " + tagged.TagNo, "tagged");
		}
	}

	public PkiArchiveOptions(EncryptedKey encKey)
	{
		value = encKey;
	}

	public PkiArchiveOptions(Asn1OctetString keyGenParameters)
	{
		value = keyGenParameters;
	}

	public PkiArchiveOptions(bool archiveRemGenPrivKey)
	{
		value = DerBoolean.GetInstance(archiveRemGenPrivKey);
	}

	public override Asn1Object ToAsn1Object()
	{
		if (value is EncryptedKey)
		{
			return new DerTaggedObject(explicitly: true, 0, value);
		}
		if (value is Asn1OctetString)
		{
			return new DerTaggedObject(explicitly: false, 1, value);
		}
		return new DerTaggedObject(explicitly: false, 2, value);
	}
}
