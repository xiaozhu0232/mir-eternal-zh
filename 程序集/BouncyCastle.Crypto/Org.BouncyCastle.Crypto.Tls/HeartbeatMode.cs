namespace Org.BouncyCastle.Crypto.Tls;

public abstract class HeartbeatMode
{
	public const byte peer_allowed_to_send = 1;

	public const byte peer_not_allowed_to_send = 2;

	public static bool IsValid(byte heartbeatMode)
	{
		if (heartbeatMode >= 1)
		{
			return heartbeatMode <= 2;
		}
		return false;
	}
}
