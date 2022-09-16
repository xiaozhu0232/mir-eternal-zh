using System;

namespace Org.BouncyCastle.Crypto.Prng;

public abstract class EntropyUtilities
{
	public static byte[] GenerateSeed(IEntropySource entropySource, int numBytes)
	{
		byte[] array = new byte[numBytes];
		int num;
		for (int i = 0; i < numBytes; i += num)
		{
			byte[] entropy = entropySource.GetEntropy();
			num = System.Math.Min(array.Length, numBytes - i);
			Array.Copy(entropy, 0, array, i, num);
		}
		return array;
	}
}
