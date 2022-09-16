using System;
using System.Security.Cryptography;

namespace Org.BouncyCastle.Crypto.Prng;

public class CryptoApiEntropySourceProvider : IEntropySourceProvider
{
	private class CryptoApiEntropySource : IEntropySource
	{
		private readonly RandomNumberGenerator mRng;

		private readonly bool mPredictionResistant;

		private readonly int mEntropySize;

		bool IEntropySource.IsPredictionResistant => mPredictionResistant;

		int IEntropySource.EntropySize => mEntropySize;

		internal CryptoApiEntropySource(RandomNumberGenerator rng, bool predictionResistant, int entropySize)
		{
			mRng = rng;
			mPredictionResistant = predictionResistant;
			mEntropySize = entropySize;
		}

		byte[] IEntropySource.GetEntropy()
		{
			byte[] array = new byte[(mEntropySize + 7) / 8];
			mRng.GetBytes(array);
			return array;
		}
	}

	private readonly RandomNumberGenerator mRng;

	private readonly bool mPredictionResistant;

	public CryptoApiEntropySourceProvider()
		: this(RandomNumberGenerator.Create(), isPredictionResistant: true)
	{
	}

	public CryptoApiEntropySourceProvider(RandomNumberGenerator rng, bool isPredictionResistant)
	{
		if (rng == null)
		{
			throw new ArgumentNullException("rng");
		}
		mRng = rng;
		mPredictionResistant = isPredictionResistant;
	}

	public IEntropySource Get(int bitsRequired)
	{
		return new CryptoApiEntropySource(mRng, mPredictionResistant, bitsRequired);
	}
}
