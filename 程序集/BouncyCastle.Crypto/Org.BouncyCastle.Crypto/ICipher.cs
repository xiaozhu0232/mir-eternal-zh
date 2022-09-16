using System.IO;

namespace Org.BouncyCastle.Crypto;

public interface ICipher
{
	Stream Stream { get; }

	int GetMaxOutputSize(int inputLen);

	int GetUpdateOutputSize(int inputLen);
}
