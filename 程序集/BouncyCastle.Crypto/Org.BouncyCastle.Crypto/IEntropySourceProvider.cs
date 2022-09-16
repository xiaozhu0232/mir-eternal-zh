namespace Org.BouncyCastle.Crypto;

public interface IEntropySourceProvider
{
	IEntropySource Get(int bitsRequired);
}
