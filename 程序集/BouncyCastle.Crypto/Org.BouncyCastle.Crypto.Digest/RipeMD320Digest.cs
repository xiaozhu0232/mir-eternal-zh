using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class RipeMD320Digest : GeneralDigest
{
	private const int DigestLength = 40;

	private int H0;

	private int H1;

	private int H2;

	private int H3;

	private int H4;

	private int H5;

	private int H6;

	private int H7;

	private int H8;

	private int H9;

	private int[] X = new int[16];

	private int xOff;

	public override string AlgorithmName => "RIPEMD320";

	public override int GetDigestSize()
	{
		return 40;
	}

	public RipeMD320Digest()
	{
		Reset();
	}

	public RipeMD320Digest(RipeMD320Digest t)
		: base(t)
	{
		CopyIn(t);
	}

	private void CopyIn(RipeMD320Digest t)
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
		H8 = t.H8;
		H9 = t.H9;
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
		UnpackWord(H8, output, outOff + 32);
		UnpackWord(H9, output, outOff + 36);
		Reset();
		return 40;
	}

	public override void Reset()
	{
		base.Reset();
		H0 = 1732584193;
		H1 = -271733879;
		H2 = -1732584194;
		H3 = 271733878;
		H4 = -1009589776;
		H5 = 1985229328;
		H6 = -19088744;
		H7 = -1985229329;
		H8 = 19088743;
		H9 = 1009589775;
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

	private int F5(int x, int y, int z)
	{
		return x ^ (y | ~z);
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
		int h9 = H8;
		int h10 = H9;
		h = RL(h + F1(h2, h3, h4) + X[0], 11) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F1(h, h2, h3) + X[1], 14) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F1(h5, h, h2) + X[2], 15) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F1(h4, h5, h) + X[3], 12) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F1(h3, h4, h5) + X[4], 5) + h;
		h4 = RL(h4, 10);
		h = RL(h + F1(h2, h3, h4) + X[5], 8) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F1(h, h2, h3) + X[6], 7) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F1(h5, h, h2) + X[7], 9) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F1(h4, h5, h) + X[8], 11) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F1(h3, h4, h5) + X[9], 13) + h;
		h4 = RL(h4, 10);
		h = RL(h + F1(h2, h3, h4) + X[10], 14) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F1(h, h2, h3) + X[11], 15) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F1(h5, h, h2) + X[12], 6) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F1(h4, h5, h) + X[13], 7) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F1(h3, h4, h5) + X[14], 9) + h;
		h4 = RL(h4, 10);
		h = RL(h + F1(h2, h3, h4) + X[15], 8) + h5;
		h3 = RL(h3, 10);
		h6 = RL(h6 + F5(h7, h8, h9) + X[5] + 1352829926, 8) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F5(h6, h7, h8) + X[14] + 1352829926, 9) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F5(h10, h6, h7) + X[7] + 1352829926, 9) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F5(h9, h10, h6) + X[0] + 1352829926, 11) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F5(h8, h9, h10) + X[9] + 1352829926, 13) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F5(h7, h8, h9) + X[2] + 1352829926, 15) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F5(h6, h7, h8) + X[11] + 1352829926, 15) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F5(h10, h6, h7) + X[4] + 1352829926, 5) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F5(h9, h10, h6) + X[13] + 1352829926, 7) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F5(h8, h9, h10) + X[6] + 1352829926, 7) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F5(h7, h8, h9) + X[15] + 1352829926, 8) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F5(h6, h7, h8) + X[8] + 1352829926, 11) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F5(h10, h6, h7) + X[1] + 1352829926, 14) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F5(h9, h10, h6) + X[10] + 1352829926, 14) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F5(h8, h9, h10) + X[3] + 1352829926, 12) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F5(h7, h8, h9) + X[12] + 1352829926, 6) + h10;
		h8 = RL(h8, 10);
		int num = h;
		h = h6;
		h6 = num;
		h5 = RL(h5 + F2(h, h2, h3) + X[7] + 1518500249, 7) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F2(h5, h, h2) + X[4] + 1518500249, 6) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F2(h4, h5, h) + X[13] + 1518500249, 8) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F2(h3, h4, h5) + X[1] + 1518500249, 13) + h;
		h4 = RL(h4, 10);
		h = RL(h + F2(h2, h3, h4) + X[10] + 1518500249, 11) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F2(h, h2, h3) + X[6] + 1518500249, 9) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F2(h5, h, h2) + X[15] + 1518500249, 7) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F2(h4, h5, h) + X[3] + 1518500249, 15) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F2(h3, h4, h5) + X[12] + 1518500249, 7) + h;
		h4 = RL(h4, 10);
		h = RL(h + F2(h2, h3, h4) + X[0] + 1518500249, 12) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F2(h, h2, h3) + X[9] + 1518500249, 15) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F2(h5, h, h2) + X[5] + 1518500249, 9) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F2(h4, h5, h) + X[2] + 1518500249, 11) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F2(h3, h4, h5) + X[14] + 1518500249, 7) + h;
		h4 = RL(h4, 10);
		h = RL(h + F2(h2, h3, h4) + X[11] + 1518500249, 13) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F2(h, h2, h3) + X[8] + 1518500249, 12) + h4;
		h2 = RL(h2, 10);
		h10 = RL(h10 + F4(h6, h7, h8) + X[6] + 1548603684, 9) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F4(h10, h6, h7) + X[11] + 1548603684, 13) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F4(h9, h10, h6) + X[3] + 1548603684, 15) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F4(h8, h9, h10) + X[7] + 1548603684, 7) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F4(h7, h8, h9) + X[0] + 1548603684, 12) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F4(h6, h7, h8) + X[13] + 1548603684, 8) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F4(h10, h6, h7) + X[5] + 1548603684, 9) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F4(h9, h10, h6) + X[10] + 1548603684, 11) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F4(h8, h9, h10) + X[14] + 1548603684, 7) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F4(h7, h8, h9) + X[15] + 1548603684, 7) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F4(h6, h7, h8) + X[8] + 1548603684, 12) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F4(h10, h6, h7) + X[12] + 1548603684, 7) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F4(h9, h10, h6) + X[4] + 1548603684, 6) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F4(h8, h9, h10) + X[9] + 1548603684, 15) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F4(h7, h8, h9) + X[1] + 1548603684, 13) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F4(h6, h7, h8) + X[2] + 1548603684, 11) + h9;
		h7 = RL(h7, 10);
		num = h2;
		h2 = h7;
		h7 = num;
		h4 = RL(h4 + F3(h5, h, h2) + X[3] + 1859775393, 11) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F3(h4, h5, h) + X[10] + 1859775393, 13) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F3(h3, h4, h5) + X[14] + 1859775393, 6) + h;
		h4 = RL(h4, 10);
		h = RL(h + F3(h2, h3, h4) + X[4] + 1859775393, 7) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F3(h, h2, h3) + X[9] + 1859775393, 14) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F3(h5, h, h2) + X[15] + 1859775393, 9) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F3(h4, h5, h) + X[8] + 1859775393, 13) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F3(h3, h4, h5) + X[1] + 1859775393, 15) + h;
		h4 = RL(h4, 10);
		h = RL(h + F3(h2, h3, h4) + X[2] + 1859775393, 14) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F3(h, h2, h3) + X[7] + 1859775393, 8) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F3(h5, h, h2) + X[0] + 1859775393, 13) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F3(h4, h5, h) + X[6] + 1859775393, 6) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F3(h3, h4, h5) + X[13] + 1859775393, 5) + h;
		h4 = RL(h4, 10);
		h = RL(h + F3(h2, h3, h4) + X[11] + 1859775393, 12) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F3(h, h2, h3) + X[5] + 1859775393, 7) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F3(h5, h, h2) + X[12] + 1859775393, 5) + h3;
		h = RL(h, 10);
		h9 = RL(h9 + F3(h10, h6, h7) + X[15] + 1836072691, 9) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F3(h9, h10, h6) + X[5] + 1836072691, 7) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F3(h8, h9, h10) + X[1] + 1836072691, 15) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F3(h7, h8, h9) + X[3] + 1836072691, 11) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F3(h6, h7, h8) + X[7] + 1836072691, 8) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F3(h10, h6, h7) + X[14] + 1836072691, 6) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F3(h9, h10, h6) + X[6] + 1836072691, 6) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F3(h8, h9, h10) + X[9] + 1836072691, 14) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F3(h7, h8, h9) + X[11] + 1836072691, 12) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F3(h6, h7, h8) + X[8] + 1836072691, 13) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F3(h10, h6, h7) + X[12] + 1836072691, 5) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F3(h9, h10, h6) + X[2] + 1836072691, 14) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F3(h8, h9, h10) + X[10] + 1836072691, 13) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F3(h7, h8, h9) + X[0] + 1836072691, 13) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F3(h6, h7, h8) + X[4] + 1836072691, 7) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F3(h10, h6, h7) + X[13] + 1836072691, 5) + h8;
		h6 = RL(h6, 10);
		num = h3;
		h3 = h8;
		h8 = num;
		h3 = RL(h3 + F4(h4, h5, h) + X[1] + -1894007588, 11) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F4(h3, h4, h5) + X[9] + -1894007588, 12) + h;
		h4 = RL(h4, 10);
		h = RL(h + F4(h2, h3, h4) + X[11] + -1894007588, 14) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F4(h, h2, h3) + X[10] + -1894007588, 15) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F4(h5, h, h2) + X[0] + -1894007588, 14) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F4(h4, h5, h) + X[8] + -1894007588, 15) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F4(h3, h4, h5) + X[12] + -1894007588, 9) + h;
		h4 = RL(h4, 10);
		h = RL(h + F4(h2, h3, h4) + X[4] + -1894007588, 8) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F4(h, h2, h3) + X[13] + -1894007588, 9) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F4(h5, h, h2) + X[3] + -1894007588, 14) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F4(h4, h5, h) + X[7] + -1894007588, 5) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F4(h3, h4, h5) + X[15] + -1894007588, 6) + h;
		h4 = RL(h4, 10);
		h = RL(h + F4(h2, h3, h4) + X[14] + -1894007588, 8) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F4(h, h2, h3) + X[5] + -1894007588, 6) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F4(h5, h, h2) + X[6] + -1894007588, 5) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F4(h4, h5, h) + X[2] + -1894007588, 12) + h2;
		h5 = RL(h5, 10);
		h8 = RL(h8 + F2(h9, h10, h6) + X[8] + 2053994217, 15) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F2(h8, h9, h10) + X[6] + 2053994217, 5) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F2(h7, h8, h9) + X[4] + 2053994217, 8) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F2(h6, h7, h8) + X[1] + 2053994217, 11) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F2(h10, h6, h7) + X[3] + 2053994217, 14) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F2(h9, h10, h6) + X[11] + 2053994217, 14) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F2(h8, h9, h10) + X[15] + 2053994217, 6) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F2(h7, h8, h9) + X[0] + 2053994217, 14) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F2(h6, h7, h8) + X[5] + 2053994217, 6) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F2(h10, h6, h7) + X[12] + 2053994217, 9) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F2(h9, h10, h6) + X[2] + 2053994217, 12) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F2(h8, h9, h10) + X[13] + 2053994217, 9) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F2(h7, h8, h9) + X[9] + 2053994217, 12) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F2(h6, h7, h8) + X[7] + 2053994217, 5) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F2(h10, h6, h7) + X[10] + 2053994217, 15) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F2(h9, h10, h6) + X[14] + 2053994217, 8) + h7;
		h10 = RL(h10, 10);
		num = h4;
		h4 = h9;
		h9 = num;
		h2 = RL(h2 + F5(h3, h4, h5) + X[4] + -1454113458, 9) + h;
		h4 = RL(h4, 10);
		h = RL(h + F5(h2, h3, h4) + X[0] + -1454113458, 15) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F5(h, h2, h3) + X[5] + -1454113458, 5) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F5(h5, h, h2) + X[9] + -1454113458, 11) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F5(h4, h5, h) + X[7] + -1454113458, 6) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F5(h3, h4, h5) + X[12] + -1454113458, 8) + h;
		h4 = RL(h4, 10);
		h = RL(h + F5(h2, h3, h4) + X[2] + -1454113458, 13) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F5(h, h2, h3) + X[10] + -1454113458, 12) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F5(h5, h, h2) + X[14] + -1454113458, 5) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F5(h4, h5, h) + X[1] + -1454113458, 12) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F5(h3, h4, h5) + X[3] + -1454113458, 13) + h;
		h4 = RL(h4, 10);
		h = RL(h + F5(h2, h3, h4) + X[8] + -1454113458, 14) + h5;
		h3 = RL(h3, 10);
		h5 = RL(h5 + F5(h, h2, h3) + X[11] + -1454113458, 11) + h4;
		h2 = RL(h2, 10);
		h4 = RL(h4 + F5(h5, h, h2) + X[6] + -1454113458, 8) + h3;
		h = RL(h, 10);
		h3 = RL(h3 + F5(h4, h5, h) + X[15] + -1454113458, 5) + h2;
		h5 = RL(h5, 10);
		h2 = RL(h2 + F5(h3, h4, h5) + X[13] + -1454113458, 6) + h;
		h4 = RL(h4, 10);
		h7 = RL(h7 + F1(h8, h9, h10) + X[12], 8) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F1(h7, h8, h9) + X[15], 5) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F1(h6, h7, h8) + X[10], 12) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F1(h10, h6, h7) + X[4], 9) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F1(h9, h10, h6) + X[1], 12) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F1(h8, h9, h10) + X[5], 5) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F1(h7, h8, h9) + X[8], 14) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F1(h6, h7, h8) + X[7], 6) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F1(h10, h6, h7) + X[6], 8) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F1(h9, h10, h6) + X[2], 13) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F1(h8, h9, h10) + X[13], 6) + h6;
		h9 = RL(h9, 10);
		h6 = RL(h6 + F1(h7, h8, h9) + X[14], 5) + h10;
		h8 = RL(h8, 10);
		h10 = RL(h10 + F1(h6, h7, h8) + X[0], 15) + h9;
		h7 = RL(h7, 10);
		h9 = RL(h9 + F1(h10, h6, h7) + X[3], 13) + h8;
		h6 = RL(h6, 10);
		h8 = RL(h8 + F1(h9, h10, h6) + X[9], 11) + h7;
		h10 = RL(h10, 10);
		h7 = RL(h7 + F1(h8, h9, h10) + X[11], 11) + h6;
		h9 = RL(h9, 10);
		H0 += h;
		H1 += h2;
		H2 += h3;
		H3 += h4;
		H4 += h10;
		H5 += h6;
		H6 += h7;
		H7 += h8;
		H8 += h9;
		H9 += h5;
		xOff = 0;
		for (int i = 0; i != X.Length; i++)
		{
			X[i] = 0;
		}
	}

	public override IMemoable Copy()
	{
		return new RipeMD320Digest(this);
	}

	public override void Reset(IMemoable other)
	{
		RipeMD320Digest t = (RipeMD320Digest)other;
		CopyIn(t);
	}
}
