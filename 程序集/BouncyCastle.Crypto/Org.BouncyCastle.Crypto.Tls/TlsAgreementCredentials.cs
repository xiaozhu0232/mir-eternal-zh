namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsAgreementCredentials : TlsCredentials
{
	byte[] GenerateAgreement(AsymmetricKeyParameter peerPublicKey);
}
