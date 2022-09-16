namespace Org.BouncyCastle.Crypto;

public interface IDigest
{
	string AlgorithmName { get; }

	int GetDigestSize();

	int GetByteLength();

	void Update(byte input);

	void BlockUpdate(byte[] input, int inOff, int length);

	int DoFinal(byte[] output, int outOff);

	void Reset();
}
