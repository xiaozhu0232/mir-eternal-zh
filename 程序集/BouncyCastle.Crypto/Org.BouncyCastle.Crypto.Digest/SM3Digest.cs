using System;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class SM3Digest : GeneralDigest
{
	private const int DIGEST_LENGTH = 32;

	private const int BLOCK_SIZE = 16;

	private uint[] V = new uint[8];

	private uint[] inwords = new uint[16];

	private int xOff;

	private uint[] W = new uint[68];

	private static readonly uint[] T;

	public override string AlgorithmName => "SM3";

	static SM3Digest()
	{
		T = new uint[64];
		for (int i = 0; i < 16; i++)
		{
			uint num = 2043430169u;
			T[i] = (num << i) | (num >> 32 - i);
		}
		for (int j = 16; j < 64; j++)
		{
			int num2 = j % 32;
			uint num3 = 2055708042u;
			T[j] = (num3 << num2) | (num3 >> 32 - num2);
		}
	}

	public SM3Digest()
	{
		Reset();
	}

	public SM3Digest(SM3Digest t)
		: base(t)
	{
		CopyIn(t);
	}

	private void CopyIn(SM3Digest t)
	{
		Array.Copy(t.V, 0, V, 0, V.Length);
		Array.Copy(t.inwords, 0, inwords, 0, inwords.Length);
		xOff = t.xOff;
	}

	public override int GetDigestSize()
	{
		return 32;
	}

	public override IMemoable Copy()
	{
		return new SM3Digest(this);
	}

	public override void Reset(IMemoable other)
	{
		SM3Digest t = (SM3Digest)other;
		CopyIn((GeneralDigest)t);
		CopyIn(t);
	}

	public override void Reset()
	{
		base.Reset();
		V[0] = 1937774191u;
		V[1] = 1226093241u;
		V[2] = 388252375u;
		V[3] = 3666478592u;
		V[4] = 2842636476u;
		V[5] = 372324522u;
		V[6] = 3817729613u;
		V[7] = 2969243214u;
		xOff = 0;
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		Finish();
		Pack.UInt32_To_BE(V, output, outOff);
		Reset();
		return 32;
	}

	internal override void ProcessWord(byte[] input, int inOff)
	{
		uint num = Pack.BE_To_UInt32(input, inOff);
		inwords[xOff] = num;
		xOff++;
		if (xOff >= 16)
		{
			ProcessBlock();
		}
	}

	internal override void ProcessLength(long bitLength)
	{
		if (xOff > 14)
		{
			inwords[xOff] = 0u;
			xOff++;
			ProcessBlock();
		}
		while (xOff < 14)
		{
			inwords[xOff] = 0u;
			xOff++;
		}
		inwords[xOff++] = (uint)(bitLength >> 32);
		inwords[xOff++] = (uint)bitLength;
	}

	private uint P0(uint x)
	{
		uint num = (x << 9) | (x >> 23);
		uint num2 = (x << 17) | (x >> 15);
		return x ^ num ^ num2;
	}

	private uint P1(uint x)
	{
		uint num = (x << 15) | (x >> 17);
		uint num2 = (x << 23) | (x >> 9);
		return x ^ num ^ num2;
	}

	private uint FF0(uint x, uint y, uint z)
	{
		return x ^ y ^ z;
	}

	private uint FF1(uint x, uint y, uint z)
	{
		return (x & y) | (x & z) | (y & z);
	}

	private uint GG0(uint x, uint y, uint z)
	{
		return x ^ y ^ z;
	}

	private uint GG1(uint x, uint y, uint z)
	{
		return (x & y) | (~x & z);
	}

	internal override void ProcessBlock()
	{
		for (int i = 0; i < 16; i++)
		{
			W[i] = inwords[i];
		}
		for (int j = 16; j < 68; j++)
		{
			uint num = W[j - 3];
			uint num2 = (num << 15) | (num >> 17);
			uint num3 = W[j - 13];
			uint num4 = (num3 << 7) | (num3 >> 25);
			W[j] = P1(W[j - 16] ^ W[j - 9] ^ num2) ^ num4 ^ W[j - 6];
		}
		uint num5 = V[0];
		uint num6 = V[1];
		uint num7 = V[2];
		uint num8 = V[3];
		uint num9 = V[4];
		uint num10 = V[5];
		uint num11 = V[6];
		uint num12 = V[7];
		for (int k = 0; k < 16; k++)
		{
			uint num13 = (num5 << 12) | (num5 >> 20);
			uint num14 = num13 + num9 + T[k];
			uint num15 = (num14 << 7) | (num14 >> 25);
			uint num16 = num15 ^ num13;
			uint num17 = W[k];
			uint num18 = num17 ^ W[k + 4];
			uint num19 = FF0(num5, num6, num7) + num8 + num16 + num18;
			uint x = GG0(num9, num10, num11) + num12 + num15 + num17;
			num8 = num7;
			num7 = (num6 << 9) | (num6 >> 23);
			num6 = num5;
			num5 = num19;
			num12 = num11;
			num11 = (num10 << 19) | (num10 >> 13);
			num10 = num9;
			num9 = P0(x);
		}
		for (int l = 16; l < 64; l++)
		{
			uint num20 = (num5 << 12) | (num5 >> 20);
			uint num21 = num20 + num9 + T[l];
			uint num22 = (num21 << 7) | (num21 >> 25);
			uint num23 = num22 ^ num20;
			uint num24 = W[l];
			uint num25 = num24 ^ W[l + 4];
			uint num26 = FF1(num5, num6, num7) + num8 + num23 + num25;
			uint x2 = GG1(num9, num10, num11) + num12 + num22 + num24;
			num8 = num7;
			num7 = (num6 << 9) | (num6 >> 23);
			num6 = num5;
			num5 = num26;
			num12 = num11;
			num11 = (num10 << 19) | (num10 >> 13);
			num10 = num9;
			num9 = P0(x2);
		}
		uint[] v;
		(v = V)[0] = v[0] ^ num5;
		(v = V)[1] = v[1] ^ num6;
		(v = V)[2] = v[2] ^ num7;
		(v = V)[3] = v[3] ^ num8;
		(v = V)[4] = v[4] ^ num9;
		(v = V)[5] = v[5] ^ num10;
		(v = V)[6] = v[6] ^ num11;
		(v = V)[7] = v[7] ^ num12;
		xOff = 0;
	}
}
