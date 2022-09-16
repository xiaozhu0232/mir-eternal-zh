using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class RipeMD256Digest : GeneralDigest
{
	private const int DigestLength = 32;

	private int H0;

	private int H1;

	private int H2;

	private int H3;

	private int H4;

	private int H5;

	private int H6;

	private int H7;

	private int[] X = new int[16];

	private int xOff;

	public override string AlgorithmName => "RIPEMD256";

	public override int GetDigestSize()
	{
		return 32;
	}

	public RipeMD256Digest()
	{
		Reset();
	}

	public RipeMD256Digest(RipeMD256Digest t)
		: base(t)
	{
		CopyIn(t);
	}

	private void CopyIn(RipeMD256Digest t)
	{
		CopyIn((GeneralDigest)t);
		H0 = t.H0;
		H1 = t.H1;
		H2 = t.H2;
		H3 = t.H3;
		H4 = t.H4;
		H5 = t.H5;
		H6 = t.H6;
		H7 = t.H7;
		Array.Copy(t.X, 0, X, 0, t.X.Length);
		xOff = t.xOff;
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
		UnpackWord(H4, output, outOff + 16);
		UnpackWord(H5, output, outOff + 20);
		UnpackWord(H6, output, outOff + 24);
		UnpackWord(H7, output, outOff + 28);
		Reset();
		return 32;
	}

	public override void Reset()
	{
		base.Reset();
		H0 = 1732584193;
		H1 = -271733879;
		H2 = -1732584194;
		H3 = 271733878;
		H4 = 1985229328;
		H5 = -19088744;
		H6 = -1985229329;
		H7 = 19088743;
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
		int h = H0;
		int h2 = H1;
		int h3 = H2;
		int h4 = H3;
		int h5 = H4;
		int h6 = H5;
		int h7 = H6;
		int h8 = H7;
		h = F1(h, h2, h3, h4, X[0], 11);
		h4 = F1(h4, h, h2, h3, X[1], 14);
		h3 = F1(h3, h4, h, h2, X[2], 15);
		h2 = F1(h2, h3, h4, h, X[3], 12);
		h = F1(h, h2, h3, h4, X[4], 5);
		h4 = F1(h4, h, h2, h3, X[5], 8);
		h3 = F1(h3, h4, h, h2, X[6], 7);
		h2 = F1(h2, h3, h4, h, X[7], 9);
		h = F1(h, h2, h3, h4, X[8], 11);
		h4 = F1(h4, h, h2, h3, X[9], 13);
		h3 = F1(h3, h4, h, h2, X[10], 14);
		h2 = F1(h2, h3, h4, h, X[11], 15);
		h = F1(h, h2, h3, h4, X[12], 6);
		h4 = F1(h4, h, h2, h3, X[13], 7);
		h3 = F1(h3, h4, h, h2, X[14], 9);
		h2 = F1(h2, h3, h4, h, X[15], 8);
		h5 = FF4(h5, h6, h7, h8, X[5], 8);
		h8 = FF4(h8, h5, h6, h7, X[14], 9);
		h7 = FF4(h7, h8, h5, h6, X[7], 9);
		h6 = FF4(h6, h7, h8, h5, X[0], 11);
		h5 = FF4(h5, h6, h7, h8, X[9], 13);
		h8 = FF4(h8, h5, h6, h7, X[2], 15);
		h7 = FF4(h7, h8, h5, h6, X[11], 15);
		h6 = FF4(h6, h7, h8, h5, X[4], 5);
		h5 = FF4(h5, h6, h7, h8, X[13], 7);
		h8 = FF4(h8, h5, h6, h7, X[6], 7);
		h7 = FF4(h7, h8, h5, h6, X[15], 8);
		h6 = FF4(h6, h7, h8, h5, X[8], 11);
		h5 = FF4(h5, h6, h7, h8, X[1], 14);
		h8 = FF4(h8, h5, h6, h7, X[10], 14);
		h7 = FF4(h7, h8, h5, h6, X[3], 12);
		h6 = FF4(h6, h7, h8, h5, X[12], 6);
		int num = h;
		h = h5;
		h5 = num;
		h = F2(h, h2, h3, h4, X[7], 7);
		h4 = F2(h4, h, h2, h3, X[4], 6);
		h3 = F2(h3, h4, h, h2, X[13], 8);
		h2 = F2(h2, h3, h4, h, X[1], 13);
		h = F2(h, h2, h3, h4, X[10], 11);
		h4 = F2(h4, h, h2, h3, X[6], 9);
		h3 = F2(h3, h4, h, h2, X[15], 7);
		h2 = F2(h2, h3, h4, h, X[3], 15);
		h = F2(h, h2, h3, h4, X[12], 7);
		h4 = F2(h4, h, h2, h3, X[0], 12);
		h3 = F2(h3, h4, h, h2, X[9], 15);
		h2 = F2(h2, h3, h4, h, X[5], 9);
		h = F2(h, h2, h3, h4, X[2], 11);
		h4 = F2(h4, h, h2, h3, X[14], 7);
		h3 = F2(h3, h4, h, h2, X[11], 13);
		h2 = F2(h2, h3, h4, h, X[8], 12);
		h5 = FF3(h5, h6, h7, h8, X[6], 9);
		h8 = FF3(h8, h5, h6, h7, X[11], 13);
		h7 = FF3(h7, h8, h5, h6, X[3], 15);
		h6 = FF3(h6, h7, h8, h5, X[7], 7);
		h5 = FF3(h5, h6, h7, h8, X[0], 12);
		h8 = FF3(h8, h5, h6, h7, X[13], 8);
		h7 = FF3(h7, h8, h5, h6, X[5], 9);
		h6 = FF3(h6, h7, h8, h5, X[10], 11);
		h5 = FF3(h5, h6, h7, h8, X[14], 7);
		h8 = FF3(h8, h5, h6, h7, X[15], 7);
		h7 = FF3(h7, h8, h5, h6, X[8], 12);
		h6 = FF3(h6, h7, h8, h5, X[12], 7);
		h5 = FF3(h5, h6, h7, h8, X[4], 6);
		h8 = FF3(h8, h5, h6, h7, X[9], 15);
		h7 = FF3(h7, h8, h5, h6, X[1], 13);
		h6 = FF3(h6, h7, h8, h5, X[2], 11);
		num = h2;
		h2 = h6;
		h6 = num;
		h = F3(h, h2, h3, h4, X[3], 11);
		h4 = F3(h4, h, h2, h3, X[10], 13);
		h3 = F3(h3, h4, h, h2, X[14], 6);
		h2 = F3(h2, h3, h4, h, X[4], 7);
		h = F3(h, h2, h3, h4, X[9], 14);
		h4 = F3(h4, h, h2, h3, X[15], 9);
		h3 = F3(h3, h4, h, h2, X[8], 13);
		h2 = F3(h2, h3, h4, h, X[1], 15);
		h = F3(h, h2, h3, h4, X[2], 14);
		h4 = F3(h4, h, h2, h3, X[7], 8);
		h3 = F3(h3, h4, h, h2, X[0], 13);
		h2 = F3(h2, h3, h4, h, X[6], 6);
		h = F3(h, h2, h3, h4, X[13], 5);
		h4 = F3(h4, h, h2, h3, X[11], 12);
		h3 = F3(h3, h4, h, h2, X[5], 7);
		h2 = F3(h2, h3, h4, h, X[12], 5);
		h5 = FF2(h5, h6, h7, h8, X[15], 9);
		h8 = FF2(h8, h5, h6, h7, X[5], 7);
		h7 = FF2(h7, h8, h5, h6, X[1], 15);
		h6 = FF2(h6, h7, h8, h5, X[3], 11);
		h5 = FF2(h5, h6, h7, h8, X[7], 8);
		h8 = FF2(h8, h5, h6, h7, X[14], 6);
		h7 = FF2(h7, h8, h5, h6, X[6], 6);
		h6 = FF2(h6, h7, h8, h5, X[9], 14);
		h5 = FF2(h5, h6, h7, h8, X[11], 12);
		h8 = FF2(h8, h5, h6, h7, X[8], 13);
		h7 = FF2(h7, h8, h5, h6, X[12], 5);
		h6 = FF2(h6, h7, h8, h5, X[2], 14);
		h5 = FF2(h5, h6, h7, h8, X[10], 13);
		h8 = FF2(h8, h5, h6, h7, X[0], 13);
		h7 = FF2(h7, h8, h5, h6, X[4], 7);
		h6 = FF2(h6, h7, h8, h5, X[13], 5);
		num = h3;
		h3 = h7;
		h7 = num;
		h = F4(h, h2, h3, h4, X[1], 11);
		h4 = F4(h4, h, h2, h3, X[9], 12);
		h3 = F4(h3, h4, h, h2, X[11], 14);
		h2 = F4(h2, h3, h4, h, X[10], 15);
		h = F4(h, h2, h3, h4, X[0], 14);
		h4 = F4(h4, h, h2, h3, X[8], 15);
		h3 = F4(h3, h4, h, h2, X[12], 9);
		h2 = F4(h2, h3, h4, h, X[4], 8);
		h = F4(h, h2, h3, h4, X[13], 9);
		h4 = F4(h4, h, h2, h3, X[3], 14);
		h3 = F4(h3, h4, h, h2, X[7], 5);
		h2 = F4(h2, h3, h4, h, X[15], 6);
		h = F4(h, h2, h3, h4, X[14], 8);
		h4 = F4(h4, h, h2, h3, X[5], 6);
		h3 = F4(h3, h4, h, h2, X[6], 5);
		h2 = F4(h2, h3, h4, h, X[2], 12);
		h5 = FF1(h5, h6, h7, h8, X[8], 15);
		h8 = FF1(h8, h5, h6, h7, X[6], 5);
		h7 = FF1(h7, h8, h5, h6, X[4], 8);
		h6 = FF1(h6, h7, h8, h5, X[1], 11);
		h5 = FF1(h5, h6, h7, h8, X[3], 14);
		h8 = FF1(h8, h5, h6, h7, X[11], 14);
		h7 = FF1(h7, h8, h5, h6, X[15], 6);
		h6 = FF1(h6, h7, h8, h5, X[0], 14);
		h5 = FF1(h5, h6, h7, h8, X[5], 6);
		h8 = FF1(h8, h5, h6, h7, X[12], 9);
		h7 = FF1(h7, h8, h5, h6, X[2], 12);
		h6 = FF1(h6, h7, h8, h5, X[13], 9);
		h5 = FF1(h5, h6, h7, h8, X[9], 12);
		h8 = FF1(h8, h5, h6, h7, X[7], 5);
		h7 = FF1(h7, h8, h5, h6, X[10], 15);
		h6 = FF1(h6, h7, h8, h5, X[14], 8);
		num = h4;
		h4 = h8;
		h8 = num;
		H0 += h;
		H1 += h2;
		H2 += h3;
		H3 += h4;
		H4 += h5;
		H5 += h6;
		H6 += h7;
		H7 += h8;
		xOff = 0;
		for (int i = 0; i != X.Length; i++)
		{
			X[i] = 0;
		}
	}

	public override IMemoable Copy()
	{
		return new RipeMD256Digest(this);
	}

	public override void Reset(IMemoable other)
	{
		RipeMD256Digest t = (RipeMD256Digest)other;
		CopyIn(t);
	}
}
