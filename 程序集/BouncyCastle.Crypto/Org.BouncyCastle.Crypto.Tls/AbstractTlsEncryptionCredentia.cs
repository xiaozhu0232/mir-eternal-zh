namespace Org.BouncyCastle.Crypto.Tls;

public abstract class AbstractTlsEncryptionCredentials : AbstractTlsCredentials, TlsEncryptionCredentials, TlsCredentials
{
	public abstract byte[] DecryptPreMasterSecret(byte[] encryptedPreMasterSecret);
}
