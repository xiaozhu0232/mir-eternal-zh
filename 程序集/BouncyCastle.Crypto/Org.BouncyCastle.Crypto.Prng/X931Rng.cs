using System;

namespace Org.BouncyCastle.Crypto.Prng;

internal class X931Rng
{
	private const long BLOCK64_RESEED_MAX = 32768L;

	private const long BLOCK128_RESEED_MAX = 8388608L;

	private const int BLOCK64_MAX_BITS_REQUEST = 4096;

	private const int BLOCK128_MAX_BITS_REQUEST = 262144;

	private readonly IBlockCipher mEngine;

	private readonly IEntropySource mEntropySource;

	private readonly byte[] mDT;

	private readonly byte[] mI;

	private readonly byte[] mR;

	private byte[] mV;

	private long mReseedCounter = 1L;

	internal IEntropySource EntropySource => mEntropySource;

	internal X931Rng(IBlockCipher engine, byte[] dateTimeVector, IEntropySource entropySource)
	{
		mEngine = engine;
		mEntropySource = entropySource;
		mDT = new byte[engine.GetBlockSize()];
		Array.Copy(dateTimeVector, 0, mDT, 0, mDT.Length);
		mI = new byte[engine.GetBlockSize()];
		mR = new byte[engine.GetBlockSize()];
	}

	internal int Generate(byte[] output, bool predictionResistant)
	{
		if (mR.Length == 8)
		{
			if (mReseedCounter > 32768)
			{
				return -1;
			}
			if (IsTooLarge(output, 512))
			{
				throw new ArgumentException("Number of bits per request limited to " + 4096, "output");
			}
		}
		else
		{
			if (mReseedCounter > 8388608)
			{
				return -1;
			}
			if (IsTooLarge(output, 32768))
			{
				throw new ArgumentException("Number of bits per request limited to " + 262144, "output");
			}
		}
		if (predictionResistant || mV == null)
		{
			mV = mEntropySource.GetEntropy();
			if (mV.Length != mEngine.GetBlockSize())
			{
				throw new InvalidOperationException("Insufficient entropy returned");
			}
		}
		int num = output.Length / mR.Length;
		for (int i = 0; i < num; i++)
		{
			mEngine.ProcessBlock(mDT, 0, mI, 0);
			Process(mR, mI, mV);
			Process(mV, mR, mI);
			Array.Copy(mR, 0, output, i * mR.Length, mR.Length);
			Increment(mDT);
		}
		int num2 = output.Length - num * mR.Length;
		if (num2 > 0)
		{
			mEngine.ProcessBlock(mDT, 0, mI, 0);
			Process(mR, mI, mV);
			Process(mV, mR, mI);
			Array.Copy(mR, 0, output, num * mR.Length, num2);
			Increment(mDT);
		}
		mReseedCounter++;
		return output.Length;
	}

	internal void Reseed()
	{
		mV = mEntropySource.GetEntropy();
		if (mV.Length != mEngine.GetBlockSize())
		{
			throw new InvalidOperationException("Insufficient entropy returned");
		}
		mReseedCounter = 1L;
	}

	private void Process(byte[] res, byte[] a, byte[] b)
	{
		for (int i = 0; i != res.Length; i++)
		{
			res[i] = (byte)(a[i] ^ b[i]);
		}
		mEngine.ProcessBlock(res, 0, res, 0);
	}

	private void Increment(byte[] val)
	{
		for (int num = val.Length - 1; num >= 0; num--)
		{
			byte[] array;
			byte[] array2 = (array = val);
			int num2 = num;
			nint num3 = num2;
			byte b;
			array2[num2] = (b = (byte)(array[num3] + 1));
			if (b != 0)
			{
				break;
			}
		}
	}

	private static bool IsTooLarge(byte[] bytes, int maxBytes)
	{
		if (bytes != null)
		{
			return bytes.Length > maxBytes;
		}
		return false;
	}
}
