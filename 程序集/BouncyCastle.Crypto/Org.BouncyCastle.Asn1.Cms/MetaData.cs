namespace Org.BouncyCastle.Asn1.Cms;

public class MetaData : Asn1Encodable
{
	private DerBoolean hashProtected;

	private DerUtf8String fileName;

	private DerIA5String mediaType;

	private Attributes otherMetaData;

	public virtual bool IsHashProtected => hashProtected.IsTrue;

	public virtual DerUtf8String FileName => fileName;

	public virtual DerIA5String MediaType => mediaType;

	public virtual Attributes OtherMetaData => otherMetaData;

	public MetaData(DerBoolean hashProtected, DerUtf8String fileName, DerIA5String mediaType, Attributes otherMetaData)
	{
		this.hashProtected = hashProtected;
		this.fileName = fileName;
		this.mediaType = mediaType;
		this.otherMetaData = otherMetaData;
	}

	private MetaData(Asn1Sequence seq)
	{
		hashProtected = DerBoolean.GetInstance(seq[0]);
		int num = 1;
		if (num < seq.Count && seq[num] is DerUtf8String)
		{
			fileName = DerUtf8String.GetInstance(seq[num++]);
		}
		if (num < seq.Count && seq[num] is DerIA5String)
		{
			mediaType = DerIA5String.GetInstance(seq[num++]);
		}
		if (num < seq.Count)
		{
			otherMetaData = Attributes.GetInstance(seq[num++]);
		}
	}

	public static MetaData GetInstance(object obj)
	{
		if (obj is MetaData)
		{
			return (MetaData)obj;
		}
		if (obj != null)
		{
			return new MetaData(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(hashProtected);
		asn1EncodableVector.AddOptional(fileName, mediaType, otherMetaData);
		return new DerSequence(asn1EncodableVector);
	}
}
