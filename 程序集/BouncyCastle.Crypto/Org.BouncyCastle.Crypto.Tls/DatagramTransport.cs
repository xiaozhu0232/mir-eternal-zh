namespace Org.BouncyCastle.Crypto.Tls;

public interface DatagramTransport : TlsCloseable
{
	int GetReceiveLimit();

	int GetSendLimit();

	int Receive(byte[] buf, int off, int len, int waitMillis);

	void Send(byte[] buf, int off, int len);
}
