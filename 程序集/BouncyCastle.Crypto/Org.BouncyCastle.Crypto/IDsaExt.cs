using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto;

public interface IDsaExt : IDsa
{
	BigInteger Order { get; }
}
