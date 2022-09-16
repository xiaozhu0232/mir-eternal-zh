namespace Org.BouncyCastle.Crypto.Tls;

public abstract class NameType
{
	public const byte host_name = 0;

	public static bool IsValid(byte nameType)
	{
		return nameType == 0;
	}
}
