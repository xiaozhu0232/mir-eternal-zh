namespace Org.BouncyCastle.Crypto.Modes;

public interface IAeadBlockCipher : IAeadCipher
{
	int GetBlockSize();

	IBlockCipher GetUnderlyingCipher();
}
