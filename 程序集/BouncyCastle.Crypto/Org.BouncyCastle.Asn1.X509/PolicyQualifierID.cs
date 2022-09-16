namespace Org.BouncyCastle.Asn1.X509;

public sealed class PolicyQualifierID : DerObjectIdentifier
{
	private const string IdQt = "1.3.6.1.5.5.7.2";

	public static readonly PolicyQualifierID IdQtCps = new PolicyQualifierID("1.3.6.1.5.5.7.2.1");

	public static readonly PolicyQualifierID IdQtUnotice = new PolicyQualifierID("1.3.6.1.5.5.7.2.2");

	private PolicyQualifierID(string id)
		: base(id)
	{
	}
}
