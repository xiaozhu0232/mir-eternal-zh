namespace Org.BouncyCastle.Bcpg.Sig;

public class TrustSignature : SignatureSubpacket
{
	public int Depth => data[0] & 0xFF;

	public int TrustAmount => data[1] & 0xFF;

	private static byte[] IntToByteArray(int v1, int v2)
	{
		return new byte[2]
		{
			(byte)v1,
			(byte)v2
		};
	}

	public TrustSignature(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.TrustSig, critical, isLongLength, data)
	{
	}

	public TrustSignature(bool critical, int depth, int trustAmount)
		: base(SignatureSubpacketTag.TrustSig, critical, isLongLength: false, IntToByteArray(depth, trustAmount))
	{
	}
}
