using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class CrlDistPoint : Asn1Encodable
{
	internal readonly Asn1Sequence seq;

	public static CrlDistPoint GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static CrlDistPoint GetInstance(object obj)
	{
		if (obj is CrlDistPoint)
		{
			return (CrlDistPoint)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new CrlDistPoint(Asn1Sequence.GetInstance(obj));
	}

	public static CrlDistPoint FromExtensions(X509Extensions extensions)
	{
		return GetInstance(X509Extensions.GetExtensionParsedValue(extensions, X509Extensions.CrlDistributionPoints));
	}

	private CrlDistPoint(Asn1Sequence seq)
	{
		this.seq = seq;
	}

	public CrlDistPoint(DistributionPoint[] points)
	{
		seq = new DerSequence(points);
	}

	public DistributionPoint[] GetDistributionPoints()
	{
		DistributionPoint[] array = new DistributionPoint[seq.Count];
		for (int i = 0; i != seq.Count; i++)
		{
			array[i] = DistributionPoint.GetInstance(seq[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return seq;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string newLine = Platform.NewLine;
		stringBuilder.Append("CRLDistPoint:");
		stringBuilder.Append(newLine);
		DistributionPoint[] distributionPoints = GetDistributionPoints();
		for (int i = 0; i != distributionPoints.Length; i++)
		{
			stringBuilder.Append("    ");
			stringBuilder.Append(distributionPoints[i]);
			stringBuilder.Append(newLine);
		}
		return stringBuilder.ToString();
	}
}
