using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math;

public abstract class Primes
{
	public class MROutput
	{
		private readonly bool mProvablyComposite;

		private readonly BigInteger mFactor;

		public BigInteger Factor => mFactor;

		public bool IsProvablyComposite => mProvablyComposite;

		public bool IsNotPrimePower
		{
			get
			{
				if (mProvablyComposite)
				{
					return mFactor == null;
				}
				return false;
			}
		}

		internal static MROutput ProbablyPrime()
		{
			return new MROutput(provablyComposite: false, null);
		}

		internal static MROutput ProvablyCompositeWithFactor(BigInteger factor)
		{
			return new MROutput(provablyComposite: true, factor);
		}

		internal static MROutput ProvablyCompositeNotPrimePower()
		{
			return new MROutput(provablyComposite: true, null);
		}

		private MROutput(bool provablyComposite, BigInteger factor)
		{
			mProvablyComposite = provablyComposite;
			mFactor = factor;
		}
	}

	public class STOutput
	{
		private readonly BigInteger mPrime;

		private readonly byte[] mPrimeSeed;

		private readonly int mPrimeGenCounter;

		public BigInteger Prime => mPrime;

		public byte[] PrimeSeed => mPrimeSeed;

		public int PrimeGenCounter => mPrimeGenCounter;

		internal STOutput(BigInteger prime, byte[] primeSeed, int primeGenCounter)
		{
			mPrime = prime;
			mPrimeSeed = primeSeed;
			mPrimeGenCounter = primeGenCounter;
		}
	}

	public static readonly int SmallFactorLimit = 211;

	private static readonly BigInteger One = BigInteger.One;

	private static readonly BigInteger Two = BigInteger.Two;

	private static readonly BigInteger Three = BigInteger.Three;

	public static STOutput GenerateSTRandomPrime(IDigest hash, int length, byte[] inputSeed)
	{
		if (hash == null)
		{
			throw new ArgumentNullException("hash");
		}
		if (length < 2)
		{
			throw new ArgumentException("must be >= 2", "length");
		}
		if (inputSeed == null)
		{
			throw new ArgumentNullException("inputSeed");
		}
		if (inputSeed.Length == 0)
		{
			throw new ArgumentException("cannot be empty", "inputSeed");
		}
		return ImplSTRandomPrime(hash, length, Arrays.Clone(inputSeed));
	}

	public static MROutput EnhancedMRProbablePrimeTest(BigInteger candidate, SecureRandom random, int iterations)
	{
		CheckCandidate(candidate, "candidate");
		if (random == null)
		{
			throw new ArgumentNullException("random");
		}
		if (iterations < 1)
		{
			throw new ArgumentException("must be > 0", "iterations");
		}
		if (candidate.BitLength == 2)
		{
			return MROutput.ProbablyPrime();
		}
		if (!candidate.TestBit(0))
		{
			return MROutput.ProvablyCompositeWithFactor(Two);
		}
		BigInteger bigInteger = candidate.Subtract(One);
		BigInteger max = candidate.Subtract(Two);
		int lowestSetBit = bigInteger.GetLowestSetBit();
		BigInteger e = bigInteger.ShiftRight(lowestSetBit);
		for (int i = 0; i < iterations; i++)
		{
			BigInteger bigInteger2 = BigIntegers.CreateRandomInRange(Two, max, random);
			BigInteger bigInteger3 = bigInteger2.Gcd(candidate);
			if (bigInteger3.CompareTo(One) > 0)
			{
				return MROutput.ProvablyCompositeWithFactor(bigInteger3);
			}
			BigInteger bigInteger4 = bigInteger2.ModPow(e, candidate);
			if (bigInteger4.Equals(One) || bigInteger4.Equals(bigInteger))
			{
				continue;
			}
			bool flag = false;
			BigInteger bigInteger5 = bigInteger4;
			for (int j = 1; j < lowestSetBit; j++)
			{
				bigInteger4 = bigInteger4.ModPow(Two, candidate);
				if (bigInteger4.Equals(bigInteger))
				{
					flag = true;
					break;
				}
				if (bigInteger4.Equals(One))
				{
					break;
				}
				bigInteger5 = bigInteger4;
			}
			if (flag)
			{
				continue;
			}
			if (!bigInteger4.Equals(One))
			{
				bigInteger5 = bigInteger4;
				bigInteger4 = bigInteger4.ModPow(Two, candidate);
				if (!bigInteger4.Equals(One))
				{
					bigInteger5 = bigInteger4;
				}
			}
			bigInteger3 = bigInteger5.Subtract(One).Gcd(candidate);
			if (bigInteger3.CompareTo(One) > 0)
			{
				return MROutput.ProvablyCompositeWithFactor(bigInteger3);
			}
			return MROutput.ProvablyCompositeNotPrimePower();
		}
		return MROutput.ProbablyPrime();
	}

	public static bool HasAnySmallFactors(BigInteger candidate)
	{
		CheckCandidate(candidate, "candidate");
		return ImplHasAnySmallFactors(candidate);
	}

