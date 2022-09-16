namespace Org.BouncyCastle.Crypto.Tls;

public abstract class SignatureAlgorithm
{
	public const byte anonymous = 0;

	public const byte rsa = 1;

	public const byte dsa = 2;

	public const byte ecdsa = 3;
}
