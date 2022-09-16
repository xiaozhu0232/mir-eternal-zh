using System;

namespace Org.BouncyCastle.Crypto.Digests;

public class NonMemoableDigest : IDigest
{
	protected readonly IDigest mBaseDigest;

	public virtual string AlgorithmName => mBaseDigest.AlgorithmName;

	public NonMemoableDigest(IDigest baseDigest)
	{
		if (baseDigest == null)
		{
			throw new ArgumentNullException("baseDigest");
		}
		mBaseDigest = baseDigest;
	}

	public virtual int GetDigestSize()
	{
		return mBaseDigest.GetDigestSize();
	}

	public virtual void Update(byte input)
	{
		mBaseDigest.Update(input);
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int len)
	{
		mBaseDigest.BlockUpdate(input, inOff, len);
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		return mBaseDigest.DoFinal(output, outOff);
	}

	public virtual void Reset()
	{
		mBaseDigest.Reset();
	}

	public virtual int GetByteLength()
	{
		return mBaseDigest.GetByteLength();
	}
}
