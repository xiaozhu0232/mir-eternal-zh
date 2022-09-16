namespace Org.BouncyCastle.Crypto;

public interface IKeyWrapper
{
	object AlgorithmDetails { get; }

	IBlockResult Wrap(byte[] keyData);
}
