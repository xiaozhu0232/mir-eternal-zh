namespace Org.BouncyCastle.Bcpg;

public enum RevocationReasonTag : byte
{
	NoReason = 0,
	KeySuperseded = 1,
	KeyCompromised = 2,
	KeyRetired = 3,
	UserNoLongerValid = 32
}
