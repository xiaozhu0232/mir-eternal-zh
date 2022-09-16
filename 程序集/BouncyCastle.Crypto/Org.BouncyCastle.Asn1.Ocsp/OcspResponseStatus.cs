namespace Org.BouncyCastle.Asn1.Ocsp;

public class OcspResponseStatus : DerEnumerated
{
	public const int Successful = 0;

	public const int MalformedRequest = 1;

	public const int InternalError = 2;

	public const int TryLater = 3;

	public const int SignatureRequired = 5;

	public const int Unauthorized = 6;

	public OcspResponseStatus(int value)
		: base(value)
	{
	}

	public OcspResponseStatus(DerEnumerated value)
		: base(value.IntValueExact)
	{
	}
}
