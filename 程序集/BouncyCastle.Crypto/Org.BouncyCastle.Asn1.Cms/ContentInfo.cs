using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class ContentInfo : Asn1Encodable
{
	private readonly DerObjectIdentifier contentType;

	private readonly Asn1Encodable content;

	public DerObjectIdentifier ContentType => contentType;

	public Asn1Encodable Content => content;

	public static ContentInfo GetInstance(object obj)
	{
		if (obj == null || obj is ContentInfo)
		{
			return (ContentInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new ContentInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj));
	}

	public static ContentInfo GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	private ContentInfo(Asn1Sequence seq)
	{
		if (seq.Count < 1 || seq.Count > 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		contentType = (DerObjectIdentifier)seq[0];
		if (seq.Count > 1)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[1];
			if (!asn1TaggedObject.IsExplicit() || asn1TaggedObject.TagNo != 0)
			{
				throw new ArgumentException("Bad tag for 'content'", "seq");
			}
			content = asn1TaggedObject.GetObject();
		}
	}

	public ContentInfo(DerObjectIdentifier contentType, Asn1Encodable content)
	{
		this.contentType = contentType;
		this.content = content;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(contentType);
		if (content != null)
		{
			asn1EncodableVector.Add(new BerTaggedObject(0, content));
		}
		return new BerSequence(asn1EncodableVector);
	}
}
