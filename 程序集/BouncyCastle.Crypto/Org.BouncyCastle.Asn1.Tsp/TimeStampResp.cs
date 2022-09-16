using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Cms;

namespace Org.BouncyCastle.Asn1.Tsp;

public class TimeStampResp : Asn1Encodable
{
	private readonly PkiStatusInfo pkiStatusInfo;

	private readonly ContentInfo timeStampToken;

	public PkiStatusInfo Status => pkiStatusInfo;

	public ContentInfo TimeStampToken => timeStampToken;

	public static TimeStampResp GetInstance(object obj)
	{
		if (obj is TimeStampResp)
		{
			return (TimeStampResp)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new TimeStampResp(Asn1Sequence.GetInstance(obj));
	}

	private TimeStampResp(Asn1Sequence seq)
	{
		pkiStatusInfo = PkiStatusInfo.GetInstance(seq[0]);
		if (seq.Count > 1)
		{
			timeStampToken = ContentInfo.GetInstance(seq[1]);
		}
	}

	public TimeStampResp(PkiStatusInfo pkiStatusInfo, ContentInfo timeStampToken)
	{
		this.pkiStatusInfo = pkiStatusInfo;
		this.timeStampToken = timeStampToken;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(pkiStatusInfo);
		asn1EncodableVector.AddOptional(timeStampToken);
		return new DerSequence(asn1EncodableVector);
	}
}
