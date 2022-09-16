using System;

namespace Org.BouncyCastle.Crypto.Digests;

public class ShortenedDigest : IDigest
{
	private IDigest baseDigest;

	private int length;

	public string AlgorithmName => baseDigest.AlgorithmName + "(" + length * 8 + ")";

	public ShortenedDigest(IDigest baseDigest, int length)
	{
		if (baseDigest == null)
		{
			throw new ArgumentNullException("baseDigest");
		}
		if (length > baseDigest.GetDigestSize())
		{
			throw new ArgumentException("baseDigest output not large enough to support length");
		}
		this.baseDigest = baseDigest;
		this.length = length;
	}

	public int GetDigestSize()
	{
		return length;
	}

	public void Update(byte input)
	{
		baseDigest.Update(input);
	}

	public void BlockUpdate(byte[] input, int inOff, int length)
	{
		baseDigest.BlockUpdate(input, inOff, length);
	}

	public int DoFinal(byte[] output, int outOff)
	{
		byte[] array = new byte[baseDigest.GetDigestSize()];
		baseDigest.DoFinal(array, 0);
		Array.Copy(array, 0, output, outOff, length);
		return length;
	}

	public void Reset()
	{
		baseDigest.Reset();
	}

	public int GetByteLength()
	{
		return baseDigest.GetByteLength();
	}
}
