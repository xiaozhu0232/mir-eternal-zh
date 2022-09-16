using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1.X500.Style;

public abstract class IetfUtilities
{
	public static string ValueToString(Asn1Encodable value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (value is IAsn1String && !(value is DerUniversalString))
		{
			string @string = ((IAsn1String)value).GetString();
			if (@string.Length > 0 && @string[0] == '#')
			{
				stringBuilder.Append('\\');
			}
			stringBuilder.Append(@string);
		}
		else
		{
			try
			{
				stringBuilder.Append('#');
				stringBuilder.Append(Hex.ToHexString(value.ToAsn1Object().GetEncoded("DER")));
			}
			catch (IOException innerException)
			{
				throw new ArgumentException("Other value has no encoded form", innerException);
			}
		}
		int num = stringBuilder.Length;
		int num2 = 0;
		if (stringBuilder.Length >= 2 && stringBuilder[0] == '\\' && stringBuilder[1] == '#')
		{
			num2 += 2;
		}
		while (num2 != num)
		{
			switch (stringBuilder[num2])
			{
			case '"':
			case '+':
			case ',':
			case ';':
			case '<':
			case '=':
			case '>':
			case '\\':
				stringBuilder.Insert(num2, "\\");
				num2 += 2;
				num++;
				break;
			default:
				num2++;
				break;
			}
		}
		int i = 0;
		if (stringBuilder.Length > 0)
		{
			for (; stringBuilder.Length > i && stringBuilder[i] == ' '; i += 2)
			{
				stringBuilder.Insert(i, "\\");
			}
		}
		int num3 = stringBuilder.Length - 1;
		while (num3 >= 0 && stringBuilder[num3] == ' ')
		{
			stringBuilder.Insert(num3, "\\");
			num3--;
		}
		return stringBuilder.ToString();
	}

	public static string Canonicalize(string s)
	{
		string text = Platform.ToLowerInvariant(s);
		if (text.Length > 0 && text[0] == '#')
		{
			Asn1Object asn1Object = DecodeObject(text);
			if (asn1Object is IAsn1String)
			{
				text = Platform.ToLowerInvariant(((IAsn1String)asn1Object).GetString());
			}
		}
		if (text.Length > 1)
		{
			int i;
			for (i = 0; i + 1 < text.Length && text[i] == '\\' && text[i + 1] == ' '; i += 2)
			{
			}
			int num = text.Length - 1;
			while (num - 1 > 0 && text[num - 1] == '\\' && text[num] == ' ')
			{
				num -= 2;
			}
			if (i > 0 || num < text.Length - 1)
			{
				text = text.Substring(i, num + 1 - i);
			}
		}
		return StripInternalSpaces(text);
	}

	public static string CanonicalString(Asn1Encodable value)
	{
		return Canonicalize(ValueToString(value));
	}

	private static Asn1Object DecodeObject(string oValue)
	{
		try
		{
			return Asn1Object.FromByteArray(Hex.DecodeStrict(oValue, 1, oValue.Length - 1));
		}
		catch (IOException ex)
		{
			throw new InvalidOperationException("unknown encoding in name: " + ex);
		}
	}

	public static string StripInternalSpaces(string str)
	{
		if (str.IndexOf("  ") < 0)
		{
			return str;
		}
		StringBuilder stringBuilder = new StringBuilder();
		char c = str[0];
		stringBuilder.Append(c);
		for (int i = 1; i < str.Length; i++)
		{
			char c2 = str[i];
			if (' ' != c || ' ' != c2)
			{
				stringBuilder.Append(c2);
				c = c2;
			}
		}
		return stringBuilder.ToString();
	}

	public static bool RdnAreEqual(Rdn rdn1, Rdn rdn2)
	{
		if (rdn1.Count != rdn2.Count)
		{
			return false;
		}
		AttributeTypeAndValue[] typesAndValues = rdn1.GetTypesAndValues();
		AttributeTypeAndValue[] typesAndValues2 = rdn2.GetTypesAndValues();
		if (typesAndValues.Length != typesAndValues2.Length)
		{
			return false;
		}
		for (int i = 0; i != typesAndValues.Length; i++)
		{
			if (!AtvAreEqual(typesAndValues[i], typesAndValues2[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool AtvAreEqual(AttributeTypeAndValue atv1, AttributeTypeAndValue atv2)
	{
		if (atv1 == atv2)
		{
			return true;
		}
		if (atv1 == null || atv2 == null)
		{
			return false;
		}
		DerObjectIdentifier type = atv1.Type;
		DerObjectIdentifier type2 = atv2.Type;
		if (!type.Equals(type2))
		{
			return false;
		}
		string text = CanonicalString(atv1.Value);
		string value = CanonicalString(atv2.Value);
		if (!text.Equals(value))
		{
			return false;
		}
		return true;
	}
}
