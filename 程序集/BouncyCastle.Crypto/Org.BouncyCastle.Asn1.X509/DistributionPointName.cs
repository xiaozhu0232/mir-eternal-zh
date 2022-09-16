using System;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class DistributionPointName : Asn1Encodable, IAsn1Choice
{
	public const int FullName = 0;

	public const int NameRelativeToCrlIssuer = 1;

	internal readonly Asn1Encodable name;

	internal readonly int type;

	public int PointType => type;

	public Asn1Encodable Name => name;

	public static DistributionPointName GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1TaggedObject.GetInstance(obj, explicitly: true));
	}

	public static DistributionPointName GetInstance(object obj)
	{
		if (obj == null || obj is DistributionPointName)
		{
			return (DistributionPointName)obj;
		}
		if (obj is Asn1TaggedObject)
		{
			return new DistributionPointName((Asn1TaggedObject)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public DistributionPointName(int type, Asn1Encodable name)
	{
		this.type = type;
		this.name = name;
	}

	public DistributionPointName(GeneralNames name)
		: this(0, name)
	{
	}

	public DistributionPointName(Asn1TaggedObject obj)
	{
		type = obj.TagNo;
		if (type == 0)
		{
			name = GeneralNames.GetInstance(obj, explicitly: false);
		}
		else
		{
			name = Asn1Set.GetInstance(obj, explicitly: false);
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerTaggedObject(explicitly: false, type, name);
	}

	public override string ToString()
	{
		string newLine = Platform.NewLine;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("DistributionPointName: [");
		stringBuilder.Append(newLine);
		if (type == 0)
		{
			appendObject(stringBuilder, newLine, "fullName", name.ToString());
		}
		else
		{
			appendObject(stringBuilder, newLine, "nameRelativeToCRLIssuer", name.ToString());
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
