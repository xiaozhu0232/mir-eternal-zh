using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Crmf;

public class ProofOfPossession : Asn1Encodable, IAsn1Choice
{
	public const int TYPE_RA_VERIFIED = 0;

	public const int TYPE_SIGNING_KEY = 1;

	public const int TYPE_KEY_ENCIPHERMENT = 2;

	public const int TYPE_KEY_AGREEMENT = 3;

	private readonly int tagNo;

	private readonly Asn1Encodable obj;

	public virtual int Type => tagNo;

	public virtual Asn1Encodable Object => obj;

	private ProofOfPossession(Asn1TaggedObject tagged)
	{
		tagNo = tagged.TagNo;
		switch (tagNo)
		{
		case 0:
			obj = DerNull.Instance;
			break;
		case 1:
			obj = PopoSigningKey.GetInstance(tagged, isExplicit: false);
			break;
		case 2:
		case 3:
			obj = PopoPrivKey.GetInstance(tagged, isExplicit: false);
			break;
		default:
			throw new ArgumentException("unknown tag: " + tagNo, "tagged");
		}
	}

	public static ProofOfPossession GetInstance(object obj)
	{
		if (obj is ProofOfPossession)
		{
			return (ProofOfPossession)obj;
		}
		if (obj is Asn1TaggedObject)
		{
			return new ProofOfPossession((Asn1TaggedObject)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public ProofOfPossession()
	{
		tagNo = 0;
		obj = DerNull.Instance;
	}

	public ProofOfPossession(PopoSigningKey Poposk)
	{
		tagNo = 1;
		obj = Poposk;
	}

	public ProofOfPossession(int type, PopoPrivKey privkey)
	{
		tagNo = type;
		obj = privkey;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerTaggedObject(explicitly: false, tagNo, obj);
	}
}
