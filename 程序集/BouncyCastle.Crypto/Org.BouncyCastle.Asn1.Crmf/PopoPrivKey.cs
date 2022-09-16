using System;
using Org.BouncyCastle.Asn1.Cms;

namespace Org.BouncyCastle.Asn1.Crmf;

public class PopoPrivKey : Asn1Encodable, IAsn1Choice
{
	public const int thisMessage = 0;

	public const int subsequentMessage = 1;

	public const int dhMAC = 2;

	public const int agreeMAC = 3;

	public const int encryptedKey = 4;

	private readonly int tagNo;

	private readonly Asn1Encodable obj;

	public virtual int Type => tagNo;

	public virtual Asn1Encodable Value => obj;

	private PopoPrivKey(Asn1TaggedObject obj)
	{
		tagNo = obj.TagNo;
		switch (tagNo)
		{
		case 0:
			this.obj = DerBitString.GetInstance(obj, isExplicit: false);
			break;
		case 1:
			this.obj = SubsequentMessage.ValueOf(DerInteger.GetInstance(obj, isExplicit: false).IntValueExact);
			break;
		case 2:
			this.obj = DerBitString.GetInstance(obj, isExplicit: false);
			break;
		case 3:
			this.obj = PKMacValue.GetInstance(obj, isExplicit: false);
			break;
		case 4:
			this.obj = EnvelopedData.GetInstance(obj, explicitly: false);
			break;
		default:
			throw new ArgumentException("unknown tag in PopoPrivKey", "obj");
		}
	}

	public static PopoPrivKey GetInstance(Asn1TaggedObject tagged, bool isExplicit)
	{
		return new PopoPrivKey(Asn1TaggedObject.GetInstance(tagged.GetObject()));
	}

	public PopoPrivKey(SubsequentMessage msg)
	{
		tagNo = 1;
		obj = msg;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerTaggedObject(explicitly: false, tagNo, obj);
	}
}
