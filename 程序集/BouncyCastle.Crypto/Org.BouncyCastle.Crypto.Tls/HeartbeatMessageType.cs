namespace Org.BouncyCastle.Crypto.Tls;

public abstract class HeartbeatMessageType
{
	public const byte heartbeat_request = 1;

	public const byte heartbeat_response = 2;

	public static bool IsValid(byte heartbeatMessageType)
	{
		if (heartbeatMessageType >= 1)
		{
			return heartbeatMessageType <= 2;
		}
		return false;
	}
}
