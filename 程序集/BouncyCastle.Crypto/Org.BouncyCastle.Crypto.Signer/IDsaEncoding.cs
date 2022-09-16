using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Signers;

public interface IDsaEncoding
{
	BigInteger[] Decode(BigInteger n, byte[] encoding);

	byte[] Encode(BigInteger n, BigInteger r, BigInteger s);
}
