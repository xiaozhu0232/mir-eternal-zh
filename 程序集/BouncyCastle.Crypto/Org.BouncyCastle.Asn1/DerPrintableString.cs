using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerPrintableString : DerStringBase
{
	private readonly string str;

	public static DerPrintableString GetInstance(object obj)
	{
		if (obj == null || obj is DerPrintableString)
		{
			return (DerPrintableString)obj;
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
	}

	public static DerPrintableString GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is DerPrintableString)
		{
			return GetInstance(@object);
		}
		return new DerPrintableString(Asn1OctetString.GetInstance(@object).GetOctets());
	}

	public DerPrintableString(byte[] str)
		: this(Strings.FromAsciiByteArray(str), validate: false)
	{
	}

	public DerPrintableString(string str)
		: this(str, validate: false)
	{
	}

	public DerPrintableString(string str, bool validate)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (validate && !IsPrintableString(str))
		{
			throw new ArgumentException("string contains illegal characters", "str");
		}
		this.str = str;
	}

	public override string GetString()
	{
		return str;
	}

	public byte[] GetOctets()
	{
		return Strings.ToAsciiByteArray(str);
	}

	internal override void Encode(DerOutputStream derOut)
	{
		derOut.WriteEncoded(19, GetOctets());
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerPrintableString derPrintableString))
		{
			return false;
		}
		return str.Equals(derPrintableString.str);
	}

	public static bool IsPrintableString(string str)
	{
		foreach (char c in str)
		{
			if (c > '\u007f')
			{
				return false;
			}
			if (!char.IsLetterOrDigit(c))
			{
				switch (c)
				{
				case ' ':
				case '\'':
				case '(':
				case ')':
				case '+':
				case ',':
				case '-':
				case '.':
				case '/':
				case ':':
				case '=':
				case '?':
					continue;
				}
				return false;
			}
		}
		return true;
	}
}
