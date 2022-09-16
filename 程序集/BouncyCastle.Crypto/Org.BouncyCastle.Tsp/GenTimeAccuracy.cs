using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Tsp;

namespace Org.BouncyCastle.Tsp;

public class GenTimeAccuracy
{
	private Accuracy accuracy;

	public int Seconds => GetTimeComponent(accuracy.Seconds);

	public int Millis => GetTimeComponent(accuracy.Millis);

	public int Micros => GetTimeComponent(accuracy.Micros);

	public GenTimeAccuracy(Accuracy accuracy)
	{
		this.accuracy = accuracy;
	}

	private int GetTimeComponent(DerInteger time)
	{
		return time?.IntValueExact ?? 0;
	}

	public override string ToString()
	{
		return Seconds + "." + Millis.ToString("000") + Micros.ToString("000");
	}
}
