namespace Org.BouncyCastle.Asn1.Cms;

public class ContentInfoParser
{
	private DerObjectIdentifier contentType;

	private Asn1TaggedObjectParser content;

	public DerObjectIdentifier ContentType => contentType;

	public ContentInfoParser(Asn1SequenceParser seq)
	{
		contentType = (DerObjectIdentifier)seq.ReadObject();
		content = (Asn1TaggedObjectParser)seq.ReadObject();
	}

	public IAsn1Convertible GetContent(int tag)
	{
		if (content == null)
		{
			return null;
		}
		return content.GetObjectParser(tag, isExplicit: true);
	}
}
