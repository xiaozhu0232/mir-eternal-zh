namespace Org.BouncyCastle.Bcpg.Sig;

public class IssuerKeyId : SignatureSubpacket
{
	public long KeyId => ((long)(data[0] & 0xFF) << 56) | ((long)(data[1] & 0xFF) << 48) | ((long)(data[2] & 0xFF) << 40) | ((long)(data[3] & 0xFF) << 32) | ((long)(data[4] & 0xFF) << 24) | ((long)(data[5] & 0xFF) << 16) | ((long)(data[6] & 0xFF) << 8) | ((long)data[7] & 0xFFL);

	protected static byte[] KeyIdToBytes(long keyId)
	{
		return new byte[8]
		{
			(byte)(keyId >> 56),
			(byte)(keyId >> 48),
			(byte)(keyId >> 40),
			(byte)(keyId >> 32),
			(byte)(keyId >> 24),
			(byte)(keyId >> 16),
			(byte)(keyId >> 8),
			(byte)keyId
		};
	}

	public IssuerKeyId(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.IssuerKeyId, critical, isLongLength, data)
	{
	}

	public IssuerKeyId(bool critical, long keyId)
		: base(SignatureSubpacketTag.IssuerKeyId, critical, isLongLength: false, KeyIdToBytes(keyId))
	{
	}
}
