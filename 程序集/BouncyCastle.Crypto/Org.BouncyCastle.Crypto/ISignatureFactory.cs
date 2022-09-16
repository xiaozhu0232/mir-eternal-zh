namespace Org.BouncyCastle.Crypto;

public interface ISignatureFactory
{
	object AlgorithmDetails { get; }

	IStreamCalculator CreateCalculator();
}
