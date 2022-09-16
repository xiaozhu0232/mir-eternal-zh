using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class Ed448KeyPairGenerator : IAsymmetricCipherKeyPairGenerator
{
	private SecureRandom random;

	public virtual void Init(KeyGenerationParameters parameters)
	{
		random = parameters.Random;
	}

	public virtual AsymmetricCipherKeyPair GenerateKeyPair()
	{
		Ed448PrivateKeyParameters ed448PrivateKeyParameters = new Ed448PrivateKeyParameters(random);
		Ed448PublicKeyParameters publicParameter = ed448PrivateKeyParameters.GeneratePublicKey();
		return new AsymmetricCipherKeyPair(publicParameter, ed448PrivateKeyParameters);
	}
}
