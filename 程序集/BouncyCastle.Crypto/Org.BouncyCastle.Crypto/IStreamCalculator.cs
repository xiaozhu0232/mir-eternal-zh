using System.IO;

namespace Org.BouncyCastle.Crypto;

public interface IStreamCalculator
{
	Stream Stream { get; }

	object GetResult();
}
