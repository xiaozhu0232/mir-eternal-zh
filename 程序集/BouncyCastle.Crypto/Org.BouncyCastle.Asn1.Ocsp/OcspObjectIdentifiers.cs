namespace Org.BouncyCastle.Asn1.Ocsp;

public abstract class OcspObjectIdentifiers
{
	internal const string PkixOcspId = "1.3.6.1.5.5.7.48.1";

	public static readonly DerObjectIdentifier PkixOcsp = new DerObjectIdentifier("1.3.6.1.5.5.7.48.1");

	public static readonly DerObjectIdentifier PkixOcspBasic = new DerObjectIdentifier("1.3.6.1.5.5.7.48.1.1");

	public static readonly DerObjectIdentifier PkixOcspNonce = new DerObjectIdentifier(string.Concat(PkixOcsp, ".2"));

	public static readonly DerObjectIdentifier PkixOcspCrl = new DerObjectIdentifier(string.Concat(PkixOcsp, ".3"));

	public static readonly DerObjectIdentifier PkixOcspResponse = new DerObjectIdentifier(string.Concat(PkixOcsp, ".4"));

	public static readonly DerObjectIdentifier PkixOcspNocheck = new DerObjectIdentifier(string.Concat(PkixOcsp, ".5"));

	public static readonly DerObjectIdentifier PkixOcspArchiveCutoff = new DerObjectIdentifier(string.Concat(PkixOcsp, ".6"));

	public static readonly DerObjectIdentifier PkixOcspServiceLocator = new DerObjectIdentifier(string.Concat(PkixOcsp, ".7"));
}
