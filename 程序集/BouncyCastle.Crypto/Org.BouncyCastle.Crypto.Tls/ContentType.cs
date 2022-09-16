namespace Org.BouncyCastle.Crypto.Tls;

public abstract class ContentType
{
	public const byte change_cipher_spec = 20;

	public const byte alert = 21;

	public const byte handshake = 22;

	public const byte application_data = 23;

	public const byte heartbeat = 24;
}
