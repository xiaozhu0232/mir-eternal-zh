using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Crypto.Prng.Drbg;

public class CtrSP800Drbg : ISP80090Drbg
{
	private static readonly long TDEA_RESEED_MAX = 2147483648L;

	private static readonly long AES_RESEED_MAX = 140737488355328L;

	private static readonly int TDEA_MAX_BITS_REQUEST = 4096;

	private static readonly int AES_MAX_BITS_REQUEST = 262144;

	private readonly IEntropySource mEntropySource;

	private readonly IBlockCipher mEngine;

	private readonly int mKeySizeInBits;

	private readonly int mSeedLength;

	private readonly int mSecurityStrength;

	private byte[] mKey;

	private byte[] mV;

	private long mReseedCounter = 0L;

	private bool mIsTdea = false;

	private static readonly byte[] K_BITS = Hex.DecodeStrict("000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F");

	public int BlockSize => mV.Length * 8;

	public CtrSP800Drbg(IBlockCipher engine, int keySizeInBits, int securityStrength, IEntropySource entropySource, byte[] personalizationString, byte[] nonce)
	{
		if (securityStrength > 256)
		{
			throw new ArgumentException("Requested security strength is not supported by the derivation function");
		}
		if (GetMaxSecurityStrength(engine, keySizeInBits) < securityStrength)
		{
			throw new ArgumentException("Requested security strength is not supported by block cipher and key size");
		}
		if (entropySource.EntropySize < securityStrength)
		{
			throw new ArgumentException("Not enough entropy for security strength required");
		}
		mEntropySource = entropySource;
		mEngine = engine;
		mKeySizeInBits = keySizeInBits;
		mSecurityStrength = securityStrength;
		mSeedLength = keySizeInBits + engine.GetBlockSize() * 8;
		mIsTdea = IsTdea(engine);
		byte[] entropy = GetEntropy();
		CTR_DRBG_Instantiate_algorithm(entropy, nonce, personalizationString);
	}

	private void CTR_DRBG_Instantiate_algorithm(byte[] entropy, byte[] nonce, byte[] personalisationString)
	{
		byte[] inputString = Arrays.ConcatenateAll(entropy, nonce, personalisationString);
		byte[] seed = Block_Cipher_df(inputString, mSeedLength);
		int blockSize = mEngine.GetBlockSize();
		mKey = new byte[(mKeySizeInBits + 7) / 8];
		mV = new byte[blockSize];
		CTR_DRBG_Update(seed, mKey, mV);
		mReseedCounter = 1L;
	}

	private void CTR_DRBG_Update(byte[] seed, byte[] key, byte[] v)
	{
		byte[] array = new byte[seed.Length];
		byte[] array2 = new byte[mEngine.GetBlockSize()];
		int i = 0;
		int blockSize = mEngine.GetBlockSize();
		mEngine.Init(forEncryption: true, new KeyParameter(ExpandKey(key)));
		for (; i * blockSize < seed.Length; i++)
		{
			AddOneTo(v);
			mEngine.ProcessBlock(v, 0, array2, 0);
			int length = ((array.Length - i * blockSize > blockSize) ? blockSize : (array.Length - i * blockSize));
			Array.Copy(array2, 0, array, i * blockSize, length);
		}
		XOR(array, seed, array, 0);
		Array.Copy(array, 0, key, 0, key.Length);
		Array.Copy(array, key.Length, v, 0, v.Length);
	}

	private void CTR_DRBG_Reseed_algorithm(byte[] additionalInput)
	{
		byte[] inputString = Arrays.Concatenate(GetEntropy(), additionalInput);
		inputString = Block_Cipher_df(inputString, mSeedLength);
		CTR_DRBG_Update(inputString, mKey, mV);
		mReseedCounter = 1L;
	}

	private void XOR(byte[] output, byte[] a, byte[] b, int bOff)
	{
		for (int i = 0; i < output.Length; i++)
		{
			output[i] = (byte)(a[i] ^ b[bOff + i]);
		}
	}

