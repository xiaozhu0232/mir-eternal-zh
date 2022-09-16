namespace Org.BouncyCastle.Crypto;

public interface IDigestFactory
{
	object AlgorithmDetails { get; }

	int DigestLength { get; }

	IStreamCalculator CreateCalculator();
}
