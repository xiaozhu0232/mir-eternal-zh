using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class RsaBlindingFactorGenerator
{
	private RsaKeyParameters key;

	private SecureRandom random;

	public void Init(ICipherParameters param)
	{
		if (param is ParametersWithRandom)
		{
			ParametersWithRandom parametersWithRandom = (ParametersWithRandom)param;
			key = (RsaKeyParameters)parametersWithRandom.Parameters;
			random = parametersWithRandom.Random;
		}
		else
		{
			key = (RsaKeyParameters)param;
			random = new SecureRandom();
		}
		if (key.IsPrivate)
		{
			throw new ArgumentException("generator requires RSA public key");
		}
	}

	public BigInteger GenerateBlindingFactor()
	{
		if (key == null)
		{
			throw new InvalidOperationException("generator not initialised");
		}
		BigInteger modulus = key.Modulus;
		int sizeInBits = modulus.BitLength - 1;
		BigInteger bigInteger;
		BigInteger bigInteger2;
		do
		{
			bigInteger = new BigInteger(sizeInBits, random);
			bigInteger2 = bigInteger.Gcd(modulus);
		}
		while (bigInteger.SignValue == 0 || bigInteger.Equals(BigInteger.One) || !bigInteger2.Equals(BigInteger.One));
		return bigInteger;
	}
}
