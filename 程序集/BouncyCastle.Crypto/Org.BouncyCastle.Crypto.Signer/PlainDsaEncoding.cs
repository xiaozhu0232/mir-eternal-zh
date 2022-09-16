using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class PlainDsaEncoding : IDsaEncoding
{
	public static readonly PlainDsaEncoding Instance = new PlainDsaEncoding();

	public virtual BigInteger[] Decode(BigInteger n, byte[] encoding)
	{
		int unsignedByteLength = BigIntegers.GetUnsignedByteLength(n);
		if (encoding.Length != unsignedByteLength * 2)
		{
			throw new ArgumentException("Encoding has incorrect length", "encoding");
		}
		return new BigInteger[2]
		{
			DecodeValue(n, encoding, 0, unsignedByteLength),
			DecodeValue(n, encoding, unsignedByteLength, unsignedByteLength)
		};
	}

	public virtual byte[] Encode(BigInteger n, BigInteger r, BigInteger s)
	{
		int unsignedByteLength = BigIntegers.GetUnsignedByteLength(n);
		byte[] array = new byte[unsignedByteLength * 2];
		EncodeValue(n, r, array, 0, unsignedByteLength);
		EncodeValue(n, s, array, unsignedByteLength, unsignedByteLength);
		return array;
	}

	protected virtual BigInteger CheckValue(BigInteger n, BigInteger x)
	{
		if (x.SignValue < 0 || x.CompareTo(n) >= 0)
		{
			throw new ArgumentException("Value out of range", "x");
		}
		return x;
	}

	protected virtual BigInteger DecodeValue(BigInteger n, byte[] buf, int off, int len)
	{
		return CheckValue(n, new BigInteger(1, buf, off, len));
	}

	protected virtual void EncodeValue(BigInteger n, BigInteger x, byte[] buf, int off, int len)
	{
		byte[] array = CheckValue(n, x).ToByteArrayUnsigned();
		int num = System.Math.Max(0, array.Length - len);
		int num2 = array.Length - num;
		int num3 = len - num2;
		Arrays.Fill(buf, off, off + num3, 0);
		Array.Copy(array, num, buf, off + num3, num2);
	}
}
