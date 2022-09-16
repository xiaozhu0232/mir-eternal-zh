using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerGraphicString : DerStringBase
{
	private readonly byte[] mString;

	public static DerGraphicString GetInstance(object obj)
	{
		if (obj == null || obj is DerGraphicString)
		{
			return (DerGraphicString)obj;
		}
		if (obj is byte[])
		{
			try
			{
				return (DerGraphicString)Asn1Object.FromByteArray((byte[])obj);
			}
			catch (Exception ex)
			{
				throw new ArgumentException("encoding error in GetInstance: " + ex.ToString(), "obj");
			}
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	public static DerGraphicString GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is DerGraphicString)
		{
			return GetInstance(@object);
		}
		return new DerGraphicString(((Asn1OctetString)@object).GetOctets());
	}

	public DerGraphicString(byte[] encoding)
	{
		mString = Arrays.Clone(encoding);
	}

	public override string GetString()
	{
		return Strings.FromByteArray(mString);
	}

	public byte[] GetOctets()
	{
		return Arrays.Clone(mString);
	}

	internal override void Encode(DerOutputStream derOut)
	{
		derOut.WriteEncoded(25, mString);
	}

	protected override int Asn1GetHashCode()
	{
		return Arrays.GetHashCode(mString);
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerGraphicString derGraphicString))
		{
			return false;
		}
		return Arrays.AreEqual(mString, derGraphicString.mString);
	}
}
