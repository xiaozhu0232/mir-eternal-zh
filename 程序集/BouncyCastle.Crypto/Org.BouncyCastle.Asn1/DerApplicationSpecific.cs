using System;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerApplicationSpecific : Asn1Object
{
	private readonly bool isConstructed;

	private readonly int tag;

	private readonly byte[] octets;

	public int ApplicationTag => tag;

	internal DerApplicationSpecific(bool isConstructed, int tag, byte[] octets)
	{
		this.isConstructed = isConstructed;
		this.tag = tag;
		this.octets = octets;
	}

	public DerApplicationSpecific(int tag, byte[] octets)
		: this(isConstructed: false, tag, octets)
	{
	}

	public DerApplicationSpecific(int tag, Asn1Encodable obj)
		: this(isExplicit: true, tag, obj)
	{
	}

	public DerApplicationSpecific(bool isExplicit, int tag, Asn1Encodable obj)
	{
		Asn1Object asn1Object = obj.ToAsn1Object();
		byte[] derEncoded = asn1Object.GetDerEncoded();
		isConstructed = Asn1TaggedObject.IsConstructed(isExplicit, asn1Object);
		this.tag = tag;
		if (isExplicit)
		{
			octets = derEncoded;
			return;
		}
		int lengthOfHeader = GetLengthOfHeader(derEncoded);
		byte[] array = new byte[derEncoded.Length - lengthOfHeader];
		Array.Copy(derEncoded, lengthOfHeader, array, 0, array.Length);
		octets = array;
	}

	public DerApplicationSpecific(int tagNo, Asn1EncodableVector vec)
	{
		tag = tagNo;
		isConstructed = true;
		MemoryStream memoryStream = new MemoryStream();
		for (int i = 0; i != vec.Count; i++)
		{
			try
			{
				byte[] derEncoded = vec[i].GetDerEncoded();
				memoryStream.Write(derEncoded, 0, derEncoded.Length);
			}
			catch (IOException innerException)
			{
				throw new InvalidOperationException("malformed object", innerException);
			}
		}
		octets = memoryStream.ToArray();
	}

	private int GetLengthOfHeader(byte[] data)
	{
		int num = data[1];
		if (num == 128)
		{
			return 2;
		}
		if (num > 127)
		{
			int num2 = num & 0x7F;
			if (num2 > 4)
			{
				throw new InvalidOperationException("DER length more than 4 bytes: " + num2);
			}
			return num2 + 2;
		}
		return 2;
	}

	public bool IsConstructed()
	{
		return isConstructed;
	}

	public byte[] GetContents()
	{
		return octets;
	}

	public Asn1Object GetObject()
	{
		return Asn1Object.FromByteArray(GetContents());
	}

	public Asn1Object GetObject(int derTagNo)
	{
		if (derTagNo >= 31)
		{
			throw new IOException("unsupported tag number");
		}
		byte[] encoded = GetEncoded();
		byte[] array = ReplaceTagNumber(derTagNo, encoded);
		if ((encoded[0] & 0x20u) != 0)
		{
			byte[] array2;
			(array2 = array)[0] = (byte)(array2[0] | 0x20u);
		}
		return Asn1Object.FromByteArray(array);
	}

	internal override void Encode(DerOutputStream derOut)
	{
		int num = 64;
		if (isConstructed)
		{
			num |= 0x20;
		}
		derOut.WriteEncoded(num, tag, octets);
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerApplicationSpecific derApplicationSpecific))
		{
			return false;
		}
		if (isConstructed == derApplicationSpecific.isConstructed && tag == derApplicationSpecific.tag)
		{
			return Arrays.AreEqual(octets, derApplicationSpecific.octets);
		}
		return false;
	}

	protected override int Asn1GetHashCode()
	{
		return isConstructed.GetHashCode() ^ tag.GetHashCode() ^ Arrays.GetHashCode(octets);
	}

	private byte[] ReplaceTagNumber(int newTag, byte[] input)
	{
		int num = input[0] & 0x1F;
		int num2 = 1;
		if (num == 31)
		{
			int num3 = input[num2++];
			if ((num3 & 0x7F) == 0)
			{
				throw new IOException("corrupted stream - invalid high tag number found");
			}
			while (((uint)num3 & 0x80u) != 0)
			{
				num3 = input[num2++];
			}
		}
		int num4 = input.Length - num2;
		byte[] array = new byte[1 + num4];
		array[0] = (byte)newTag;
		Array.Copy(input, num2, array, 1, num4);
		return array;
	}
}
