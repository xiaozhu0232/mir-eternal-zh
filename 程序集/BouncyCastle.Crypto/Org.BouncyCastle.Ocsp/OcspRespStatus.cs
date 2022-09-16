namespace Org.BouncyCastle.Ocsp;

public abstract class OcspRespStatus
{
	public const int Successful = 0;

	public const int MalformedRequest = 1;

	public const int InternalError = 2;

	public const int TryLater = 3;

	public const int SigRequired = 5;

	public const int Unauthorized = 6;
}
