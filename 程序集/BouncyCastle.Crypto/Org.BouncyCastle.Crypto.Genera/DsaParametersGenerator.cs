using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Crypto.Generators;

public class DsaParametersGenerator
{
	private IDigest digest;

	private int L;

	private int N;

	private int certainty;

	private SecureRandom random;

	private bool use186_3;

	private int usageIndex;

	public DsaParametersGenerator()
		: this(new Sha1Digest())
	{
	}

	public DsaParametersGenerator(IDigest digest)
	{
		this.digest = digest;
	}

	public virtual void Init(int size, int certainty, SecureRandom random)
	{
		if (!IsValidDsaStrength(size))
		{
			throw new ArgumentException("size must be from 512 - 1024 and a multiple of 64", "size");
		}
		use186_3 = false;
		L = size;
		N = GetDefaultN(size);
		this.certainty = certainty;
		this.random = random;
	}

	public virtual void Init(DsaParameterGenerationParameters parameters)
	{
		use186_3 = true;
		L = parameters.L;
		N = parameters.N;
		certainty = parameters.Certainty;
		random = parameters.Random;
		usageIndex = parameters.UsageIndex;
		if (L < 1024 || L > 3072 || L % 1024 != 0)
		{
			throw new ArgumentException("Values must be between 1024 and 3072 and a multiple of 1024", "L");
		}
		if (L == 1024 && N != 160)
		{
			throw new ArgumentException("N must be 160 for L = 1024");
		}
		if (L == 2048 && N != 224 && N != 256)
		{
			throw new ArgumentException("N must be 224 or 256 for L = 2048");
		}
		if (L == 3072 && N != 256)
		{
			throw new ArgumentException("N must be 256 for L = 3072");
		}
		if (digest.GetDigestSize() * 8 < N)
		{
			throw new InvalidOperationException("Digest output size too small for value of N");
		}
	}

	public virtual DsaParameters GenerateParameters()
	{
		if (!use186_3)
		{
			return GenerateParameters_FIPS186_2();
		}
		return GenerateParameters_FIPS186_3();
	}

	protected virtual DsaParameters GenerateParameters_FIPS186_2()
	{
		byte[] array = new byte[20];
		byte[] array2 = new byte[20];
		byte[] array3 = new byte[20];
		byte[] array4 = new byte[20];
		int num = (L - 1) / 160;
		byte[] array5 = new byte[L / 8];
		if (!(digest is Sha1Digest))
		{
			throw new InvalidOperationException("can only use SHA-1 for generating FIPS 186-2 parameters");
		}
		while (true)
		{
			random.NextBytes(array);
			Hash(digest, array, array2);
			Array.Copy(array, 0, array3, 0, array.Length);
			Inc(array3);
			Hash(digest, array3, array3);
			for (int i = 0; i != array4.Length; i++)
			{
				array4[i] = (byte)(array2[i] ^ array3[i]);
			}
			byte[] array6;
			(array6 = array4)[0] = (byte)(array6[0] | 0x80u);
			(array6 = array4)[19] = (byte)(array6[19] | 1u);
			BigInteger bigInteger = new BigInteger(1, array4);
			if (!bigInteger.IsProbablePrime(certainty))
			{
				continue;
			}
			byte[] array7 = Arrays.Clone(array);
			Inc(array7);
			for (int j = 0; j < 4096; j++)
			{
				for (int k = 0; k < num; k++)
				{
					Inc(array7);
					Hash(digest, array7, array2);
					Array.Copy(array2, 0, array5, array5.Length - (k + 1) * array2.Length, array2.Length);
				}
				Inc(array7);
				Hash(digest, array7, array2);
				Array.Copy(array2, array2.Length - (array5.Length - num * array2.Length), array5, 0, array5.Length - num * array2.Length);
				(array6 = array5)[0] = (byte)(array6[0] | 0x80u);
				BigInteger bigInteger2 = new BigInteger(1, array5);
				BigInteger bigInteger3 = bigInteger2.Mod(bigInteger.ShiftLeft(1));
				BigInteger bigInteger4 = bigInteger2.Subtract(bigInteger3.Subtract(BigInteger.One));
				if (bigInteger4.BitLength == L && bigInteger4.IsProbablePrime(certainty))
				{
					BigInteger g = CalculateGenerator_FIPS186_2(bigInteger4, bigInteger, random);
					return new DsaParameters(bigInteger4, bigInteger, g, new DsaValidationParameters(array, j));
				}
			}
		}
	}

	protected virtual BigInteger CalculateGenerator_FIPS186_2(BigInteger p, BigInteger q, SecureRandom r)
	{
		BigInteger e = p.Subtract(BigInteger.One).Divide(q);
		BigInteger max = p.Subtract(BigInteger.Two);
		BigInteger bigInteger2;
		do
		{
			BigInteger bigInteger = BigIntegers.CreateRandomInRange(BigInteger.Two, max, r);
			bigInteger2 = bigInteger.ModPow(e, p);
		}
		while (bigInteger2.BitLength <= 1);
		return bigInteger2;
	}

