using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Generators;

internal class DHParametersHelper
{
	private static readonly BigInteger Six = BigInteger.ValueOf(6L);

	private static readonly int[][] primeLists = BigInteger.primeLists;

	private static readonly int[] primeProducts = BigInteger.primeProducts;

	private static readonly BigInteger[] BigPrimeProducts = ConstructBigPrimeProducts(primeProducts);

	private static BigInteger[] ConstructBigPrimeProducts(int[] primeProducts)
	{
		BigInteger[] array = new BigInteger[primeProducts.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = BigInteger.ValueOf(primeProducts[i]);
		}
		return array;
	}

	internal static BigInteger[] GenerateSafePrimes(int size, int certainty, SecureRandom random)
	{
		int num = size - 1;
		int num2 = size >> 2;
		BigInteger bigInteger;
		BigInteger bigInteger2;
		if (size <= 32)
		{
			do
			{
				bigInteger = new BigInteger(num, 2, random);
				bigInteger2 = bigInteger.ShiftLeft(1).Add(BigInteger.One);
			}
			while (!bigInteger2.IsProbablePrime(certainty, randomlySelected: true) || (certainty > 2 && !bigInteger.IsProbablePrime(certainty, randomlySelected: true)));
		}
		else
		{
			while (true)
			{
				bigInteger = new BigInteger(num, 0, random);
				while (true)
				{
					for (int i = 0; i < primeLists.Length; i++)
					{
						int num3 = bigInteger.Remainder(BigPrimeProducts[i]).IntValue;
						if (i == 0)
						{
							int num4 = num3 % 3;
							if (num4 != 2)
							{
								int num5 = 2 * num4 + 2;
								bigInteger = bigInteger.Add(BigInteger.ValueOf(num5));
								num3 = (num3 + num5) % primeProducts[i];
							}
						}
						int[] array = primeLists[i];
						foreach (int num6 in array)
						{
							int num7 = num3 % num6;
							if (num7 == 0 || num7 == num6 >> 1)
							{
								goto IL_00cd;
							}
						}
					}
					break;
					IL_00cd:
					bigInteger = bigInteger.Add(Six);
				}
				if (bigInteger.BitLength == num && bigInteger.RabinMillerTest(2, random, randomlySelected: true))
				{
					bigInteger2 = bigInteger.ShiftLeft(1).Add(BigInteger.One);
					if (bigInteger2.RabinMillerTest(certainty, random, randomlySelected: true) && (certainty <= 2 || bigInteger.RabinMillerTest(certainty - 2, random, randomlySelected: true)) && WNafUtilities.GetNafWeight(bigInteger2) >= num2)
					{
						break;
					}
				}
			}
		}
		return new BigInteger[2] { bigInteger2, bigInteger };
	}

	internal static BigInteger SelectGenerator(BigInteger p, BigInteger q, SecureRandom random)
	{
		BigInteger max = p.Subtract(BigInteger.Two);
		BigInteger bigInteger2;
		do
		{
			BigInteger bigInteger = BigIntegers.CreateRandomInRange(BigInteger.Two, max, random);
			bigInteger2 = bigInteger.ModPow(BigInteger.Two, p);
		}
		while (bigInteger2.Equals(BigInteger.One));
		return bigInteger2;
	}
}