	private void AddOneTo(byte[] longer)
	{
		uint num = 1u;
		int num2 = longer.Length;
		while (--num2 >= 0)
		{
			num += longer[num2];
			longer[num2] = (byte)num;
			num >>= 8;
		}
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

	private byte[] Block_Cipher_df(byte[] inputString, int bitLength)
	{
		int blockSize = mEngine.GetBlockSize();
		int num = inputString.Length;
		int value = bitLength / 8;
		int num2 = 8 + num + 1;
		int num3 = (num2 + blockSize - 1) / blockSize * blockSize;
		byte[] array = new byte[num3];
		copyIntToByteArray(array, num, 0);
		copyIntToByteArray(array, value, 4);
		Array.Copy(inputString, 0, array, 8, num);
		array[8 + num] = 128;
		byte[] array2 = new byte[mKeySizeInBits / 8 + blockSize];
		byte[] array3 = new byte[blockSize];
		byte[] array4 = new byte[blockSize];
		int i = 0;
		byte[] array5 = new byte[mKeySizeInBits / 8];
		Array.Copy(K_BITS, 0, array5, 0, array5.Length);
		for (; i * blockSize * 8 < mKeySizeInBits + blockSize * 8; i++)
		{
			copyIntToByteArray(array4, i, 0);
			BCC(array3, array5, array4, array);
			int length = ((array2.Length - i * blockSize > blockSize) ? blockSize : (array2.Length - i * blockSize));
			Array.Copy(array3, 0, array2, i * blockSize, length);
		}
		byte[] array6 = new byte[blockSize];
		Array.Copy(array2, 0, array5, 0, array5.Length);
		Array.Copy(array2, array5.Length, array6, 0, array6.Length);
		array2 = new byte[bitLength / 2];
		i = 0;
		mEngine.Init(forEncryption: true, new KeyParameter(ExpandKey(array5)));
		for (; i * blockSize < array2.Length; i++)
		{
			mEngine.ProcessBlock(array6, 0, array6, 0);
			int length2 = ((array2.Length - i * blockSize > blockSize) ? blockSize : (array2.Length - i * blockSize));
			Array.Copy(array6, 0, array2, i * blockSize, length2);
		}
		return array2;
	}

	private void BCC(byte[] bccOut, byte[] k, byte[] iV, byte[] data)
	{
		int blockSize = mEngine.GetBlockSize();
		byte[] array = new byte[blockSize];
		int num = data.Length / blockSize;
		byte[] array2 = new byte[blockSize];
		mEngine.Init(forEncryption: true, new KeyParameter(ExpandKey(k)));
		mEngine.ProcessBlock(iV, 0, array, 0);
		for (int i = 0; i < num; i++)
		{
			XOR(array2, array, data, i * blockSize);
			mEngine.ProcessBlock(array2, 0, array, 0);
		}
		Array.Copy(array, 0, bccOut, 0, bccOut.Length);
	}

	private void copyIntToByteArray(byte[] buf, int value, int offSet)
	{
		buf[offSet] = (byte)(value >> 24);
		buf[offSet + 1] = (byte)(value >> 16);
		buf[offSet + 2] = (byte)(value >> 8);
		buf[offSet + 3] = (byte)value;
	}

	public int Generate(byte[] output, byte[] additionalInput, bool predictionResistant)
	{
		if (mIsTdea)
		{
			if (mReseedCounter > TDEA_RESEED_MAX)
			{
				return -1;
			}
			if (DrbgUtilities.IsTooLarge(output, TDEA_MAX_BITS_REQUEST / 8))
			{
				throw new ArgumentException("Number of bits per request limited to " + TDEA_MAX_BITS_REQUEST, "output");
			}
		}
		else
		{
			if (mReseedCounter > AES_RESEED_MAX)
			{
				return -1;
			}
			if (DrbgUtilities.IsTooLarge(output, AES_MAX_BITS_REQUEST / 8))
			{
				throw new ArgumentException("Number of bits per request limited to " + AES_MAX_BITS_REQUEST, "output");
			}
		}
		if (predictionResistant)
		{
			CTR_DRBG_Reseed_algorithm(additionalInput);
			additionalInput = null;
		}
		if (additionalInput != null)
		{
			additionalInput = Block_Cipher_df(additionalInput, mSeedLength);
			CTR_DRBG_Update(additionalInput, mKey, mV);
		}
		else
		{
			additionalInput = new byte[mSeedLength];
		}
		byte[] array = new byte[mV.Length];
		mEngine.Init(forEncryption: true, new KeyParameter(ExpandKey(mKey)));
		for (int i = 0; i <= output.Length / array.Length; i++)
		{
			int num = ((output.Length - i * array.Length > array.Length) ? array.Length : (output.Length - i * mV.Length));
			if (num != 0)
			{
				AddOneTo(mV);
				mEngine.ProcessBlock(mV, 0, array, 0);
				Array.Copy(array, 0, output, i * array.Length, num);
			}
		}
		CTR_DRBG_Update(additionalInput, mKey, mV);
		mReseedCounter++;
		return output.Length * 8;
	}

	public void Reseed(byte[] additionalInput)
	{
		CTR_DRBG_Reseed_algorithm(additionalInput);
	}

	private bool IsTdea(IBlockCipher cipher)
	{
		if (!cipher.AlgorithmName.Equals("DESede"))
		{
			return cipher.AlgorithmName.Equals("TDEA");
		}
		return true;
	}

	private int GetMaxSecurityStrength(IBlockCipher cipher, int keySizeInBits)
	{
		if (IsTdea(cipher) && keySizeInBits == 168)
		{
			return 112;
		}
		if (cipher.AlgorithmName.Equals("AES"))
		{
			return keySizeInBits;
		}
		return -1;
	}

	private byte[] ExpandKey(byte[] key)
	{
		if (mIsTdea)
		{
			byte[] array = new byte[24];
			PadKey(key, 0, array, 0);
			PadKey(key, 7, array, 8);
			PadKey(key, 14, array, 16);
			return array;
		}
		return key;
	}

	private void PadKey(byte[] keyMaster, int keyOff, byte[] tmp, int tmpOff)
	{
		tmp[tmpOff] = (byte)(keyMaster[keyOff] & 0xFEu);
		tmp[tmpOff + 1] = (byte)((keyMaster[keyOff] << 7) | ((keyMaster[keyOff + 1] & 0xFC) >> 1));
		tmp[tmpOff + 2] = (byte)((keyMaster[keyOff + 1] << 6) | ((keyMaster[keyOff + 2] & 0xF8) >> 2));
		tmp[tmpOff + 3] = (byte)((keyMaster[keyOff + 2] << 5) | ((keyMaster[keyOff + 3] & 0xF0) >> 3));
		tmp[tmpOff + 4] = (byte)((keyMaster[keyOff + 3] << 4) | ((keyMaster[keyOff + 4] & 0xE0) >> 4));
		tmp[tmpOff + 5] = (byte)((keyMaster[keyOff + 4] << 3) | ((keyMaster[keyOff + 5] & 0xC0) >> 5));
		tmp[tmpOff + 6] = (byte)((keyMaster[keyOff + 5] << 2) | ((keyMaster[keyOff + 6] & 0x80) >> 6));
		tmp[tmpOff + 7] = (byte)(keyMaster[keyOff + 6] << 1);
		DesParameters.SetOddParity(tmp, tmpOff, 8);
	}
}
