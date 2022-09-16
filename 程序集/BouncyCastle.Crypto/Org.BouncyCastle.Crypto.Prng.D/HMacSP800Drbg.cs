using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Prng.Drbg;

public class HMacSP800Drbg : ISP80090Drbg
{
	private static readonly long RESEED_MAX = 140737488355328L;

	private static readonly int MAX_BITS_REQUEST = 262144;

	private readonly byte[] mK;

	private readonly byte[] mV;

	private readonly IEntropySource mEntropySource;

	private readonly IMac mHMac;

	private readonly int mSecurityStrength;

	private long mReseedCounter;

	public int BlockSize => mV.Length * 8;

	public HMacSP800Drbg(IMac hMac, int securityStrength, IEntropySource entropySource, byte[] personalizationString, byte[] nonce)
	{
		if (securityStrength > DrbgUtilities.GetMaxSecurityStrength(hMac))
		{
			throw new ArgumentException("Requested security strength is not supported by the derivation function");
		}
		if (entropySource.EntropySize < securityStrength)
		{
			throw new ArgumentException("Not enough entropy for security strength required");
		}
		mHMac = hMac;
		mSecurityStrength = securityStrength;
		mEntropySource = entropySource;
		byte[] entropy = GetEntropy();
		byte[] seedMaterial = Arrays.ConcatenateAll(entropy, nonce, personalizationString);
		mK = new byte[hMac.GetMacSize()];
		mV = new byte[mK.Length];
		Arrays.Fill(mV, 1);
		hmac_DRBG_Update(seedMaterial);
		mReseedCounter = 1L;
	}

	private void hmac_DRBG_Update(byte[] seedMaterial)
	{
		hmac_DRBG_Update_Func(seedMaterial, 0);
		if (seedMaterial != null)
		{
			hmac_DRBG_Update_Func(seedMaterial, 1);
		}
	}

	private void hmac_DRBG_Update_Func(byte[] seedMaterial, byte vValue)
	{
		mHMac.Init(new KeyParameter(mK));
		mHMac.BlockUpdate(mV, 0, mV.Length);
		mHMac.Update(vValue);
		if (seedMaterial != null)
		{
			mHMac.BlockUpdate(seedMaterial, 0, seedMaterial.Length);
		}
		mHMac.DoFinal(mK, 0);
		mHMac.Init(new KeyParameter(mK));
		mHMac.BlockUpdate(mV, 0, mV.Length);
		mHMac.DoFinal(mV, 0);
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
			hmac_DRBG_Update(additionalInput);
		}
		byte[] array = new byte[output.Length];
		int num2 = output.Length / mV.Length;
		mHMac.Init(new KeyParameter(mK));
		for (int i = 0; i < num2; i++)
		{
			mHMac.BlockUpdate(mV, 0, mV.Length);
			mHMac.DoFinal(mV, 0);
			Array.Copy(mV, 0, array, i * mV.Length, mV.Length);
		}
		if (num2 * mV.Length < array.Length)
		{
			mHMac.BlockUpdate(mV, 0, mV.Length);
			mHMac.DoFinal(mV, 0);
			Array.Copy(mV, 0, array, num2 * mV.Length, array.Length - num2 * mV.Length);
		}
		hmac_DRBG_Update(additionalInput);
		mReseedCounter++;
		Array.Copy(array, 0, output, 0, output.Length);
		return num;
	}

	public void Reseed(byte[] additionalInput)
	{
		byte[] entropy = GetEntropy();
		byte[] seedMaterial = Arrays.Concatenate(entropy, additionalInput);
		hmac_DRBG_Update(seedMaterial);
		mReseedCounter = 1L;
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
}
