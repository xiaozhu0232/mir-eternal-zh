using System;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class Gost3411Digest : IDigest, IMemoable
{
	private const int DIGEST_LENGTH = 32;

	private byte[] H = new byte[32];

	private byte[] L = new byte[32];

	private byte[] M = new byte[32];

	private byte[] Sum = new byte[32];

	private byte[][] C = MakeC();

	private byte[] xBuf = new byte[32];

	private int xBufOff;

	private ulong byteCount;

	private readonly IBlockCipher cipher = new Gost28147Engine();

	private byte[] sBox;

	private byte[] K = new byte[32];

	private byte[] a = new byte[8];

	internal short[] wS = new short[16];

	internal short[] w_S = new short[16];

	internal byte[] S = new byte[32];

	internal byte[] U = new byte[32];

	internal byte[] V = new byte[32];

	internal byte[] W = new byte[32];

	private static readonly byte[] C2 = new byte[32]
	{
		0, 255, 0, 255, 0, 255, 0, 255, 255, 0,
		255, 0, 255, 0, 255, 0, 0, 255, 255, 0,
		255, 0, 0, 255, 255, 0, 0, 0, 255, 255,
		0, 255
	};

	public string AlgorithmName => "Gost3411";

	private static byte[][] MakeC()
	{
		byte[][] array = new byte[4][];
		for (int i = 0; i < 4; i++)
		{
			array[i] = new byte[32];
		}
		return array;
	}

	public Gost3411Digest()
	{
		sBox = Gost28147Engine.GetSBox("D-A");
		cipher.Init(forEncryption: true, new ParametersWithSBox(null, sBox));
		Reset();
	}

	public Gost3411Digest(byte[] sBoxParam)
	{
		sBox = Arrays.Clone(sBoxParam);
		cipher.Init(forEncryption: true, new ParametersWithSBox(null, sBox));
		Reset();
	}

	public Gost3411Digest(Gost3411Digest t)
	{
		Reset(t);
	}

	public int GetDigestSize()
	{
		return 32;
	}

	public void Update(byte input)
	{
		xBuf[xBufOff++] = input;
		if (xBufOff == xBuf.Length)
		{
			sumByteArray(xBuf);
			processBlock(xBuf, 0);
			xBufOff = 0;
		}
		byteCount++;
	}

	public void BlockUpdate(byte[] input, int inOff, int length)
	{
		while (xBufOff != 0 && length > 0)
		{
			Update(input[inOff]);
			inOff++;
			length--;
		}
		while (length > xBuf.Length)
		{
			Array.Copy(input, inOff, xBuf, 0, xBuf.Length);
			sumByteArray(xBuf);
			processBlock(xBuf, 0);
			inOff += xBuf.Length;
			length -= xBuf.Length;
			byteCount += (uint)xBuf.Length;
		}
		while (length > 0)
		{
			Update(input[inOff]);
			inOff++;
			length--;
		}
	}

	private byte[] P(byte[] input)
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			K[num++] = input[i];
			K[num++] = input[8 + i];
			K[num++] = input[16 + i];
			K[num++] = input[24 + i];
		}
		return K;
	}

	private byte[] A(byte[] input)
	{
		for (int i = 0; i < 8; i++)
		{
			a[i] = (byte)(input[i] ^ input[i + 8]);
		}
		Array.Copy(input, 8, input, 0, 24);
		Array.Copy(a, 0, input, 24, 8);
		return input;
	}

	private void E(byte[] key, byte[] s, int sOff, byte[] input, int inOff)
	{
		cipher.Init(forEncryption: true, new KeyParameter(key));
		cipher.ProcessBlock(input, inOff, s, sOff);
	}

	private void fw(byte[] input)
	{
		cpyBytesToShort(input, wS);
		w_S[15] = (short)(wS[0] ^ wS[1] ^ wS[2] ^ wS[3] ^ wS[12] ^ wS[15]);
		Array.Copy(wS, 1, w_S, 0, 15);
		cpyShortToBytes(w_S, input);
	}

	private void processBlock(byte[] input, int inOff)
	{
		Array.Copy(input, inOff, M, 0, 32);
		H.CopyTo(U, 0);
		M.CopyTo(V, 0);
		for (int i = 0; i < 32; i++)
		{
			W[i] = (byte)(U[i] ^ V[i]);
		}
		E(P(W), S, 0, H, 0);
		for (int j = 1; j < 4; j++)
		{
			byte[] array = A(U);
			for (int k = 0; k < 32; k++)
			{
				U[k] = (byte)(array[k] ^ C[j][k]);
			}
			V = A(A(V));
			for (int l = 0; l < 32; l++)
			{
				W[l] = (byte)(U[l] ^ V[l]);
			}
			E(P(W), S, j * 8, H, j * 8);
		}
		for (int m = 0; m < 12; m++)
		{
			fw(S);
		}
		for (int n = 0; n < 32; n++)
		{
			S[n] = (byte)(S[n] ^ M[n]);
		}
		fw(S);
		for (int num = 0; num < 32; num++)
		{
			S[num] = (byte)(H[num] ^ S[num]);
		}
		for (int num2 = 0; num2 < 61; num2++)
		{
			fw(S);
		}
		Array.Copy(S, 0, H, 0, H.Length);
	}

	private void finish()
	{
		ulong n = byteCount * 8;
		Pack.UInt64_To_LE(n, L);
		while (xBufOff != 0)
		{
			Update(0);
		}
		processBlock(L, 0);
		processBlock(Sum, 0);
	}

	public int DoFinal(byte[] output, int outOff)
	{
		finish();
		H.CopyTo(output, outOff);
		Reset();
		return 32;
	}

	public void Reset()
	{
		byteCount = 0uL;
		xBufOff = 0;
		Array.Clear(H, 0, H.Length);
		Array.Clear(L, 0, L.Length);
		Array.Clear(M, 0, M.Length);
		Array.Clear(C[1], 0, C[1].Length);
		Array.Clear(C[3], 0, C[3].Length);
		Array.Clear(Sum, 0, Sum.Length);
		Array.Clear(xBuf, 0, xBuf.Length);
		C2.CopyTo(C[2], 0);
	}

	private void sumByteArray(byte[] input)
	{
		int num = 0;
		for (int i = 0; i != Sum.Length; i++)
		{
			int num2 = (Sum[i] & 0xFF) + (input[i] & 0xFF) + num;
			Sum[i] = (byte)num2;
			num = num2 >> 8;
		}
	}

	private static void cpyBytesToShort(byte[] S, short[] wS)
	{
		for (int i = 0; i < S.Length / 2; i++)
		{
			wS[i] = (short)(((S[i * 2 + 1] << 8) & 0xFF00) | (S[i * 2] & 0xFF));
		}
	}

	private static void cpyShortToBytes(short[] wS, byte[] S)
	{
		for (int i = 0; i < S.Length / 2; i++)
		{
			S[i * 2 + 1] = (byte)(wS[i] >> 8);
			S[i * 2] = (byte)wS[i];
		}
	}

	public int GetByteLength()
	{
		return 32;
	}

	public IMemoable Copy()
	{
		return new Gost3411Digest(this);
	}

	public void Reset(IMemoable other)
	{
		Gost3411Digest gost3411Digest = (Gost3411Digest)other;
		sBox = gost3411Digest.sBox;
		cipher.Init(forEncryption: true, new ParametersWithSBox(null, sBox));
		Reset();
		Array.Copy(gost3411Digest.H, 0, H, 0, gost3411Digest.H.Length);
		Array.Copy(gost3411Digest.L, 0, L, 0, gost3411Digest.L.Length);
		Array.Copy(gost3411Digest.M, 0, M, 0, gost3411Digest.M.Length);
		Array.Copy(gost3411Digest.Sum, 0, Sum, 0, gost3411Digest.Sum.Length);
		Array.Copy(gost3411Digest.C[1], 0, C[1], 0, gost3411Digest.C[1].Length);
		Array.Copy(gost3411Digest.C[2], 0, C[2], 0, gost3411Digest.C[2].Length);
		Array.Copy(gost3411Digest.C[3], 0, C[3], 0, gost3411Digest.C[3].Length);
		Array.Copy(gost3411Digest.xBuf, 0, xBuf, 0, gost3411Digest.xBuf.Length);
		xBufOff = gost3411Digest.xBufOff;
		byteCount = gost3411Digest.byteCount;
	}
}
