using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Generators;

public class ElGamalKeyPairGenerator : IAsymmetricCipherKeyPairGenerator
{
	private ElGamalKeyGenerationParameters param;

	public void Init(KeyGenerationParameters parameters)
	{
		param = (ElGamalKeyGenerationParameters)parameters;
	}

	public AsymmetricCipherKeyPair GenerateKeyPair()
	{
		DHKeyGeneratorHelper instance = DHKeyGeneratorHelper.Instance;
		ElGamalParameters parameters = param.Parameters;
		DHParameters dhParams = new DHParameters(parameters.P, parameters.G, null, 0, parameters.L);
		BigInteger x = instance.CalculatePrivate(dhParams, param.Random);
		BigInteger y = instance.CalculatePublic(dhParams, x);
		return new AsymmetricCipherKeyPair(new ElGamalPublicKeyParameters(y, parameters), new ElGamalPrivateKeyParameters(x, parameters));
	}
}
