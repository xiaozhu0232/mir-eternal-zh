namespace Org.BouncyCastle.Bcpg.Sig;

public class SignatureExpirationTime : SignatureSubpacket
{
	public long Time => ((long)(data[0] & 0xFF) << 24) | ((long)(data[1] & 0xFF) << 16) | ((long)(data[2] & 0xFF) << 8) | ((long)data[3] & 0xFFL);

	protected static byte[] TimeToBytes(long t)
	{
		return new byte[4]
		{
			(byte)(t >> 24),
			(byte)(t >> 16),
			(byte)(t >> 8),
			(byte)t
		};
	}

	public SignatureExpirationTime(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.ExpireTime, critical, isLongLength, data)
	{
	}

	public SignatureExpirationTime(bool critical, long seconds)
		: base(SignatureSubpacketTag.ExpireTime, critical, isLongLength: false, TimeToBytes(seconds))
	{
	}
}
