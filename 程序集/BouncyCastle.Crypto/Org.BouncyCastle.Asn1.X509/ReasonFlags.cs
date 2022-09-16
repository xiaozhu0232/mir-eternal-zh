namespace Org.BouncyCastle.Asn1.X509;

public class ReasonFlags : DerBitString
{
	public const int Unused = 128;

	public const int KeyCompromise = 64;

	public const int CACompromise = 32;

	public const int AffiliationChanged = 16;

	public const int Superseded = 8;

	public const int CessationOfOperation = 4;

	public const int CertificateHold = 2;

	public const int PrivilegeWithdrawn = 1;

	public const int AACompromise = 32768;

	public ReasonFlags(int reasons)
		: base(reasons)
	{
	}

	public ReasonFlags(DerBitString reasons)
		: base(reasons.GetBytes(), reasons.PadBits)
	{
	}
}
