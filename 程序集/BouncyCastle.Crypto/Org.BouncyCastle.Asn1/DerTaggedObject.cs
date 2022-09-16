namespace Org.BouncyCastle.Asn1;

public class DerTaggedObject : Asn1TaggedObject
{
	public DerTaggedObject(int tagNo, Asn1Encodable obj)
		: base(tagNo, obj)
	{
	}

	public DerTaggedObject(bool explicitly, int tagNo, Asn1Encodable obj)
		: base(explicitly, tagNo, obj)
	{
	}

	public DerTaggedObject(int tagNo)
		: base(explicitly: false, tagNo, DerSequence.Empty)
	{
	}

	internal override void Encode(DerOutputStream derOut)
	{
		if (!IsEmpty())
		{
			byte[] derEncoded = obj.GetDerEncoded();
			if (explicitly)
			{
				derOut.WriteEncoded(160, tagNo, derEncoded);
				return;
			}
			int flags = (derEncoded[0] & 0x20) | 0x80;
			derOut.WriteTag(flags, tagNo);
			derOut.Write(derEncoded, 1, derEncoded.Length - 1);
		}
		else
		{
			derOut.WriteEncoded(160, tagNo, new byte[0]);
		}
	}
}
