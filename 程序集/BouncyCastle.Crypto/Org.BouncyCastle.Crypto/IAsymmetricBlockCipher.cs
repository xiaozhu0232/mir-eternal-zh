namespace Org.BouncyCastle.Crypto;

public interface IAsymmetricBlockCipher
{
	string AlgorithmName { get; }

	void Init(bool forEncryption, ICipherParameters parameters);

	int GetInputBlockSize();

	int GetOutputBlockSize();

	byte[] ProcessBlock(byte[] inBuf, int inOff, int inLen);
}
