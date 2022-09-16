namespace Org.BouncyCastle.Crypto;

public interface IDecryptorBuilderProvider
{
	ICipherBuilder CreateDecryptorBuilder(object algorithmDetails);
}
