namespace Org.BouncyCastle.Crypto.Tls;

public abstract class SrtpProtectionProfile
{
	public const int SRTP_AES128_CM_HMAC_SHA1_80 = 1;

	public const int SRTP_AES128_CM_HMAC_SHA1_32 = 2;

	public const int SRTP_NULL_HMAC_SHA1_80 = 5;

	public const int SRTP_NULL_HMAC_SHA1_32 = 6;

	public const int SRTP_AEAD_AES_128_GCM = 7;

	public const int SRTP_AEAD_AES_256_GCM = 8;
}