	protected virtual DsaParameters GenerateParameters_FIPS186_3()
	{
		IDigest digest = this.digest;
		int num = digest.GetDigestSize() * 8;
		int n = N;
		byte[] array = new byte[n / 8];
		int num2 = (L - 1) / num;
		int n2 = (L - 1) % num;
		byte[] array2 = new byte[digest.GetDigestSize()];
		while (true)
		{
			random.NextBytes(array);
			Hash(digest, array, array2);
			BigInteger bigInteger = new BigInteger(1, array2).Mod(BigInteger.One.ShiftLeft(N - 1));
			BigInteger bigInteger2 = bigInteger.SetBit(0).SetBit(N - 1);
			if (!bigInteger2.IsProbablePrime(certainty))
			{
				continue;
			}
			byte[] array3 = Arrays.Clone(array);
			int num3 = 4 * L;
			for (int i = 0; i < num3; i++)
			{
				BigInteger bigInteger3 = BigInteger.Zero;
				int num4 = 0;
				int num5 = 0;
				while (num4 <= num2)
				{
					Inc(array3);
					Hash(digest, array3, array2);
					BigInteger bigInteger4 = new BigInteger(1, array2);
					if (num4 == num2)
					{
						bigInteger4 = bigInteger4.Mod(BigInteger.One.ShiftLeft(n2));
					}
					bigInteger3 = bigInteger3.Add(bigInteger4.ShiftLeft(num5));
					num4++;
					num5 += num;
				}
				BigInteger bigInteger5 = bigInteger3.Add(BigInteger.One.ShiftLeft(L - 1));
				BigInteger bigInteger6 = bigInteger5.Mod(bigInteger2.ShiftLeft(1));
				BigInteger bigInteger7 = bigInteger5.Subtract(bigInteger6.Subtract(BigInteger.One));
				if (bigInteger7.BitLength != L || !bigInteger7.IsProbablePrime(certainty))
				{
					continue;
				}
				if (usageIndex >= 0)
				{
					BigInteger bigInteger8 = CalculateGenerator_FIPS186_3_Verifiable(digest, bigInteger7, bigInteger2, array, usageIndex);
					if (bigInteger8 != null)
					{
						return new DsaParameters(bigInteger7, bigInteger2, bigInteger8, new DsaValidationParameters(array, i, usageIndex));
					}
				}
				BigInteger g = CalculateGenerator_FIPS186_3_Unverifiable(bigInteger7, bigInteger2, random);
				return new DsaParameters(bigInteger7, bigInteger2, g, new DsaValidationParameters(array, i));
			}
		}
	}

	protected virtual BigInteger CalculateGenerator_FIPS186_3_Unverifiable(BigInteger p, BigInteger q, SecureRandom r)
	{
		return CalculateGenerator_FIPS186_2(p, q, r);
	}

	protected virtual BigInteger CalculateGenerator_FIPS186_3_Verifiable(IDigest d, BigInteger p, BigInteger q, byte[] seed, int index)
	{
		BigInteger e = p.Subtract(BigInteger.One).Divide(q);
		byte[] array = Hex.DecodeStrict("6767656E");
		byte[] array2 = new byte[seed.Length + array.Length + 1 + 2];
		Array.Copy(seed, 0, array2, 0, seed.Length);
		Array.Copy(array, 0, array2, seed.Length, array.Length);
		array2[array2.Length - 3] = (byte)index;
		byte[] array3 = new byte[d.GetDigestSize()];
		for (int i = 1; i < 65536; i++)
		{
			Inc(array2);
			Hash(d, array2, array3);
			BigInteger bigInteger = new BigInteger(1, array3);
			BigInteger bigInteger2 = bigInteger.ModPow(e, p);
			if (bigInteger2.CompareTo(BigInteger.Two) >= 0)
			{
				return bigInteger2;
			}
		}
		return null;
	}

	private static bool IsValidDsaStrength(int strength)
	{
		if (strength >= 512 && strength <= 1024)
		{
			return strength % 64 == 0;
		}
		return false;
	}

	protected static void Hash(IDigest d, byte[] input, byte[] output)
	{
		d.BlockUpdate(input, 0, input.Length);
		d.DoFinal(output, 0);
	}

	private static int GetDefaultN(int L)
	{
		if (L <= 1024)
		{
			return 160;
		}
		return 256;
	}

	protected static void Inc(byte[] buf)
	{
		int num = buf.Length - 1;
		while (num >= 0 && (buf[num] = (byte)(buf[num] + 1)) == 0)
		{
			num--;
		}
	}
}
