namespace Org.BouncyCastle.Asn1.Cmp;

public enum PkiStatus
{
	Granted,
	GrantedWithMods,
	Rejection,
	Waiting,
	RevocationWarning,
	RevocationNotification,
	KeyUpdateWarning
}
