namespace Org.BouncyCastle.Asn1.IsisMtt;

public abstract class IsisMttObjectIdentifiers
{
	public static readonly DerObjectIdentifier IdIsisMtt = new DerObjectIdentifier("1.3.36.8");

	public static readonly DerObjectIdentifier IdIsisMttCP = new DerObjectIdentifier(string.Concat(IdIsisMtt, ".1"));

	public static readonly DerObjectIdentifier IdIsisMttCPAccredited = new DerObjectIdentifier(string.Concat(IdIsisMttCP, ".1"));

	public static readonly DerObjectIdentifier IdIsisMttAT = new DerObjectIdentifier(string.Concat(IdIsisMtt, ".3"));

	public static readonly DerObjectIdentifier IdIsisMttATDateOfCertGen = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".1"));

	public static readonly DerObjectIdentifier IdIsisMttATProcuration = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".2"));

	public static readonly DerObjectIdentifier IdIsisMttATAdmission = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".3"));

	public static readonly DerObjectIdentifier IdIsisMttATMonetaryLimit = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".4"));

	public static readonly DerObjectIdentifier IdIsisMttATDeclarationOfMajority = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".5"));

	public static readonly DerObjectIdentifier IdIsisMttATIccsn = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".6"));

	public static readonly DerObjectIdentifier IdIsisMttATPKReference = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".7"));

	public static readonly DerObjectIdentifier IdIsisMttATRestriction = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".8"));

	public static readonly DerObjectIdentifier IdIsisMttATRetrieveIfAllowed = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".9"));

	public static readonly DerObjectIdentifier IdIsisMttATRequestedCertificate = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".10"));

	public static readonly DerObjectIdentifier IdIsisMttATNamingAuthorities = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".11"));

	public static readonly DerObjectIdentifier IdIsisMttATCertInDirSince = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".12"));

	public static readonly DerObjectIdentifier IdIsisMttATCertHash = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".13"));

	public static readonly DerObjectIdentifier IdIsisMttATNameAtBirth = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".14"));

	public static readonly DerObjectIdentifier IdIsisMttATAdditionalInformation = new DerObjectIdentifier(string.Concat(IdIsisMttAT, ".15"));

	public static readonly DerObjectIdentifier IdIsisMttATLiabilityLimitationFlag = new DerObjectIdentifier("0.2.262.1.10.12.0");
}
