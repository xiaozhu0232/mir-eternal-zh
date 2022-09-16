using System;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1;

public abstract class Asn1OctetString : Asn1Object, Asn1OctetStringParser, IAsn1Convertible
{
	internal byte[] str;

	public Asn1OctetStringParser Parser => this;

	public static Asn1OctetString GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is Asn1OctetString)
		{
			return GetInstance(@object);
		}
		return BerOctetString.FromSequence(Asn1Sequence.GetInstance(@object));
	}

	public static Asn1OctetString GetInstance(object obj)
	{
		if (obj == null || obj is Asn1OctetString)
		{
			return (Asn1OctetString)obj;
		}
		if (obj is byte[])
		{
			try
			{
				return GetInstance(Asn1Object.FromByteArray((byte[])obj));
			}
			catch (IOException ex)
			{
				throw new ArgumentException("failed to construct OCTET STRING from byte[]: " + ex.Message);
			}
		}
		if (obj is Asn1TaggedObject)
		{
			return GetInstance(((Asn1TaggedObject)obj).GetObject());
		}
		if (obj is Asn1Encodable)
		{
			Asn1Object asn1Object = ((Asn1Encodable)obj).ToAsn1Object();
			if (asn1Object is Asn1OctetString)
			{
				return (Asn1OctetString)asn1Object;
			}
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
	}

	internal Asn1OctetString(byte[] str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		this.str = str;
	}

	public Stream GetOctetStream()
	{
		return new MemoryStream(str, writable: false);
	}

	public virtual byte[] GetOctets()
	{
		return str;
	}

	protected override int Asn1GetHashCode()
	{
		return Arrays.GetHashCode(GetOctets());
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerOctetString derOctetString))
		{
			return false;
		}
		return Arrays.AreEqual(GetOctets(), derOctetString.GetOctets());
	}

	public override string ToString()
	{
		return "#" + Hex.ToHexString(str);
	}
}
