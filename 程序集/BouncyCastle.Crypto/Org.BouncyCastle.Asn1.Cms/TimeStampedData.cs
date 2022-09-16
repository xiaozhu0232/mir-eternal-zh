namespace Org.BouncyCastle.Asn1.Cms;

public class TimeStampedData : Asn1Encodable
{
	private DerInteger version;

	private DerIA5String dataUri;

	private MetaData metaData;

	private Asn1OctetString content;

	private Evidence temporalEvidence;

	public virtual DerIA5String DataUri => dataUri;

	public MetaData MetaData => metaData;

	public Asn1OctetString Content => content;

	public Evidence TemporalEvidence => temporalEvidence;

	public TimeStampedData(DerIA5String dataUri, MetaData metaData, Asn1OctetString content, Evidence temporalEvidence)
	{
		version = new DerInteger(1);
		this.dataUri = dataUri;
		this.metaData = metaData;
		this.content = content;
		this.temporalEvidence = temporalEvidence;
	}

	private TimeStampedData(Asn1Sequence seq)
	{
		version = DerInteger.GetInstance(seq[0]);
		int index = 1;
		if (seq[index] is DerIA5String)
		{
			dataUri = DerIA5String.GetInstance(seq[index++]);
		}
		if (seq[index] is MetaData || seq[index] is Asn1Sequence)
		{
			metaData = MetaData.GetInstance(seq[index++]);
		}
		if (seq[index] is Asn1OctetString)
		{
			content = Asn1OctetString.GetInstance(seq[index++]);
		}
		temporalEvidence = Evidence.GetInstance(seq[index]);
	}

	public static TimeStampedData GetInstance(object obj)
	{
		if (obj is TimeStampedData)
		{
			return (TimeStampedData)obj;
		}
		if (obj != null)
		{
			return new TimeStampedData(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version);
		asn1EncodableVector.AddOptional(dataUri, metaData, content);
		asn1EncodableVector.Add(temporalEvidence);
		return new BerSequence(asn1EncodableVector);
	}
}
