namespace Org.BouncyCastle.Asn1.X509.Qualified;

public sealed class Rfc3739QCObjectIdentifiers
{
	public static readonly DerObjectIdentifier IdQcs = new DerObjectIdentifier("1.3.6.1.5.5.7.11");

	public static readonly DerObjectIdentifier IdQcsPkixQCSyntaxV1 = new DerObjectIdentifier(string.Concat(IdQcs, ".1"));

	public static readonly DerObjectIdentifier IdQcsPkixQCSyntaxV2 = new DerObjectIdentifier(string.Concat(IdQcs, ".2"));

	private Rfc3739QCObjectIdentifiers()
	{
	}
}
