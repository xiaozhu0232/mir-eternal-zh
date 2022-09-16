using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class RipeMD128Digest : GeneralDigest
{
	private const int DigestLength = 16;

	private int H0;

	private int H1;

	private int H2;

	private int H3;

	private int[] X = new int[16];

	private int xOff;

	public override string AlgorithmName => "RIPEMD128";

	public RipeMD128Digest()
	{
		Reset();
	}

	public RipeMD128Digest(RipeMD128Digest t)
		: base(t)
	{
		CopyIn(t);
	}

	private void CopyIn(RipeMD128Digest t)
	{
		CopyIn((GeneralDigest)t);
		H0 = t.H0;
		H1 = t.H1;
		H2 = t.H2;
		H3 = t.H3;
		Array.Copy(t.X, 0, X, 0, t.X.Length);
		xOff = t.xOff;
	}

	public override int GetDigestSize()
	{
		return 16;
	}

	internal override void ProcessWord(byte[] input, int inOff)
	{
		X[xOff++] = (input[inOff] & 0xFF) | ((input[inOff + 1] & 0xFF) << 8) | ((input[inOff + 2] & 0xFF) << 16) | ((input[inOff + 3] & 0xFF) << 24);
		if (xOff == 16)
		{
			ProcessBlock();
		}
	}

	internal override void ProcessLength(long bitLength)
	{
		if (xOff > 14)
		{
			ProcessBlock();
		}
		X[14] = (int)(bitLength & 0xFFFFFFFFu);
		X[15] = (int)((ulong)bitLength >> 32);
	}

	private void UnpackWord(int word, byte[] outBytes, int outOff)
	{
		outBytes[outOff] = (byte)word;
		outBytes[outOff + 1] = (byte)((uint)word >> 8);
		outBytes[outOff + 2] = (byte)((uint)word >> 16);
		outBytes[outOff + 3] = (byte)((uint)word >> 24);
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		Finish();
		UnpackWord(H0, output, outOff);
		UnpackWord(H1, output, outOff + 4);
		UnpackWord(H2, output, outOff + 8);
		UnpackWord(H3, output, outOff + 12);
		Reset();
		return 16;
	}

	public override void Reset()
	{
		base.Reset();
		H0 = 1732584193;
		H1 = -271733879;
		H2 = -1732584194;
		H3 = 271733878;
		xOff = 0;
		for (int i = 0; i != X.Length; i++)
		{
			X[i] = 0;
		}
	}

	private int RL(int x, int n)
	{
		return (x << n) | (int)((uint)x >> 32 - n);
	}

	private int F1(int x, int y, int z)
	{
		return x ^ y ^ z;
	}

	private int F2(int x, int y, int z)
	{
		return (x & y) | (~x & z);
	}

	private int F3(int x, int y, int z)
	{
		return (x | ~y) ^ z;
	}

	private int F4(int x, int y, int z)
	{
		return (x & z) | (y & ~z);
	}

	private int F1(int a, int b, int c, int d, int x, int s)
	{
		return RL(a + F1(b, c, d) + x, s);
	}

	private int F2(int a, int b, int c, int d, int x, int s)
	{
		return RL(a + F2(b, c, d) + x + 1518500249, s);
	}

	private int F3(int a, int b, int c, int d, int x, int s)
	{
		return RL(a + F3(b, c, d) + x + 1859775393, s);
	}

	private int F4(int a, int b, int c, int d, int x, int s)
	{
		return RL(a + F4(b, c, d) + x + -1894007588, s);
	}

	private int FF1(int a, int b, int c, int d, int x, int s)
	{
		return RL(a + F1(b, c, d) + x, s);
	}

	private int FF2(int a, int b, int c, int d, int x, int s)
	{
		return RL(a + F2(b, c, d) + x + 1836072691, s);
	}

	private int FF3(int a, int b, int c, int d, int x, int s)
	{
		return RL(a + F3(b, c, d) + x + 1548603684, s);
	}

	private int FF4(int a, int b, int c, int d, int x, int s)
	{
		return RL(a + F4(b, c, d) + x + 1352829926, s);
	}

	internal override void ProcessBlock()
	{
		int h;
		int a = (h = H0);
		int h2;
		int num = (h2 = H1);
		int h3;
		int num2 = (h3 = H2);
		int h4;
		int num3 = (h4 = H3);
		a = F1(a, num, num2, num3, X[0], 11);
		num3 = F1(num3, a, num, num2, X[1], 14);
		num2 = F1(num2, num3, a, num, X[2], 15);
		num = F1(num, num2, num3, a, X[3], 12);
		a = F1(a, num, num2, num3, X[4], 5);
		num3 = F1(num3, a, num, num2, X[5], 8);
		num2 = F1(num2, num3, a, num, X[6], 7);
		num = F1(num, num2, num3, a, X[7], 9);
		a = F1(a, num, num2, num3, X[8], 11);
		num3 = F1(num3, a, num, num2, X[9], 13);
		num2 = F1(num2, num3, a, num, X[10], 14);
		num = F1(num, num2, num3, a, X[11], 15);
		a = F1(a, num, num2, num3, X[12], 6);
		num3 = F1(num3, a, num, num2, X[13], 7);
		num2 = F1(num2, num3, a, num, X[14], 9);
		num = F1(num, num2, num3, a, X[15], 8);
		a = F2(a, num, num2, num3, X[7], 7);
		num3 = F2(num3, a, num, num2, X[4], 6);
		num2 = F2(num2, num3, a, num, X[13], 8);
		num = F2(num, num2, num3, a, X[1], 13);
		a = F2(a, num, num2, num3, X[10], 11);
		num3 = F2(num3, a, num, num2, X[6], 9);
		num2 = F2(num2, num3, a, num, X[15], 7);
		num = F2(num, num2, num3, a, X[3], 15);
		a = F2(a, num, num2, num3, X[12], 7);
		num3 = F2(num3, a, num, num2, X[0], 12);
		num2 = F2(num2, num3, a, num, X[9], 15);
		num = F2(num, num2, num3, a, X[5], 9);
		a = F2(a, num, num2, num3, X[2], 11);
		num3 = F2(num3, a, num, num2, X[14], 7);
		num2 = F2(num2, num3, a, num, X[11], 13);
		num = F2(num, num2, num3, a, X[8], 12);
		a = F3(a, num, num2, num3, X[3], 11);
		num3 = F3(num3, a, num, num2, X[10], 13);
		num2 = F3(num2, num3, a, num, X[14], 6);
		num = F3(num, num2, num3, a, X[4], 7);
		a = F3(a, num, num2, num3, X[9], 14);
		num3 = F3(num3, a, num, num2, X[15], 9);
		num2 = F3(num2, num3, a, num, X[8], 13);
		num = F3(num, num2, num3, a, X[1], 15);
		a = F3(a, num, num2, num3, X[2], 14);
		num3 = F3(num3, a, num, num2, X[7], 8);
		num2 = F3(num2, num3, a, num, X[0], 13);
		num = F3(num, num2, num3, a, X[6], 6);
		a = F3(a, num, num2, num3, X[13], 5);
		num3 = F3(num3, a, num, num2, X[11], 12);
		num2 = F3(num2, num3, a, num, X[5], 7);
		num = F3(num, num2, num3, a, X[12], 5);
		a = F4(a, num, num2, num3, X[1], 11);
		num3 = F4(num3, a, num, num2, X[9], 12);
		num2 = F4(num2, num3, a, num, X[11], 14);
		num = F4(num, num2, num3, a, X[10], 15);
		a = F4(a, num, num2, num3, X[0], 14);
		num3 = F4(num3, a, num, num2, X[8], 15);
		num2 = F4(num2, num3, a, num, X[12], 9);
		num = F4(num, num2, num3, a, X[4], 8);
		a = F4(a, num, num2, num3, X[13], 9);
		num3 = F4(num3, a, num, num2, X[3], 14);
		num2 = F4(num2, num3, a, num, X[7], 5);
		num = F4(num, num2, num3, a, X[15], 6);
		a = F4(a, num, num2, num3, X[14], 8);
		num3 = F4(num3, a, num, num2, X[5], 6);
		num2 = F4(num2, num3, a, num, X[6], 5);
		num = F4(num, num2, num3, a, X[2], 12);
		h = FF4(h, h2, h3, h4, X[5], 8);
		h4 = FF4(h4, h, h2, h3, X[14], 9);
		h3 = FF4(h3, h4, h, h2, X[7], 9);
		h2 = FF4(h2, h3, h4, h, X[0], 11);
		h = FF4(h, h2, h3, h4, X[9], 13);
		h4 = FF4(h4, h, h2, h3, X[2], 15);
		h3 = FF4(h3, h4, h, h2, X[11], 15);
		h2 = FF4(h2, h3, h4, h, X[4], 5);
		h = FF4(h, h2, h3, h4, X[13], 7);
		h4 = FF4(h4, h, h2, h3, X[6], 7);
		h3 = FF4(h3, h4, h, h2, X[15], 8);
		h2 = FF4(h2, h3, h4, h, X[8], 11);
		h = FF4(h, h2, h3, h4, X[1], 14);
		h4 = FF4(h4, h, h2, h3, X[10], 14);
		h3 = FF4(h3, h4, h, h2, X[3], 12);
		h2 = FF4(h2, h3, h4, h, X[12], 6);
		h = FF3(h, h2, h3, h4, X[6], 9);
		h4 = FF3(h4, h, h2, h3, X[11], 13);
		h3 = FF3(h3, h4, h, h2, X[3], 15);
		h2 = FF3(h2, h3, h4, h, X[7], 7);
		h = FF3(h, h2, h3, h4, X[0], 12);
		h4 = FF3(h4, h, h2, h3, X[13], 8);
		h3 = FF3(h3, h4, h, h2, X[5], 9);
		h2 = FF3(h2, h3, h4, h, X[10], 11);
		h = FF3(h, h2, h3, h4, X[14], 7);
		h4 = FF3(h4, h, h2, h3, X[15], 7);
		h3 = FF3(h3, h4, h, h2, X[8], 12);
		h2 = FF3(h2, h3, h4, h, X[12], 7);
		h = FF3(h, h2, h3, h4, X[4], 6);
		h4 = FF3(h4, h, h2, h3, X[9], 15);
		h3 = FF3(h3, h4, h, h2, X[1], 13);
		h2 = FF3(h2, h3, h4, h, X[2], 11);
		h = FF2(h, h2, h3, h4, X[15], 9);
		h4 = FF2(h4, h, h2, h3, X[5], 7);
		h3 = FF2(h3, h4, h, h2, X[1], 15);
		h2 = FF2(h2, h3, h4, h, X[3], 11);
		h = FF2(h, h2, h3, h4, X[7], 8);
		h4 = FF2(h4, h, h2, h3, X[14], 6);
		h3 = FF2(h3, h4, h, h2, X[6], 6);
		h2 = FF2(h2, h3, h4, h, X[9], 14);
		h = FF2(h, h2, h3, h4, X[11], 12);
		h4 = FF2(h4, h, h2, h3, X[8], 13);
		h3 = FF2(h3, h4, h, h2, X[12], 5);
		h2 = FF2(h2, h3, h4, h, X[2], 14);
		h = FF2(h, h2, h3, h4, X[10], 13);
		h4 = FF2(h4, h, h2, h3, X[0], 13);
		h3 = FF2(h3, h4, h, h2, X[4], 7);
		h2 = FF2(h2, h3, h4, h, X[13], 5);
		h = FF1(h, h2, h3, h4, X[8], 15);
		h4 = FF1(h4, h, h2, h3, X[6], 5);
		h3 = FF1(h3, h4, h, h2, X[4], 8);
		h2 = FF1(h2, h3, h4, h, X[1], 11);
		h = FF1(h, h2, h3, h4, X[3], 14);
		h4 = FF1(h4, h, h2, h3, X[11], 14);
		h3 = FF1(h3, h4, h, h2, X[15], 6);
		h2 = FF1(h2, h3, h4, h, X[0], 14);
		h = FF1(h, h2, h3, h4, X[5], 6);
		h4 = FF1(h4, h, h2, h3, X[12], 9);
		h3 = FF1(h3, h4, h, h2, X[2], 12);
		h2 = FF1(h2, h3, h4, h, X[13], 9);
		h = FF1(h, h2, h3, h4, X[9], 12);
		h4 = FF1(h4, h, h2, h3, X[7], 5);
		h3 = FF1(h3, h4, h, h2, X[10], 15);
		h2 = FF1(h2, h3, h4, h, X[14], 8);
		h4 += num2 + H1;
		H1 = H2 + num3 + h;
		H2 = H3 + a + h2;
		H3 = H0 + num + h3;
		H0 = h4;
		xOff = 0;
		for (int i = 0; i != X.Length; i++)
		{
			X[i] = 0;
		}
	}

	public override IMemoable Copy()
	{
		return new RipeMD128Digest(this);
	}

	public override void Reset(IMemoable other)
	{
		RipeMD128Digest t = (RipeMD128Digest)other;
		CopyIn(t);
	}
}
