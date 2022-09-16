using Org.BouncyCastle.Crypto.Prng.Drbg;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Prng;

public class SP800SecureRandomBuilder
{
	private class HashDrbgProvider : IDrbgProvider
	{
		private readonly IDigest mDigest;

		private readonly byte[] mNonce;

		private readonly byte[] mPersonalizationString;

		private readonly int mSecurityStrength;

		public HashDrbgProvider(IDigest digest, byte[] nonce, byte[] personalizationString, int securityStrength)
		{
			mDigest = digest;
			mNonce = nonce;
			mPersonalizationString = personalizationString;
			mSecurityStrength = securityStrength;
		}

		public ISP80090Drbg Get(IEntropySource entropySource)
		{
			return new HashSP800Drbg(mDigest, mSecurityStrength, entropySource, mPersonalizationString, mNonce);
		}
	}

	private class HMacDrbgProvider : IDrbgProvider
	{
		private readonly IMac mHMac;

		private readonly byte[] mNonce;

		private readonly byte[] mPersonalizationString;

		private readonly int mSecurityStrength;

		public HMacDrbgProvider(IMac hMac, byte[] nonce, byte[] personalizationString, int securityStrength)
		{
			mHMac = hMac;
			mNonce = nonce;
			mPersonalizationString = personalizationString;
			mSecurityStrength = securityStrength;
		}

		public ISP80090Drbg Get(IEntropySource entropySource)
		{
			return new HMacSP800Drbg(mHMac, mSecurityStrength, entropySource, mPersonalizationString, mNonce);
		}
	}

	private class CtrDrbgProvider : IDrbgProvider
	{
		private readonly IBlockCipher mBlockCipher;

		private readonly int mKeySizeInBits;

		private readonly byte[] mNonce;

		private readonly byte[] mPersonalizationString;

		private readonly int mSecurityStrength;

		public CtrDrbgProvider(IBlockCipher blockCipher, int keySizeInBits, byte[] nonce, byte[] personalizationString, int securityStrength)
		{
			mBlockCipher = blockCipher;
			mKeySizeInBits = keySizeInBits;
			mNonce = nonce;
			mPersonalizationString = personalizationString;
			mSecurityStrength = securityStrength;
		}

		public ISP80090Drbg Get(IEntropySource entropySource)
		{
			return new CtrSP800Drbg(mBlockCipher, mKeySizeInBits, mSecurityStrength, entropySource, mPersonalizationString, mNonce);
		}
	}

	private readonly SecureRandom mRandom;

	private readonly IEntropySourceProvider mEntropySourceProvider;

	private byte[] mPersonalizationString = null;

	private int mSecurityStrength = 256;

	private int mEntropyBitsRequired = 256;

	public SP800SecureRandomBuilder()
		: this(new SecureRandom(), predictionResistant: false)
	{
	}

	public SP800SecureRandomBuilder(SecureRandom entropySource, bool predictionResistant)
	{
		mRandom = entropySource;
		mEntropySourceProvider = new BasicEntropySourceProvider(entropySource, predictionResistant);
	}

	public SP800SecureRandomBuilder(IEntropySourceProvider entropySourceProvider)
	{
		mRandom = null;
		mEntropySourceProvider = entropySourceProvider;
	}

	public SP800SecureRandomBuilder SetPersonalizationString(byte[] personalizationString)
	{
		mPersonalizationString = personalizationString;
		return this;
	}

	public SP800SecureRandomBuilder SetSecurityStrength(int securityStrength)
	{
		mSecurityStrength = securityStrength;
		return this;
	}

	public SP800SecureRandomBuilder SetEntropyBitsRequired(int entropyBitsRequired)
	{
		mEntropyBitsRequired = entropyBitsRequired;
		return this;
	}

	public SP800SecureRandom BuildHash(IDigest digest, byte[] nonce, bool predictionResistant)
	{
		return new SP800SecureRandom(mRandom, mEntropySourceProvider.Get(mEntropyBitsRequired), new HashDrbgProvider(digest, nonce, mPersonalizationString, mSecurityStrength), predictionResistant);
	}

	public SP800SecureRandom BuildCtr(IBlockCipher cipher, int keySizeInBits, byte[] nonce, bool predictionResistant)
	{
		return new SP800SecureRandom(mRandom, mEntropySourceProvider.Get(mEntropyBitsRequired), new CtrDrbgProvider(cipher, keySizeInBits, nonce, mPersonalizationString, mSecurityStrength), predictionResistant);
	}

	public SP800SecureRandom BuildHMac(IMac hMac, byte[] nonce, bool predictionResistant)
	{
		return new SP800SecureRandom(mRandom, mEntropySourceProvider.Get(mEntropyBitsRequired), new HMacDrbgProvider(hMac, nonce, mPersonalizationString, mSecurityStrength), predictionResistant);
	}
}
