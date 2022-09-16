namespace Org.BouncyCastle.Crypto;

public interface IXof : IDigest
{
	int DoFinal(byte[] output, int outOff, int outLen);

	int DoOutput(byte[] output, int outOff, int outLen);
}
