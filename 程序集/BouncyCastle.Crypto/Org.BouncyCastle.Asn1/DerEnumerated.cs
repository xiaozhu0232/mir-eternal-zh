using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerEnumerated : Asn1Object
{
	private readonly byte[] bytes;

	private readonly int start;

	private static readonly DerEnumerated[] cache = new DerEnumerated[12];

	public BigInteger Value => new BigInteger(bytes);

	public int IntValueExact
	{
		get
		{
			int num = bytes.Length - start;
			if (num > 4)
			{
				throw new ArithmeticException("ASN.1 Enumerated out of int range");
			}
			return DerInteger.IntValue(bytes, start, -1);
		}
	}

	public static DerEnumerated GetInstance(object obj)
	{
		if (obj == null || obj is DerEnumerated)
		{
			return (DerEnumerated)obj;
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
	}

	public static DerEnumerated GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is DerEnumerated)
		{
			return GetInstance(@object);
		}
		return FromOctetString(((Asn1OctetString)@object).GetOctets());
	}

	public DerEnumerated(int val)
	{
		if (val < 0)
		{
			throw new ArgumentException("enumerated must be non-negative", "val");
		}
		bytes = BigInteger.ValueOf(val).ToByteArray();
		start = 0;
	}

	public DerEnumerated(long val)
	{
		if (val < 0)
		{
			throw new ArgumentException("enumerated must be non-negative", "val");
		}
		bytes = BigInteger.ValueOf(val).ToByteArray();
		start = 0;
	}

	public DerEnumerated(BigInteger val)
	{
		if (val.SignValue < 0)
		{
			throw new ArgumentException("enumerated must be non-negative", "val");
		}
		bytes = val.ToByteArray();
		start = 0;
	}

	public DerEnumerated(byte[] bytes)
	{
		if (DerInteger.IsMalformed(bytes))
		{
			throw new ArgumentException("malformed enumerated", "bytes");
		}
		if ((bytes[0] & 0x80u) != 0)
		{
			throw new ArgumentException("enumerated must be non-negative", "bytes");
		}
		this.bytes = Arrays.Clone(bytes);
		start = DerInteger.SignBytesToSkip(bytes);
	}

	public bool HasValue(BigInteger x)
	{
		if (x != null && DerInteger.IntValue(bytes, start, -1) == x.IntValue)
		{
			return Value.Equals(x);
		}
		return false;
	}

	internal override void Encode(DerOutputStream derOut)
	{
		derOut.WriteEncoded(10, bytes);
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerEnumerated derEnumerated))
		{
			return false;
		}
		return Arrays.AreEqual(bytes, derEnumerated.bytes);
	}

	protected override int Asn1GetHashCode()
	{
		return Arrays.GetHashCode(bytes);
	}

	internal static DerEnumerated FromOctetString(byte[] enc)
	{
		if (enc.Length > 1)
		{
			return new DerEnumerated(enc);
		}
		if (enc.Length == 0)
		{
			throw new ArgumentException("ENUMERATED has zero length", "enc");
		}
		int num = enc[0];
		if (num >= cache.Length)
		{
			return new DerEnumerated(enc);
		}
		DerEnumerated derEnumerated = cache[num];
		if (derEnumerated == null)
		{
			derEnumerated = (cache[num] = new DerEnumerated(enc));
		}
		return derEnumerated;
	}
}
