using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class Ed25519KeyPairGenerator : IAsymmetricCipherKeyPairGenerator
{
	private SecureRandom random;

	public virtual void Init(KeyGenerationParameters parameters)
	{
		random = parameters.Random;
	}

	public virtual AsymmetricCipherKeyPair GenerateKeyPair()
	{
		Ed25519PrivateKeyParameters ed25519PrivateKeyParameters = new Ed25519PrivateKeyParameters(random);
		Ed25519PublicKeyParameters publicParameter = ed25519PrivateKeyParameters.GeneratePublicKey();
		return new AsymmetricCipherKeyPair(publicParameter, ed25519PrivateKeyParameters);
	}
}
