using System;
using System.IO;

namespace Org.BouncyCastle.Asn1;

public abstract class Asn1Object : Asn1Encodable
{
	public static Asn1Object FromByteArray(byte[] data)
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(data, writable: false);
			Asn1InputStream asn1InputStream = new Asn1InputStream(memoryStream, data.Length);
			Asn1Object result = asn1InputStream.ReadObject();
			if (memoryStream.Position != memoryStream.Length)
			{
				throw new IOException("extra data found after object");
			}
			return result;
		}
		catch (InvalidCastException)
		{
			throw new IOException("cannot recognise object in byte array");
		}
	}

	public static Asn1Object FromStream(Stream inStr)
	{
		try
		{
			return new Asn1InputStream(inStr).ReadObject();
		}
		catch (InvalidCastException)
		{
			throw new IOException("cannot recognise object in stream");
		}
	}

	public sealed override Asn1Object ToAsn1Object()
	{
		return this;
	}

	internal abstract void Encode(DerOutputStream derOut);

	protected abstract bool Asn1Equals(Asn1Object asn1Object);

	protected abstract int Asn1GetHashCode();

	internal bool CallAsn1Equals(Asn1Object obj)
	{
		return Asn1Equals(obj);
	}

	internal int CallAsn1GetHashCode()
	{
		return Asn1GetHashCode();
	}
}
