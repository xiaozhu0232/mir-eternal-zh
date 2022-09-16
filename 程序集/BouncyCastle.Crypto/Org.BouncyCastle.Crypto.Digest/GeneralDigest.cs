using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public abstract class GeneralDigest : IDigest, IMemoable
{
	private const int BYTE_LENGTH = 64;

	private byte[] xBuf;

	private int xBufOff;

	private long byteCount;

	public abstract string AlgorithmName { get; }

	internal GeneralDigest()
	{
		xBuf = new byte[4];
	}

	internal GeneralDigest(GeneralDigest t)
	{
		xBuf = new byte[t.xBuf.Length];
		CopyIn(t);
	}

	protected void CopyIn(GeneralDigest t)
	{
		Array.Copy(t.xBuf, 0, xBuf, 0, t.xBuf.Length);
		xBufOff = t.xBufOff;
		byteCount = t.byteCount;
	}

	public void Update(byte input)
	{
		xBuf[xBufOff++] = input;
		if (xBufOff == xBuf.Length)
		{
			ProcessWord(xBuf, 0);
			xBufOff = 0;
		}
		byteCount++;
	}

	public void BlockUpdate(byte[] input, int inOff, int length)
	{
		length = System.Math.Max(0, length);
		int i = 0;
		if (xBufOff != 0)
		{
			while (i < length)
			{
				xBuf[xBufOff++] = input[inOff + i++];
				if (xBufOff == 4)
				{
					ProcessWord(xBuf, 0);
					xBufOff = 0;
					break;
				}
			}
		}
		for (int num = ((length - i) & -4) + i; i < num; i += 4)
		{
			ProcessWord(input, inOff + i);
		}
		while (i < length)
		{
			xBuf[xBufOff++] = input[inOff + i++];
		}
		byteCount += length;
	}

	public void Finish()
	{
		long bitLength = byteCount << 3;
		Update(128);
		while (xBufOff != 0)
		{
			Update(0);
		}
		ProcessLength(bitLength);
		ProcessBlock();
	}

	public virtual void Reset()
	{
		byteCount = 0L;
		xBufOff = 0;
		Array.Clear(xBuf, 0, xBuf.Length);
	}

	public int GetByteLength()
	{
		return 64;
	}

	internal abstract void ProcessWord(byte[] input, int inOff);

	internal abstract void ProcessLength(long bitLength);

	internal abstract void ProcessBlock();

	public abstract int GetDigestSize();

	public abstract int DoFinal(byte[] output, int outOff);

	public abstract IMemoable Copy();

	public abstract void Reset(IMemoable t);
}
