namespace Org.BouncyCastle.Crypto;

public interface IStreamCipher
{
	string AlgorithmName { get; }

	void Init(bool forEncryption, ICipherParameters parameters);

	byte ReturnByte(byte input);

	void ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff);

	void Reset();
}
