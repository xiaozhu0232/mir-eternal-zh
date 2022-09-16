using System;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Prng;

public class X931SecureRandom : SecureRandom
{
	private readonly bool mPredictionResistant;

	private readonly SecureRandom mRandomSource;

	private readonly X931Rng mDrbg;

	internal X931SecureRandom(SecureRandom randomSource, X931Rng drbg, bool predictionResistant)
		: base((IRandomGenerator)null)
	{
		mRandomSource = randomSource;
		mDrbg = drbg;
		mPredictionResistant = predictionResistant;
	}

	public override void SetSeed(byte[] seed)
	{
		lock (this)
		{
			if (mRandomSource != null)
			{
				mRandomSource.SetSeed(seed);
			}
		}
	}

	public override void SetSeed(long seed)
	{
		lock (this)
		{
			if (mRandomSource != null)
			{
				mRandomSource.SetSeed(seed);
			}
		}
	}

	public override void NextBytes(byte[] bytes)
	{
		lock (this)
		{
			if (mDrbg.Generate(bytes, mPredictionResistant) < 0)
			{
				mDrbg.Reseed();
				mDrbg.Generate(bytes, mPredictionResistant);
			}
		}
	}

	public override void NextBytes(byte[] buf, int off, int len)
	{
		byte[] array = new byte[len];
		NextBytes(array);
		Array.Copy(array, 0, buf, off, len);
	}

	public override byte[] GenerateSeed(int numBytes)
	{
		return EntropyUtilities.GenerateSeed(mDrbg.EntropySource, numBytes);
	}
}
