namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsSession
{
	byte[] SessionID { get; }

	bool IsResumable { get; }

	SessionParameters ExportSessionParameters();

	void Invalidate();
}
