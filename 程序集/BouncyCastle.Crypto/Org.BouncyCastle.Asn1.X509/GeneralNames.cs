using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class GeneralNames : Asn1Encodable
{
	private readonly GeneralName[] names;

	private static GeneralName[] Copy(GeneralName[] names)
	{
		return (GeneralName[])names.Clone();
	}

	public static GeneralNames GetInstance(object obj)
	{
		if (obj is GeneralNames)
		{
			return (GeneralNames)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new GeneralNames(Asn1Sequence.GetInstance(obj));
	}

	public static GeneralNames GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static GeneralNames FromExtensions(X509Extensions extensions, DerObjectIdentifier extOid)
	{
		return GetInstance(X509Extensions.GetExtensionParsedValue(extensions, extOid));
	}

	public GeneralNames(GeneralName name)
	{
		names = new GeneralName[1] { name };
	}

	public GeneralNames(GeneralName[] names)
	{
		this.names = Copy(names);
	}

	private GeneralNames(Asn1Sequence seq)
	{
		names = new GeneralName[seq.Count];
		for (int i = 0; i != seq.Count; i++)
		{
			names[i] = GeneralName.GetInstance(seq[i]);
		}
	}

	public GeneralName[] GetNames()
	{
		return Copy(names);
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(names);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string newLine = Platform.NewLine;
		stringBuilder.Append("GeneralNames:");
		stringBuilder.Append(newLine);
		GeneralName[] array = names;
		foreach (GeneralName value in array)
		{
			stringBuilder.Append("    ");
			stringBuilder.Append(value);
			stringBuilder.Append(newLine);
		}
		return stringBuilder.ToString();
	}
}
