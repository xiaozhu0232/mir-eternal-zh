using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class X25519KeyPairGenerator : IAsymmetricCipherKeyPairGenerator
{
	private SecureRandom random;

	public virtual void Init(KeyGenerationParameters parameters)
	{
		random = parameters.Random;
	}

	public virtual AsymmetricCipherKeyPair GenerateKeyPair()
	{
		X25519PrivateKeyParameters x25519PrivateKeyParameters = new X25519PrivateKeyParameters(random);
		X25519PublicKeyParameters publicParameter = x25519PrivateKeyParameters.GeneratePublicKey();
		return new AsymmetricCipherKeyPair(publicParameter, x25519PrivateKeyParameters);
	}
}
