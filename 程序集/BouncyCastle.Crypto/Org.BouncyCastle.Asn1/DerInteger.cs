using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerInteger : Asn1Object
{
	public const string AllowUnsafeProperty = "Org.BouncyCastle.Asn1.AllowUnsafeInteger";

	internal const int SignExtSigned = -1;

	internal const int SignExtUnsigned = 255;

	private readonly byte[] bytes;

	private readonly int start;

	public BigInteger PositiveValue => new BigInteger(1, bytes);

	public BigInteger Value => new BigInteger(bytes);

	public int IntPositiveValueExact
	{
		get
		{
			int num = bytes.Length - start;
			if (num > 4 || (num == 4 && (bytes[start] & 0x80u) != 0))
			{
				throw new ArithmeticException("ASN.1 Integer out of positive int range");
			}
			return IntValue(bytes, start, 255);
		}
	}

	public int IntValueExact
	{
		get
		{
			int num = bytes.Length - start;
			if (num > 4)
			{
				throw new ArithmeticException("ASN.1 Integer out of int range");
			}
			return IntValue(bytes, start, -1);
		}
	}

	public long LongValueExact
	{
		get
		{
			int num = bytes.Length - start;
			if (num > 8)
			{
				throw new ArithmeticException("ASN.1 Integer out of long range");
			}
			return LongValue(bytes, start, -1);
		}
	}

	internal static bool AllowUnsafe()
	{
		string environmentVariable = Platform.GetEnvironmentVariable("Org.BouncyCastle.Asn1.AllowUnsafeInteger");
		if (environmentVariable != null)
		{
			return Platform.EqualsIgnoreCase("true", environmentVariable);
		}
		return false;
	}

	public static DerInteger GetInstance(object obj)
	{
		if (obj == null || obj is DerInteger)
		{
			return (DerInteger)obj;
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
	}

	public static DerInteger GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is DerInteger)
		{
			return GetInstance(@object);
		}
		return new DerInteger(Asn1OctetString.GetInstance(@object).GetOctets());
	}

	public DerInteger(int value)
	{
		bytes = BigInteger.ValueOf(value).ToByteArray();
		start = 0;
	}

	public DerInteger(long value)
	{
		bytes = BigInteger.ValueOf(value).ToByteArray();
		start = 0;
	}

	public DerInteger(BigInteger value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		bytes = value.ToByteArray();
		start = 0;
	}

	public DerInteger(byte[] bytes)
		: this(bytes, clone: true)
	{
	}

	internal DerInteger(byte[] bytes, bool clone)
	{
		if (IsMalformed(bytes))
		{
			throw new ArgumentException("malformed integer", "bytes");
		}
		this.bytes = (clone ? Arrays.Clone(bytes) : bytes);
		start = SignBytesToSkip(bytes);
	}

	public bool HasValue(BigInteger x)
	{
		if (x != null && IntValue(bytes, start, -1) == x.IntValue)
		{
			return Value.Equals(x);
		}
		return false;
	}

	internal override void Encode(DerOutputStream derOut)
	{
		derOut.WriteEncoded(2, bytes);
	}

	protected override int Asn1GetHashCode()
	{
		return Arrays.GetHashCode(bytes);
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerInteger derInteger))
		{
			return false;
		}
		return Arrays.AreEqual(bytes, derInteger.bytes);
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	internal static int IntValue(byte[] bytes, int start, int signExt)
	{
		int num = bytes.Length;
		int num2 = System.Math.Max(start, num - 4);
		int num3 = (sbyte)bytes[num2] & signExt;
		while (++num2 < num)
		{
			num3 = (num3 << 8) | bytes[num2];
		}
		return num3;
	}

	internal static long LongValue(byte[] bytes, int start, int signExt)
	{
		int num = bytes.Length;
		int num2 = System.Math.Max(start, num - 8);
		long num3 = (sbyte)bytes[num2] & signExt;
		while (++num2 < num)
		{
			num3 = (num3 << 8) | bytes[num2];
		}
		return num3;
	}

	internal static bool IsMalformed(byte[] bytes)
	{
		switch (bytes.Length)
		{
		case 0:
			return true;
		case 1:
			return false;
		default:
			if ((sbyte)bytes[0] == (sbyte)bytes[1] >> 7)
			{
				return !AllowUnsafe();
			}
			return false;
		}
	}

	internal static int SignBytesToSkip(byte[] bytes)
	{
		int i = 0;
		for (int num = bytes.Length - 1; i < num && (sbyte)bytes[i] == (sbyte)bytes[i + 1] >> 7; i++)
		{
		}
		return i;
	}
}
