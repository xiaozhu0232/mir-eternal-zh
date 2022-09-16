using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerIA5String : DerStringBase
{
	private readonly string str;

	public static DerIA5String GetInstance(object obj)
	{
		if (obj == null || obj is DerIA5String)
		{
			return (DerIA5String)obj;
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
	}

	public static DerIA5String GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is DerIA5String)
		{
			return GetInstance(@object);
		}
		return new DerIA5String(((Asn1OctetString)@object).GetOctets());
	}

	public DerIA5String(byte[] str)
		: this(Strings.FromAsciiByteArray(str), validate: false)
	{
	}

	public DerIA5String(string str)
		: this(str, validate: false)
	{
	}

	public DerIA5String(string str, bool validate)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (validate && !IsIA5String(str))
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
		derOut.WriteEncoded(22, GetOctets());
	}

	protected override int Asn1GetHashCode()
	{
		return str.GetHashCode();
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerIA5String derIA5String))
		{
			return false;
		}
		return str.Equals(derIA5String.str);
	}

	public static bool IsIA5String(string str)
	{
		foreach (char c in str)
		{
			if (c > '\u007f')
			{
				return false;
			}
		}
		return true;
	}
}