	public static bool IsMRProbablePrime(BigInteger candidate, SecureRandom random, int iterations)
	{
		CheckCandidate(candidate, "candidate");
		if (random == null)
		{
			throw new ArgumentException("cannot be null", "random");
		}
		if (iterations < 1)
		{
			throw new ArgumentException("must be > 0", "iterations");
		}
		if (candidate.BitLength == 2)
		{
			return true;
		}
		if (!candidate.TestBit(0))
		{
			return false;
		}
		BigInteger bigInteger = candidate.Subtract(One);
		BigInteger max = candidate.Subtract(Two);
		int lowestSetBit = bigInteger.GetLowestSetBit();
		BigInteger m = bigInteger.ShiftRight(lowestSetBit);
		for (int i = 0; i < iterations; i++)
		{
			BigInteger b = BigIntegers.CreateRandomInRange(Two, max, random);
			if (!ImplMRProbablePrimeToBase(candidate, bigInteger, m, lowestSetBit, b))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsMRProbablePrimeToBase(BigInteger candidate, BigInteger baseValue)
	{
		CheckCandidate(candidate, "candidate");
		CheckCandidate(baseValue, "baseValue");
		if (baseValue.CompareTo(candidate.Subtract(One)) >= 0)
		{
			throw new ArgumentException("must be < ('candidate' - 1)", "baseValue");
		}
		if (candidate.BitLength == 2)
		{
			return true;
		}
		BigInteger bigInteger = candidate.Subtract(One);
		int lowestSetBit = bigInteger.GetLowestSetBit();
		BigInteger m = bigInteger.ShiftRight(lowestSetBit);
		return ImplMRProbablePrimeToBase(candidate, bigInteger, m, lowestSetBit, baseValue);
	}

	private static void CheckCandidate(BigInteger n, string name)
	{
		if (n == null || n.SignValue < 1 || n.BitLength < 2)
		{
			throw new ArgumentException("must be non-null and >= 2", name);
		}
	}

	private static bool ImplHasAnySmallFactors(BigInteger x)
	{
		int num = 223092870;
		int intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 2 == 0 || intValue % 3 == 0 || intValue % 5 == 0 || intValue % 7 == 0 || intValue % 11 == 0 || intValue % 13 == 0 || intValue % 17 == 0 || intValue % 19 == 0 || intValue % 23 == 0)
		{
			return true;
		}
		num = 58642669;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 29 == 0 || intValue % 31 == 0 || intValue % 37 == 0 || intValue % 41 == 0 || intValue % 43 == 0)
		{
			return true;
		}
		num = 600662303;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 47 == 0 || intValue % 53 == 0 || intValue % 59 == 0 || intValue % 61 == 0 || intValue % 67 == 0)
		{
			return true;
		}
		num = 33984931;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 71 == 0 || intValue % 73 == 0 || intValue % 79 == 0 || intValue % 83 == 0)
		{
			return true;
		}
		num = 89809099;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 89 == 0 || intValue % 97 == 0 || intValue % 101 == 0 || intValue % 103 == 0)
		{
			return true;
		}
		num = 167375713;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 107 == 0 || intValue % 109 == 0 || intValue % 113 == 0 || intValue % 127 == 0)
		{
			return true;
		}
		num = 371700317;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 131 == 0 || intValue % 137 == 0 || intValue % 139 == 0 || intValue % 149 == 0)
		{
			return true;
		}
		num = 645328247;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 151 == 0 || intValue % 157 == 0 || intValue % 163 == 0 || intValue % 167 == 0)
		{
			return true;
		}
		num = 1070560157;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 173 == 0 || intValue % 179 == 0 || intValue % 181 == 0 || intValue % 191 == 0)
		{
			return true;
		}
		num = 1596463769;
		intValue = x.Mod(BigInteger.ValueOf(num)).IntValue;
		if (intValue % 193 == 0 || intValue % 197 == 0 || intValue % 199 == 0 || intValue % 211 == 0)
		{
			return true;
		}
		return false;
	}

	private static bool ImplMRProbablePrimeToBase(BigInteger w, BigInteger wSubOne, BigInteger m, int a, BigInteger b)
	{
		BigInteger bigInteger = b.ModPow(m, w);
		if (bigInteger.Equals(One) || bigInteger.Equals(wSubOne))
		{
			return true;
		}
		bool result = false;
		for (int i = 1; i < a; i++)
		{
			bigInteger = bigInteger.ModPow(Two, w);
			if (bigInteger.Equals(wSubOne))
			{
				result = true;
				break;
			}
			if (bigInteger.Equals(One))
			{
				return false;
			}
		}
		return result;
	}

	private static STOutput ImplSTRandomPrime(IDigest d, int length, byte[] primeSeed)
	{
		int digestSize = d.GetDigestSize();
		if (length < 33)
		{
			int num = 0;
			byte[] array = new byte[digestSize];
			byte[] array2 = new byte[digestSize];
			do
			{
				Hash(d, primeSeed, array, 0);
				Inc(primeSeed, 1);
				Hash(d, primeSeed, array2, 0);
				Inc(primeSeed, 1);
				uint num2 = Extract32(array) ^ Extract32(array2);
				num2 &= uint.MaxValue >> 32 - length;
				num2 |= (uint)(1 << length - 1) | 1u;
				num++;
				if (IsPrime32(num2))
				{
					return new STOutput(BigInteger.ValueOf(num2), primeSeed, num);
				}
			}
			while (num <= 4 * length);
			throw new InvalidOperationException("Too many iterations in Shawe-Taylor Random_Prime Routine");
		}
		STOutput sTOutput = ImplSTRandomPrime(d, (length + 3) / 2, primeSeed);
		BigInteger prime = sTOutput.Prime;
		primeSeed = sTOutput.PrimeSeed;
		int num3 = sTOutput.PrimeGenCounter;
		int num4 = 8 * digestSize;
		int num5 = (length - 1) / num4;
		int num6 = num3;
		BigInteger bigInteger = HashGen(d, primeSeed, num5 + 1);
		bigInteger = bigInteger.Mod(One.ShiftLeft(length - 1)).SetBit(length - 1);
		BigInteger bigInteger2 = prime.ShiftLeft(1);
		BigInteger bigInteger3 = bigInteger.Subtract(One).Divide(bigInteger2).Add(One)
			.ShiftLeft(1);
		int num7 = 0;
		BigInteger bigInteger4 = bigInteger3.Multiply(prime).Add(One);
		while (true)
		{
			if (bigInteger4.BitLength > length)
			{
				bigInteger3 = One.ShiftLeft(length - 1).Subtract(One).Divide(bigInteger2)
					.Add(One)
					.ShiftLeft(1);
				bigInteger4 = bigInteger3.Multiply(prime).Add(One);
			}
			num3++;
			if (!ImplHasAnySmallFactors(bigInteger4))
			{
				BigInteger bigInteger5 = HashGen(d, primeSeed, num5 + 1);
				bigInteger5 = bigInteger5.Mod(bigInteger4.Subtract(Three)).Add(Two);
				bigInteger3 = bigInteger3.Add(BigInteger.ValueOf(num7));
				num7 = 0;
				BigInteger bigInteger6 = bigInteger5.ModPow(bigInteger3, bigInteger4);
				if (bigInteger4.Gcd(bigInteger6.Subtract(One)).Equals(One) && bigInteger6.ModPow(prime, bigInteger4).Equals(One))
				{
					return new STOutput(bigInteger4, primeSeed, num3);
				}
			}
			else
			{
				Inc(primeSeed, num5 + 1);
			}
			if (num3 >= 4 * length + num6)
			{
				break;
			}
			num7 += 2;
			bigInteger4 = bigInteger4.Add(bigInteger2);
		}
		throw new InvalidOperationException("Too many iterations in Shawe-Taylor Random_Prime Routine");
	}

	private static uint Extract32(byte[] bs)
	{
		uint num = 0u;
		int num2 = System.Math.Min(4, bs.Length);
		for (int i = 0; i < num2; i++)
		{
			uint num3 = bs[bs.Length - (i + 1)];
			num |= num3 << 8 * i;
		}
		return num;
	}

	private static void Hash(IDigest d, byte[] input, byte[] output, int outPos)
	{
		d.BlockUpdate(input, 0, input.Length);
		d.DoFinal(output, outPos);
	}

	private static BigInteger HashGen(IDigest d, byte[] seed, int count)
	{
		int digestSize = d.GetDigestSize();
		int num = count * digestSize;
		byte[] array = new byte[num];
		for (int i = 0; i < count; i++)
		{
			num -= digestSize;
			Hash(d, seed, array, num);
			Inc(seed, 1);
		}
		return new BigInteger(1, array);
	}

	private static void Inc(byte[] seed, int c)
	{
		int num = seed.Length;
		while (c > 0 && --num >= 0)
		{
			c += seed[num];
			seed[num] = (byte)c;
			c >>= 8;
		}
	}

	private static bool IsPrime32(uint x)
	{
		switch (x)
		{
		case 0u:
		case 1u:
		case 4u:
		case 5u:
			return x == 5;
		case 2u:
		case 3u:
			return true;
		default:
		{
			if ((x & 1) == 0 || x % 3u == 0 || x % 5u == 0)
			{
				return false;
			}
			uint[] array = new uint[8] { 1u, 7u, 11u, 13u, 17u, 19u, 23u, 29u };
			uint num = 0u;
			int num2 = 1;
			while (true)
			{
				if (num2 < array.Length)
				{
					uint num3 = num + array[num2];
					if (x % num3 == 0)
					{
						return x < 30;
					}
					num2++;
				}
				else
				{
					num += 30;
					if (num >> 16 != 0 || num * num >= x)
					{
						break;
					}
					num2 = 0;
				}
			}
			return true;
		}
		}
	}
}
