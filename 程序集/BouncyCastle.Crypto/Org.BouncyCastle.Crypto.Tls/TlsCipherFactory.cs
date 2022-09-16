namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsCipherFactory
{
	TlsCipher CreateCipher(TlsContext context, int encryptionAlgorithm, int macAlgorithm);
}
