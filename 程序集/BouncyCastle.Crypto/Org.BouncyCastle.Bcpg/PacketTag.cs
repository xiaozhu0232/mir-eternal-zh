namespace Org.BouncyCastle.Bcpg;

public enum PacketTag
{
	Reserved = 0,
	PublicKeyEncryptedSession = 1,
	Signature = 2,
	SymmetricKeyEncryptedSessionKey = 3,
	OnePassSignature = 4,
	SecretKey = 5,
	PublicKey = 6,
	SecretSubkey = 7,
	CompressedData = 8,
	SymmetricKeyEncrypted = 9,
	Marker = 10,
	LiteralData = 11,
	Trust = 12,
	UserId = 13,
	PublicSubkey = 14,
	UserAttribute = 17,
	SymmetricEncryptedIntegrityProtected = 18,
	ModificationDetectionCode = 19,
	Experimental1 = 60,
	Experimental2 = 61,
	Experimental3 = 62,
	Experimental4 = 63
}
