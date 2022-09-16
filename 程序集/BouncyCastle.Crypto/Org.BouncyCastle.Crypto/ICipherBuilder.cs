using System.IO;

namespace Org.BouncyCastle.Crypto;

public interface ICipherBuilder
{
	object AlgorithmDetails { get; }

	int GetMaxOutputSize(int inputLen);

	ICipher BuildCipher(Stream stream);
}
