using System;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class Sha1Digest : GeneralDigest
{
	private const int DigestLength = 20;

	private const uint Y1 = 1518500249u;

	private const uint Y2 = 1859775393u;

	private const uint Y3 = 2400959708u;

	private const uint Y4 = 3395469782u;

	private uint H1;

	private uint H2;

	private uint H3;

	private uint H4;

	private uint H5;

	private uint[] X = new uint[80];

	private int xOff;

	public override string AlgorithmName => "SHA-1";

	public Sha1Digest()
	{
		Reset();
	}

	public Sha1Digest(Sha1Digest t)
		: base(t)
	{
		CopyIn(t);
	}

	private void CopyIn(Sha1Digest t)
	{
		CopyIn((GeneralDigest)t);
		H1 = t.H1;
		H2 = t.H2;
		H3 = t.H3;
		H4 = t.H4;
		H5 = t.H5;
		Array.Copy(t.X, 0, X, 0, t.X.Length);
		xOff = t.xOff;
	}

	public override int GetDigestSize()
	{
		return 20;
	}

	internal override void ProcessWord(byte[] input, int inOff)
	{
		X[xOff] = Pack.BE_To_UInt32(input, inOff);
		if (++xOff == 16)
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
		X[14] = (uint)((ulong)bitLength >> 32);
		X[15] = (uint)bitLength;
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		Finish();
		Pack.UInt32_To_BE(H1, output, outOff);
		Pack.UInt32_To_BE(H2, output, outOff + 4);
		Pack.UInt32_To_BE(H3, output, outOff + 8);
		Pack.UInt32_To_BE(H4, output, outOff + 12);
		Pack.UInt32_To_BE(H5, output, outOff + 16);
		Reset();
		return 20;
	}

	public override void Reset()
	{
		base.Reset();
		H1 = 1732584193u;
		H2 = 4023233417u;
		H3 = 2562383102u;
		H4 = 271733878u;
		H5 = 3285377520u;
		xOff = 0;
		Array.Clear(X, 0, X.Length);
	}

	private static uint F(uint u, uint v, uint w)
	{
		return (u & v) | (~u & w);
	}

	private static uint H(uint u, uint v, uint w)
	{
		return u ^ v ^ w;
	}

	private static uint G(uint u, uint v, uint w)
	{
		return (u & v) | (u & w) | (v & w);
	}

	internal override void ProcessBlock()
	{
		for (int i = 16; i < 80; i++)
		{
			uint num = X[i - 3] ^ X[i - 8] ^ X[i - 14] ^ X[i - 16];
			X[i] = (num << 1) | (num >> 31);
		}
		uint num2 = H1;
		uint num3 = H2;
		uint num4 = H3;
		uint num5 = H4;
		uint num6 = H5;
		int num7 = 0;
		for (int j = 0; j < 4; j++)
		{
			num6 += ((num2 << 5) | (num2 >> 27)) + F(num3, num4, num5) + X[num7++] + 1518500249;
			num3 = (num3 << 30) | (num3 >> 2);
			num5 += ((num6 << 5) | (num6 >> 27)) + F(num2, num3, num4) + X[num7++] + 1518500249;
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += ((num5 << 5) | (num5 >> 27)) + F(num6, num2, num3) + X[num7++] + 1518500249;
			num6 = (num6 << 30) | (num6 >> 2);
			num3 += ((num4 << 5) | (num4 >> 27)) + F(num5, num6, num2) + X[num7++] + 1518500249;
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += ((num3 << 5) | (num3 >> 27)) + F(num4, num5, num6) + X[num7++] + 1518500249;
			num4 = (num4 << 30) | (num4 >> 2);
		}
		for (int k = 0; k < 4; k++)
		{
			num6 += ((num2 << 5) | (num2 >> 27)) + H(num3, num4, num5) + X[num7++] + 1859775393;
			num3 = (num3 << 30) | (num3 >> 2);
			num5 += ((num6 << 5) | (num6 >> 27)) + H(num2, num3, num4) + X[num7++] + 1859775393;
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += ((num5 << 5) | (num5 >> 27)) + H(num6, num2, num3) + X[num7++] + 1859775393;
			num6 = (num6 << 30) | (num6 >> 2);
			num3 += ((num4 << 5) | (num4 >> 27)) + H(num5, num6, num2) + X[num7++] + 1859775393;
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += ((num3 << 5) | (num3 >> 27)) + H(num4, num5, num6) + X[num7++] + 1859775393;
			num4 = (num4 << 30) | (num4 >> 2);
		}
		for (int l = 0; l < 4; l++)
		{
			num6 += (uint)((int)(((num2 << 5) | (num2 >> 27)) + G(num3, num4, num5) + X[num7++]) + -1894007588);
			num3 = (num3 << 30) | (num3 >> 2);
			num5 += (uint)((int)(((num6 << 5) | (num6 >> 27)) + G(num2, num3, num4) + X[num7++]) + -1894007588);
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += (uint)((int)(((num5 << 5) | (num5 >> 27)) + G(num6, num2, num3) + X[num7++]) + -1894007588);
			num6 = (num6 << 30) | (num6 >> 2);
			num3 += (uint)((int)(((num4 << 5) | (num4 >> 27)) + G(num5, num6, num2) + X[num7++]) + -1894007588);
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += (uint)((int)(((num3 << 5) | (num3 >> 27)) + G(num4, num5, num6) + X[num7++]) + -1894007588);
			num4 = (num4 << 30) | (num4 >> 2);
		}
		for (int m = 0; m < 4; m++)
		{
			num6 += (uint)((int)(((num2 << 5) | (num2 >> 27)) + H(num3, num4, num5) + X[num7++]) + -899497514);
			num3 = (num3 << 30) | (num3 >> 2);
			num5 += (uint)((int)(((num6 << 5) | (num6 >> 27)) + H(num2, num3, num4) + X[num7++]) + -899497514);
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += (uint)((int)(((num5 << 5) | (num5 >> 27)) + H(num6, num2, num3) + X[num7++]) + -899497514);
			num6 = (num6 << 30) | (num6 >> 2);
			num3 += (uint)((int)(((num4 << 5) | (num4 >> 27)) + H(num5, num6, num2) + X[num7++]) + -899497514);
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += (uint)((int)(((num3 << 5) | (num3 >> 27)) + H(num4, num5, num6) + X[num7++]) + -899497514);
			num4 = (num4 << 30) | (num4 >> 2);
		}
		H1 += num2;
		H2 += num3;
		H3 += num4;
		H4 += num5;
		H5 += num6;
		xOff = 0;
		Array.Clear(X, 0, 16);
	}

	public override IMemoable Copy()
	{
		return new Sha1Digest(this);
	}

	public override void Reset(IMemoable other)
	{
		Sha1Digest t = (Sha1Digest)other;
		CopyIn(t);
	}
}
