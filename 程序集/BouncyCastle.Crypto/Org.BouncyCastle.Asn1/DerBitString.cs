using System;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerBitString : DerStringBase
{
	private static readonly char[] table = new char[16]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F'
	};

	protected readonly byte[] mData;

	protected readonly int mPadBits;

	public virtual int PadBits => mPadBits;

	public virtual int IntValue
	{
		get
		{
			int num = 0;
			int num2 = System.Math.Min(4, mData.Length);
			for (int i = 0; i < num2; i++)
			{
				num |= mData[i] << 8 * i;
			}
			if (mPadBits > 0 && num2 == mData.Length)
			{
				int num3 = (1 << mPadBits) - 1;
				num &= ~(num3 << 8 * (num2 - 1));
			}
			return num;
		}
	}

	public static DerBitString GetInstance(object obj)
	{
		if (obj == null || obj is DerBitString)
		{
			return (DerBitString)obj;
		}
		if (obj is byte[])
		{
			try
			{
				return (DerBitString)Asn1Object.FromByteArray((byte[])obj);
			}
			catch (Exception ex)
			{
				throw new ArgumentException("encoding error in GetInstance: " + ex.ToString());
			}
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
	}

	public static DerBitString GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is DerBitString)
		{
			return GetInstance(@object);
		}
		return FromAsn1Octets(((Asn1OctetString)@object).GetOctets());
	}

	public DerBitString(byte[] data, int padBits)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (padBits < 0 || padBits > 7)
		{
			throw new ArgumentException("must be in the range 0 to 7", "padBits");
		}
		if (data.Length == 0 && padBits != 0)
		{
			throw new ArgumentException("if 'data' is empty, 'padBits' must be 0");
		}
		mData = Arrays.Clone(data);
		mPadBits = padBits;
	}

	public DerBitString(byte[] data)
		: this(data, 0)
	{
	}

	public DerBitString(int namedBits)
	{
		if (namedBits == 0)
		{
			mData = new byte[0];
			mPadBits = 0;
			return;
		}
		int num = BigInteger.BitLen(namedBits);
		int num2 = (num + 7) / 8;
		byte[] array = new byte[num2];
		num2--;
		for (int i = 0; i < num2; i++)
		{
			array[i] = (byte)namedBits;
			namedBits >>= 8;
		}
		array[num2] = (byte)namedBits;
		int j;
		for (j = 0; (namedBits & (1 << j)) == 0; j++)
		{
		}
		mData = array;
		mPadBits = j;
	}

	public DerBitString(Asn1Encodable obj)
		: this(obj.GetDerEncoded())
	{
	}

	public virtual byte[] GetOctets()
	{
		if (mPadBits != 0)
		{
			throw new InvalidOperationException("attempt to get non-octet aligned data from BIT STRING");
		}
		return Arrays.Clone(mData);
	}

	public virtual byte[] GetBytes()
	{
		byte[] array = Arrays.Clone(mData);
		if (mPadBits > 0)
		{
			byte[] array2;
			byte[] array3 = (array2 = array);
			int num = array.Length - 1;
			nint num2 = num;
			array3[num] = (byte)(array2[num2] & (byte)(255 << mPadBits));
		}
		return array;
	}

	internal override void Encode(DerOutputStream derOut)
	{
		if (mPadBits > 0)
		{
			int num = mData[mData.Length - 1];
			int num2 = (1 << mPadBits) - 1;
			int num3 = num & num2;
			if (num3 != 0)
			{
				byte[] array = Arrays.Prepend(mData, (byte)mPadBits);
				array[array.Length - 1] = (byte)(num ^ num3);
				derOut.WriteEncoded(3, array);
				return;
			}
		}
		derOut.WriteEncoded(3, (byte)mPadBits, mData);
	}

	protected override int Asn1GetHashCode()
	{
		return mPadBits.GetHashCode() ^ Arrays.GetHashCode(mData);
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerBitString derBitString))
		{
			return false;
		}
		if (mPadBits == derBitString.mPadBits)
		{
			return Arrays.AreEqual(mData, derBitString.mData);
		}
		return false;
	}

	public override string GetString()
	{
		StringBuilder stringBuilder = new StringBuilder("#");
		byte[] derEncoded = GetDerEncoded();
		for (int i = 0; i != derEncoded.Length; i++)
		{
			uint num = derEncoded[i];
			stringBuilder.Append(table[(num >> 4) & 0xF]);
			stringBuilder.Append(table[derEncoded[i] & 0xF]);
		}
		return stringBuilder.ToString();
	}

	internal static DerBitString FromAsn1Octets(byte[] octets)
	{
		if (octets.Length < 1)
		{
			throw new ArgumentException("truncated BIT STRING detected", "octets");
		}
		int num = octets[0];
		byte[] array = Arrays.CopyOfRange(octets, 1, octets.Length);
		if (num > 0 && num < 8 && array.Length > 0)
		{
			int num2 = array[array.Length - 1];
			int num3 = (1 << num) - 1;
			if ((num2 & num3) != 0)
			{
				return new BerBitString(array, num);
			}
		}
		return new DerBitString(array, num);
	}
}
