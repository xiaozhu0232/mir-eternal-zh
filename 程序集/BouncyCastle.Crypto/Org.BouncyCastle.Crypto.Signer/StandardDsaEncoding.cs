using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class StandardDsaEncoding : IDsaEncoding
{
	public static readonly StandardDsaEncoding Instance = new StandardDsaEncoding();

	public virtual BigInteger[] Decode(BigInteger n, byte[] encoding)
	{
		Asn1Sequence asn1Sequence = (Asn1Sequence)Asn1Object.FromByteArray(encoding);
		if (asn1Sequence.Count == 2)
		{
			BigInteger bigInteger = DecodeValue(n, asn1Sequence, 0);
			BigInteger bigInteger2 = DecodeValue(n, asn1Sequence, 1);
			byte[] a = Encode(n, bigInteger, bigInteger2);
			if (Arrays.AreEqual(a, encoding))
			{
				return new BigInteger[2] { bigInteger, bigInteger2 };
			}
		}
		throw new ArgumentException("Malformed signature", "encoding");
	}

	public virtual byte[] Encode(BigInteger n, BigInteger r, BigInteger s)
	{
		return new DerSequence(EncodeValue(n, r), EncodeValue(n, s)).GetEncoded("DER");
	}

	protected virtual BigInteger CheckValue(BigInteger n, BigInteger x)
	{
		if (x.SignValue < 0 || (n != null && x.CompareTo(n) >= 0))
		{
			throw new ArgumentException("Value out of range", "x");
		}
		return x;
	}

	protected virtual BigInteger DecodeValue(BigInteger n, Asn1Sequence s, int pos)
	{
		return CheckValue(n, ((DerInteger)s[pos]).Value);
	}

	protected virtual DerInteger EncodeValue(BigInteger n, BigInteger x)
	{
		return new DerInteger(CheckValue(n, x));
	}
}
