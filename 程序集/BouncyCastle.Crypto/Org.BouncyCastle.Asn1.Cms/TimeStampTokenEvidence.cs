namespace Org.BouncyCastle.Asn1.Cms;

public class TimeStampTokenEvidence : Asn1Encodable
{
	private TimeStampAndCrl[] timeStampAndCrls;

	public TimeStampTokenEvidence(TimeStampAndCrl[] timeStampAndCrls)
	{
		this.timeStampAndCrls = timeStampAndCrls;
	}

	public TimeStampTokenEvidence(TimeStampAndCrl timeStampAndCrl)
	{
		timeStampAndCrls = new TimeStampAndCrl[1] { timeStampAndCrl };
	}

	private TimeStampTokenEvidence(Asn1Sequence seq)
	{
		timeStampAndCrls = new TimeStampAndCrl[seq.Count];
		int num = 0;
		foreach (Asn1Encodable item in seq)
		{
			timeStampAndCrls[num++] = TimeStampAndCrl.GetInstance(item.ToAsn1Object());
		}
	}

	public static TimeStampTokenEvidence GetInstance(Asn1TaggedObject tagged, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(tagged, isExplicit));
	}

	public static TimeStampTokenEvidence GetInstance(object obj)
	{
		if (obj is TimeStampTokenEvidence)
		{
			return (TimeStampTokenEvidence)obj;
		}
		if (obj != null)
		{
			return new TimeStampTokenEvidence(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public virtual TimeStampAndCrl[] ToTimeStampAndCrlArray()
	{
		return (TimeStampAndCrl[])timeStampAndCrls.Clone();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(timeStampAndCrls);
	}
}
