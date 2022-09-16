using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Prng;

public class BasicEntropySourceProvider : IEntropySourceProvider
{
	private class BasicEntropySource : IEntropySource
	{
		private readonly SecureRandom mSecureRandom;

		private readonly bool mPredictionResistant;

		private readonly int mEntropySize;

		bool IEntropySource.IsPredictionResistant => mPredictionResistant;

		int IEntropySource.EntropySize => mEntropySize;

		internal BasicEntropySource(SecureRandom secureRandom, bool predictionResistant, int entropySize)
		{
			mSecureRandom = secureRandom;
			mPredictionResistant = predictionResistant;
			mEntropySize = entropySize;
		}

		byte[] IEntropySource.GetEntropy()
		{
			return SecureRandom.GetNextBytes(mSecureRandom, (mEntropySize + 7) / 8);
		}
	}

	private readonly SecureRandom mSecureRandom;

	private readonly bool mPredictionResistant;

	public BasicEntropySourceProvider(SecureRandom secureRandom, bool isPredictionResistant)
	{
		mSecureRandom = secureRandom;
		mPredictionResistant = isPredictionResistant;
	}

	public IEntropySource Get(int bitsRequired)
	{
		return new BasicEntropySource(mSecureRandom, mPredictionResistant, bitsRequired);
	}
}
