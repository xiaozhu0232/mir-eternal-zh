namespace Org.BouncyCastle.Crypto;

public interface IKeyUnwrapper
{
	object AlgorithmDetails { get; }

	IBlockResult Unwrap(byte[] cipherText, int offset, int length);
}
