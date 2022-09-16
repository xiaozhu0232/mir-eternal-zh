using System;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerUtf8String : DerStringBase
{
	private readonly string str;

	public static DerUtf8String GetInstance(object obj)
	{
		if (obj == null || obj is DerUtf8String)
		{
			return (DerUtf8String)obj;
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
	}

	public static DerUtf8String GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is DerUtf8String)
		{
			return GetInstance(@object);
		}
		return new DerUtf8String(Asn1OctetString.GetInstance(@object).GetOctets());
	}

	public DerUtf8String(byte[] str)
		: this(Encoding.UTF8.GetString(str, 0, str.Length))
	{
	}

	public DerUtf8String(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		this.str = str;
	}

	public override string GetString()
	{
		return str;
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerUtf8String derUtf8String))
		{
			return false;
		}
		return str.Equals(derUtf8String.str);
	}

	internal override void Encode(DerOutputStream derOut)
	{
		derOut.WriteEncoded(12, Encoding.UTF8.GetBytes(str));
	}
}
