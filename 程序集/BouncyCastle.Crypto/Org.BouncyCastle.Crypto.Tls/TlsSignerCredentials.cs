namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsSignerCredentials : TlsCredentials
{
	SignatureAndHashAlgorithm SignatureAndHashAlgorithm { get; }

	byte[] GenerateCertificateSignature(byte[] hash);
}
