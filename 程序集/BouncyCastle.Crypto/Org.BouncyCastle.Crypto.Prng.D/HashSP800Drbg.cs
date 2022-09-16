using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Prng.Drbg;

public class HashSP800Drbg : ISP80090Drbg
{
	private static readonly byte[] ONE;

	private static readonly long RESEED_MAX;

	private static readonly int MAX_BITS_REQUEST;

	private static readonly IDictionary seedlens;

	private readonly IDigest mDigest;

	private readonly IEntropySource mEntropySource;

	private readonly int mSecurityStrength;

	private readonly int mSeedLength;

	private byte[] mV;

	private byte[] mC;

	private long mReseedCounter;

	public int BlockSize => mDigest.GetDigestSize() * 8;

	static HashSP800Drbg()
	{
		ONE = new byte[1] { 1 };
		RESEED_MAX = 140737488355328L;
		MAX_BITS_REQUEST = 262144;
		seedlens = Platform.CreateHashtable();
		seedlens.Add("SHA-1", 440);
		seedlens.Add("SHA-224", 440);
		seedlens.Add("SHA-256", 440);
		seedlens.Add("SHA-512/256", 440);
		seedlens.Add("SHA-512/224", 440);
		seedlens.Add("SHA-384", 888);
		seedlens.Add("SHA-512", 888);
	}

	public HashSP800Drbg(IDigest digest, int securityStrength, IEntropySource entropySource, byte[] personalizationString, byte[] nonce)
	{
		if (securityStrength > DrbgUtilities.GetMaxSecurityStrength(digest))
		{
			throw new ArgumentException("Requested security strength is not supported by the derivation function");
		}
		if (entropySource.EntropySize < securityStrength)
		{
			throw new ArgumentException("Not enough entropy for security strength required");
		}
		mDigest = digest;
		mEntropySource = entropySource;
		mSecurityStrength = securityStrength;
		mSeedLength = (int)seedlens[digest.AlgorithmName];
		byte[] entropy = GetEntropy();
		byte[] seedMaterial = Arrays.ConcatenateAll(entropy, nonce, personalizationString);
		byte[] array = (mV = DrbgUtilities.HashDF(mDigest, seedMaterial, mSeedLength));
		byte[] array2 = new byte[mV.Length + 1];
		Array.Copy(mV, 0, array2, 1, mV.Length);
		mC = DrbgUtilities.HashDF(mDigest, array2, mSeedLength);
		mReseedCounter = 1L;
	}

	public int Generate(byte[] output, byte[] additionalInput, bool predictionResistant)
	{
		int num = output.Length * 8;
		if (num > MAX_BITS_REQUEST)
		{
			throw new ArgumentException("Number of bits per request limited to " + MAX_BITS_REQUEST, "output");
		}
		if (mReseedCounter > RESEED_MAX)
		{
			return -1;
		}
		if (predictionResistant)
		{
			Reseed(additionalInput);
			additionalInput = null;
		}
		if (additionalInput != null)
		{
			byte[] array = new byte[1 + mV.Length + additionalInput.Length];
			array[0] = 2;
			Array.Copy(mV, 0, array, 1, mV.Length);
			Array.Copy(additionalInput, 0, array, 1 + mV.Length, additionalInput.Length);
			byte[] shorter = Hash(array);
			AddTo(mV, shorter);
		}
		byte[] sourceArray = hashgen(mV, num);
		byte[] array2 = new byte[mV.Length + 1];
		Array.Copy(mV, 0, array2, 1, mV.Length);
		array2[0] = 3;
		byte[] shorter2 = Hash(array2);
		AddTo(mV, shorter2);
		AddTo(mV, mC);
		AddTo(shorter: new byte[4]
		{
			(byte)(mReseedCounter >> 24),
			(byte)(mReseedCounter >> 16),
			(byte)(mReseedCounter >> 8),
			(byte)mReseedCounter
		}, longer: mV);
		mReseedCounter++;
		Array.Copy(sourceArray, 0, output, 0, output.Length);
		return num;
	}

	private byte[] GetEntropy()
	{
		byte[] entropy = mEntropySource.GetEntropy();
		if (entropy.Length < (mSecurityStrength + 7) / 8)
		{
			throw new InvalidOperationException("Insufficient entropy provided by entropy source");
		}
		return entropy;
	}

	private void AddTo(byte[] longer, byte[] shorter)
	{
		int num = longer.Length - shorter.Length;
		uint num2 = 0u;
		int num3 = shorter.Length;
		while (--num3 >= 0)
		{
			num2 += (uint)(longer[num + num3] + shorter[num3]);
			longer[num + num3] = (byte)num2;
			num2 >>= 8;
		}
		num3 = num;
		while (--num3 >= 0)
		{
			num2 += longer[num3];
			longer[num3] = (byte)num2;
			num2 >>= 8;
		}
	}

	public void Reseed(byte[] additionalInput)
	{
		byte[] entropy = GetEntropy();
		byte[] seedMaterial = Arrays.ConcatenateAll(ONE, mV, entropy, additionalInput);
		byte[] array = (mV = DrbgUtilities.HashDF(mDigest, seedMaterial, mSeedLength));
		byte[] array2 = new byte[mV.Length + 1];
		array2[0] = 0;
		Array.Copy(mV, 0, array2, 1, mV.Length);
		mC = DrbgUtilities.HashDF(mDigest, array2, mSeedLength);
		mReseedCounter = 1L;
	}

	private byte[] Hash(byte[] input)
	{
		byte[] array = new byte[mDigest.GetDigestSize()];
		DoHash(input, array);
		return array;
	}

	private void DoHash(byte[] input, byte[] output)
	{
		mDigest.BlockUpdate(input, 0, input.Length);
		mDigest.DoFinal(output, 0);
	}

	private byte[] hashgen(byte[] input, int lengthInBits)
	{
		int digestSize = mDigest.GetDigestSize();
		int num = lengthInBits / 8 / digestSize;
		byte[] array = new byte[input.Length];
		Array.Copy(input, 0, array, 0, input.Length);
		byte[] array2 = new byte[lengthInBits / 8];
		byte[] array3 = new byte[mDigest.GetDigestSize()];
		for (int i = 0; i <= num; i++)
		{
			DoHash(array, array3);
			int length = ((array2.Length - i * array3.Length > array3.Length) ? array3.Length : (array2.Length - i * array3.Length));
			Array.Copy(array3, 0, array2, i * array3.Length, length);
			AddTo(array, ONE);
		}
		return array2;
	}
}
