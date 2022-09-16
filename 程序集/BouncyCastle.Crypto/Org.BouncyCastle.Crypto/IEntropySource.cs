namespace Org.BouncyCastle.Crypto;

public interface IEntropySource
{
	bool IsPredictionResistant { get; }

	int EntropySize { get; }

	byte[] GetEntropy();
}
