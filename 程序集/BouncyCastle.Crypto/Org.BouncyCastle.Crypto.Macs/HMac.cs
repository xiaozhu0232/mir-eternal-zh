using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Macs;

public class HMac : IMac
{
	private const byte IPAD = 54;

	private const byte OPAD = 92;

	private readonly IDigest digest;

	private readonly int digestSize;

	private readonly int blockLength;

	private IMemoable ipadState;

	private IMemoable opadState;

	private readonly byte[] inputPad;

	private readonly byte[] outputBuf;

	public virtual string AlgorithmName => digest.AlgorithmName + "/HMAC";

	public HMac(IDigest digest)
	{
		this.digest = digest;
		digestSize = digest.GetDigestSize();
		blockLength = digest.GetByteLength();
		inputPad = new byte[blockLength];
		outputBuf = new byte[blockLength + digestSize];
	}

	public virtual IDigest GetUnderlyingDigest()
	{
		return digest;
	}

	public virtual void Init(ICipherParameters parameters)
	{
		digest.Reset();
		byte[] key = ((KeyParameter)parameters).GetKey();
		int num = key.Length;
		if (num > blockLength)
		{
			digest.BlockUpdate(key, 0, num);
			digest.DoFinal(inputPad, 0);
			num = digestSize;
		}
		else
		{
			Array.Copy(key, 0, inputPad, 0, num);
		}
		Array.Clear(inputPad, num, blockLength - num);
		Array.Copy(inputPad, 0, outputBuf, 0, blockLength);
		XorPad(inputPad, blockLength, 54);
		XorPad(outputBuf, blockLength, 92);
		if (digest is IMemoable)
		{
			opadState = ((IMemoable)digest).Copy();
			((IDigest)opadState).BlockUpdate(outputBuf, 0, blockLength);
		}
		digest.BlockUpdate(inputPad, 0, inputPad.Length);
		if (digest is IMemoable)
		{
			ipadState = ((IMemoable)digest).Copy();
		}
	}

	public virtual int GetMacSize()
	{
		return digestSize;
	}

	public virtual void Update(byte input)
	{
		digest.Update(input);
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int len)
	{
		digest.BlockUpdate(input, inOff, len);
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		digest.DoFinal(outputBuf, blockLength);
		if (opadState != null)
		{
			((IMemoable)digest).Reset(opadState);
			digest.BlockUpdate(outputBuf, blockLength, digest.GetDigestSize());
		}
		else
		{
			digest.BlockUpdate(outputBuf, 0, outputBuf.Length);
		}
		int result = digest.DoFinal(output, outOff);
		Array.Clear(outputBuf, blockLength, digestSize);
		if (ipadState != null)
		{
			((IMemoable)digest).Reset(ipadState);
		}
		else
		{
			digest.BlockUpdate(inputPad, 0, inputPad.Length);
		}
		return result;
	}

	public virtual void Reset()
	{
		digest.Reset();
		digest.BlockUpdate(inputPad, 0, inputPad.Length);
	}

	private static void XorPad(byte[] pad, int len, byte n)
	{
		for (int i = 0; i < len; i++)
		{
			byte[] array;
			byte[] array2 = (array = pad);
			int num = i;
			nint num2 = num;
			array2[num] = (byte)(array[num2] ^ n);
		}
	}
}
