using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Generators;

internal class DHKeyGeneratorHelper
{
	internal static readonly DHKeyGeneratorHelper Instance = new DHKeyGeneratorHelper();

	private DHKeyGeneratorHelper()
	{
	}

	internal BigInteger CalculatePrivate(DHParameters dhParams, SecureRandom random)
	{
		int l = dhParams.L;
		if (l != 0)
		{
			int num = l >> 2;
			BigInteger bigInteger;
			do
			{
				bigInteger = new BigInteger(l, random).SetBit(l - 1);
			}
			while (WNafUtilities.GetNafWeight(bigInteger) < num);
			return bigInteger;
		}
		BigInteger min = BigInteger.Two;
		int m = dhParams.M;
		if (m != 0)
		{
			min = BigInteger.One.ShiftLeft(m - 1);
		}
		BigInteger bigInteger2 = dhParams.Q;
		if (bigInteger2 == null)
		{
			bigInteger2 = dhParams.P;
		}
		BigInteger bigInteger3 = bigInteger2.Subtract(BigInteger.Two);
		int num2 = bigInteger3.BitLength >> 2;
		BigInteger bigInteger4;
		do
		{
			bigInteger4 = BigIntegers.CreateRandomInRange(min, bigInteger3, random);
		}
		while (WNafUtilities.GetNafWeight(bigInteger4) < num2);
		return bigInteger4;
	}

	internal BigInteger CalculatePublic(DHParameters dhParams, BigInteger x)
	{
		return dhParams.G.ModPow(x, dhParams.P);
	}
}
