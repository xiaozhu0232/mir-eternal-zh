using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Generators;

public class DsaKeyPairGenerator : IAsymmetricCipherKeyPairGenerator
{
	private static readonly BigInteger One = BigInteger.One;

	private DsaKeyGenerationParameters param;

	public void Init(KeyGenerationParameters parameters)
	{
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		param = (DsaKeyGenerationParameters)parameters;
	}

	public AsymmetricCipherKeyPair GenerateKeyPair()
	{
		DsaParameters parameters = param.Parameters;
		BigInteger x = GeneratePrivateKey(parameters.Q, param.Random);
		BigInteger y = CalculatePublicKey(parameters.P, parameters.G, x);
		return new AsymmetricCipherKeyPair(new DsaPublicKeyParameters(y, parameters), new DsaPrivateKeyParameters(x, parameters));
	}

	private static BigInteger GeneratePrivateKey(BigInteger q, SecureRandom random)
	{
		int num = q.BitLength >> 2;
		BigInteger bigInteger;
		do
		{
			bigInteger = BigIntegers.CreateRandomInRange(One, q.Subtract(One), random);
		}
		while (WNafUtilities.GetNafWeight(bigInteger) < num);
		return bigInteger;
	}

	private static BigInteger CalculatePublicKey(BigInteger p, BigInteger g, BigInteger x)
	{
		return g.ModPow(x, p);
	}
}
