namespace Org.BouncyCastle.Asn1.X509.SigI;

public sealed class SigIObjectIdentifiers
{
	public static readonly DerObjectIdentifier IdSigI = new DerObjectIdentifier("1.3.36.8");

	public static readonly DerObjectIdentifier IdSigIKP = new DerObjectIdentifier(string.Concat(IdSigI, ".2"));

	public static readonly DerObjectIdentifier IdSigICP = new DerObjectIdentifier(string.Concat(IdSigI, ".1"));

	public static readonly DerObjectIdentifier IdSigION = new DerObjectIdentifier(string.Concat(IdSigI, ".4"));

	public static readonly DerObjectIdentifier IdSigIKPDirectoryService = new DerObjectIdentifier(string.Concat(IdSigIKP, ".1"));

	public static readonly DerObjectIdentifier IdSigIONPersonalData = new DerObjectIdentifier(string.Concat(IdSigION, ".1"));

	public static readonly DerObjectIdentifier IdSigICPSigConform = new DerObjectIdentifier(string.Concat(IdSigICP, ".1"));

	private SigIObjectIdentifiers()
	{
	}
}
