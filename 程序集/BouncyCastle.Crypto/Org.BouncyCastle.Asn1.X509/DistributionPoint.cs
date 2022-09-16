using System;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class DistributionPoint : Asn1Encodable
{
	internal readonly DistributionPointName distributionPoint;

	internal readonly ReasonFlags reasons;

	internal readonly GeneralNames cRLIssuer;

	public DistributionPointName DistributionPointName => distributionPoint;

	public ReasonFlags Reasons => reasons;

	public GeneralNames CrlIssuer => cRLIssuer;

	public static DistributionPoint GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static DistributionPoint GetInstance(object obj)
	{
		if (obj == null || obj is DistributionPoint)
		{
			return (DistributionPoint)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new DistributionPoint((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid DistributionPoint: " + Platform.GetTypeName(obj));
	}

	private DistributionPoint(Asn1Sequence seq)
	{
		for (int i = 0; i != seq.Count; i++)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[i]);
			switch (instance.TagNo)
			{
			case 0:
				distributionPoint = DistributionPointName.GetInstance(instance, explicitly: true);
				break;
			case 1:
				reasons = new ReasonFlags(DerBitString.GetInstance(instance, isExplicit: false));
				break;
			case 2:
				cRLIssuer = GeneralNames.GetInstance(instance, explicitly: false);
				break;
			}
		}
	}

	public DistributionPoint(DistributionPointName distributionPointName, ReasonFlags reasons, GeneralNames crlIssuer)
	{
		distributionPoint = distributionPointName;
		this.reasons = reasons;
		cRLIssuer = crlIssuer;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, distributionPoint);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, reasons);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 2, cRLIssuer);
		return new DerSequence(asn1EncodableVector);
	}

	public override string ToString()
	{
		string newLine = Platform.NewLine;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("DistributionPoint: [");
		stringBuilder.Append(newLine);
		if (distributionPoint != null)
		{
			appendObject(stringBuilder, newLine, "distributionPoint", distributionPoint.ToString());
		}
		if (reasons != null)
		{
			appendObject(stringBuilder, newLine, "reasons", reasons.ToString());
		}
		if (cRLIssuer != null)
		{
			appendObject(stringBuilder, newLine, "cRLIssuer", cRLIssuer.ToString());
		}
		stringBuilder.Append("]");
		stringBuilder.Append(newLine);
		return stringBuilder.ToString();
	}

	private void appendObject(StringBuilder buf, string sep, string name, string val)
	{
		string value = "    ";
		buf.Append(value);
		buf.Append(name);
		buf.Append(":");
		buf.Append(sep);
		buf.Append(value);
		buf.Append(value);
		buf.Append(val);
		buf.Append(sep);
	}
}
