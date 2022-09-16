using System.IO;

namespace Org.BouncyCastle.Asn1;

public abstract class Asn1Encodable : IAsn1Convertible
{
	public const string Der = "DER";

	public const string Ber = "BER";

	public byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		Asn1OutputStream asn1OutputStream = new Asn1OutputStream(memoryStream);
		asn1OutputStream.WriteObject(this);
		return memoryStream.ToArray();
	}

	public byte[] GetEncoded(string encoding)
	{
		if (encoding.Equals("DER"))
		{
			MemoryStream memoryStream = new MemoryStream();
			DerOutputStream derOutputStream = new DerOutputStream(memoryStream);
			derOutputStream.WriteObject(this);
			return memoryStream.ToArray();
		}
		return GetEncoded();
	}

	public byte[] GetDerEncoded()
	{
		try
		{
			return GetEncoded("DER");
		}
		catch (IOException)
		{
			return null;
		}
	}

	public sealed override int GetHashCode()
	{
		return ToAsn1Object().CallAsn1GetHashCode();
	}

	public sealed override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is IAsn1Convertible asn1Convertible))
		{
			return false;
		}
		Asn1Object asn1Object = ToAsn1Object();
		Asn1Object asn1Object2 = asn1Convertible.ToAsn1Object();
		if (asn1Object != asn1Object2)
		{
			if (asn1Object2 != null)
			{
				return asn1Object.CallAsn1Equals(asn1Object2);
			}
			return false;
		}
		return true;
	}

	public abstract Asn1Object ToAsn1Object();
}
