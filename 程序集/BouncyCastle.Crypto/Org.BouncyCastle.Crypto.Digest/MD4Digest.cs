using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class MD4Digest : GeneralDigest
{
	private const int DigestLength = 16;

	private const int S11 = 3;

	private const int S12 = 7;

	private const int S13 = 11;

	private const int S14 = 19;

	private const int S21 = 3;

	private const int S22 = 5;

	private const int S23 = 9;

	private const int S24 = 13;

	private const int S31 = 3;

	private const int S32 = 9;

	private const int S33 = 11;

	private const int S34 = 15;

	private int H1;

	private int H2;

	private int H3;

	private int H4;

	private int[] X = new int[16];

	private int xOff;

	public override string AlgorithmName => "MD4";

	public MD4Digest()
	{
		Reset();
	}

	public MD4Digest(MD4Digest t)
		: base(t)
	{
		CopyIn(t);
	}

	private void CopyIn(MD4Digest t)
	{
		CopyIn((GeneralDigest)t);
		H1 = t.H1;
		H2 = t.H2;
		H3 = t.H3;
		H4 = t.H4;
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
		UnpackWord(H1, output, outOff);
		UnpackWord(H2, output, outOff + 4);
		UnpackWord(H3, output, outOff + 8);
		UnpackWord(H4, output, outOff + 12);
		Reset();
		return 16;
	}

	public override void Reset()
	{
		base.Reset();
		H1 = 1732584193;
		H2 = -271733879;
		H3 = -1732584194;
		H4 = 271733878;
		xOff = 0;
		for (int i = 0; i != X.Length; i++)
		{
			X[i] = 0;
		}
	}

	private int RotateLeft(int x, int n)
	{
		return (x << n) | (int)((uint)x >> 32 - n);
	}

	private int F(int u, int v, int w)
	{
		return (u & v) | (~u & w);
	}

	private int G(int u, int v, int w)
	{
		return (u & v) | (u & w) | (v & w);
	}

	private int H(int u, int v, int w)
	{
		return u ^ v ^ w;
	}

	internal override void ProcessBlock()
	{
		int h = H1;
		int h2 = H2;
		int h3 = H3;
		int h4 = H4;
		h = RotateLeft(h + F(h2, h3, h4) + X[0], 3);
		h4 = RotateLeft(h4 + F(h, h2, h3) + X[1], 7);
		h3 = RotateLeft(h3 + F(h4, h, h2) + X[2], 11);
		h2 = RotateLeft(h2 + F(h3, h4, h) + X[3], 19);
		h = RotateLeft(h + F(h2, h3, h4) + X[4], 3);
		h4 = RotateLeft(h4 + F(h, h2, h3) + X[5], 7);
		h3 = RotateLeft(h3 + F(h4, h, h2) + X[6], 11);
		h2 = RotateLeft(h2 + F(h3, h4, h) + X[7], 19);
		h = RotateLeft(h + F(h2, h3, h4) + X[8], 3);
		h4 = RotateLeft(h4 + F(h, h2, h3) + X[9], 7);
		h3 = RotateLeft(h3 + F(h4, h, h2) + X[10], 11);
		h2 = RotateLeft(h2 + F(h3, h4, h) + X[11], 19);
		h = RotateLeft(h + F(h2, h3, h4) + X[12], 3);
		h4 = RotateLeft(h4 + F(h, h2, h3) + X[13], 7);
		h3 = RotateLeft(h3 + F(h4, h, h2) + X[14], 11);
		h2 = RotateLeft(h2 + F(h3, h4, h) + X[15], 19);
		h = RotateLeft(h + G(h2, h3, h4) + X[0] + 1518500249, 3);
		h4 = RotateLeft(h4 + G(h, h2, h3) + X[4] + 1518500249, 5);
		h3 = RotateLeft(h3 + G(h4, h, h2) + X[8] + 1518500249, 9);
		h2 = RotateLeft(h2 + G(h3, h4, h) + X[12] + 1518500249, 13);
		h = RotateLeft(h + G(h2, h3, h4) + X[1] + 1518500249, 3);
		h4 = RotateLeft(h4 + G(h, h2, h3) + X[5] + 1518500249, 5);
		h3 = RotateLeft(h3 + G(h4, h, h2) + X[9] + 1518500249, 9);
		h2 = RotateLeft(h2 + G(h3, h4, h) + X[13] + 1518500249, 13);
		h = RotateLeft(h + G(h2, h3, h4) + X[2] + 1518500249, 3);
		h4 = RotateLeft(h4 + G(h, h2, h3) + X[6] + 1518500249, 5);
		h3 = RotateLeft(h3 + G(h4, h, h2) + X[10] + 1518500249, 9);
		h2 = RotateLeft(h2 + G(h3, h4, h) + X[14] + 1518500249, 13);
		h = RotateLeft(h + G(h2, h3, h4) + X[3] + 1518500249, 3);
		h4 = RotateLeft(h4 + G(h, h2, h3) + X[7] + 1518500249, 5);
		h3 = RotateLeft(h3 + G(h4, h, h2) + X[11] + 1518500249, 9);
		h2 = RotateLeft(h2 + G(h3, h4, h) + X[15] + 1518500249, 13);
		h = RotateLeft(h + H(h2, h3, h4) + X[0] + 1859775393, 3);
		h4 = RotateLeft(h4 + H(h, h2, h3) + X[8] + 1859775393, 9);
		h3 = RotateLeft(h3 + H(h4, h, h2) + X[4] + 1859775393, 11);
		h2 = RotateLeft(h2 + H(h3, h4, h) + X[12] + 1859775393, 15);
		h = RotateLeft(h + H(h2, h3, h4) + X[2] + 1859775393, 3);
		h4 = RotateLeft(h4 + H(h, h2, h3) + X[10] + 1859775393, 9);
		h3 = RotateLeft(h3 + H(h4, h, h2) + X[6] + 1859775393, 11);
		h2 = RotateLeft(h2 + H(h3, h4, h) + X[14] + 1859775393, 15);
		h = RotateLeft(h + H(h2, h3, h4) + X[1] + 1859775393, 3);
		h4 = RotateLeft(h4 + H(h, h2, h3) + X[9] + 1859775393, 9);
		h3 = RotateLeft(h3 + H(h4, h, h2) + X[5] + 1859775393, 11);
		h2 = RotateLeft(h2 + H(h3, h4, h) + X[13] + 1859775393, 15);
		h = RotateLeft(h + H(h2, h3, h4) + X[3] + 1859775393, 3);
		h4 = RotateLeft(h4 + H(h, h2, h3) + X[11] + 1859775393, 9);
		h3 = RotateLeft(h3 + H(h4, h, h2) + X[7] + 1859775393, 11);
		h2 = RotateLeft(h2 + H(h3, h4, h) + X[15] + 1859775393, 15);
		H1 += h;
		H2 += h2;
		H3 += h3;
		H4 += h4;
		xOff = 0;
		for (int i = 0; i != X.Length; i++)
		{
			X[i] = 0;
		}
	}

	public override IMemoable Copy()
	{
		return new MD4Digest(this);
	}

	public override void Reset(IMemoable other)
	{
		MD4Digest t = (MD4Digest)other;
		CopyIn(t);
	}
}
