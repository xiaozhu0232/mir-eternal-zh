namespace Org.BouncyCastle.Bcpg;

public enum SignatureSubpacketTag
{
	CreationTime = 2,
	ExpireTime = 3,
	Exportable = 4,
	TrustSig = 5,
	RegExp = 6,
	Revocable = 7,
	KeyExpireTime = 9,
	Placeholder = 10,
	PreferredSymmetricAlgorithms = 11,
	RevocationKey = 12,
	IssuerKeyId = 16,
	NotationData = 20,
	PreferredHashAlgorithms = 21,
	PreferredCompressionAlgorithms = 22,
	KeyServerPreferences = 23,
	PreferredKeyServer = 24,
	PrimaryUserId = 25,
	PolicyUrl = 26,
	KeyFlags = 27,
	SignerUserId = 28,
	RevocationReason = 29,
	Features = 30,
	SignatureTarget = 31,
	EmbeddedSignature = 32
}
