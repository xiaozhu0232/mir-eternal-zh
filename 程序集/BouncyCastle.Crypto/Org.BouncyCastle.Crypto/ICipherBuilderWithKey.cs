namespace Org.BouncyCastle.Crypto;

public interface ICipherBuilderWithKey : ICipherBuilder
{
	ICipherParameters Key { get; }
}
