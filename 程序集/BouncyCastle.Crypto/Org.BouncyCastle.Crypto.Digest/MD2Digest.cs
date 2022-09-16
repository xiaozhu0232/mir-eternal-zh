using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class MD2Digest : IDigest, IMemoable
{
	private const int DigestLength = 16;

	private const int BYTE_LENGTH = 16;

	private byte[] X = new byte[48];

	private int xOff;

	private byte[] M = new byte[16];

	private int mOff;

	private byte[] C = new byte[16];

	private int COff;

	private static readonly byte[] S = new byte[256]
	{
		41, 46, 67, 201, 162, 216, 124, 1, 61, 54,
		84, 161, 236, 240, 6, 19, 98, 167, 5, 243,
		192, 199, 115, 140, 152, 147, 43, 217, 188, 76,
		130, 202, 30, 155, 87, 60, 253, 212, 224, 22,
		103, 66, 111, 24, 138, 23, 229, 18, 190, 78,
		196, 214, 218, 158, 222, 73, 160, 251, 245, 142,
		187, 47, 238, 122, 169, 104, 121, 145, 21, 178,
		7, 63, 148, 194, 16, 137, 11, 34, 95, 33,
		128, 127, 93, 154, 90, 144, 50, 39, 53, 62,
		204, 231, 191, 247, 151, 3, 255, 25, 48, 179,
		72, 165, 181, 209, 215, 94, 146, 42, 172, 86,
		170, 198, 79, 184, 56, 210, 150, 164, 125, 182,
		118, 252, 107, 226, 156, 116, 4, 241, 69, 157,
		112, 89, 100, 113, 135, 32, 134, 91, 207, 101,
		230, 45, 168, 2, 27, 96, 37, 173, 174, 176,
		185, 246, 28, 70, 97, 105, 52, 64, 126, 15,
		85, 71, 163, 35, 221, 81, 175, 58, 195, 92,
		249, 206, 186, 197, 234, 38, 44, 83, 13, 110,
		133, 40, 132, 9, 211, 223, 205, 244, 65, 129,
		77, 82, 106, 220, 55, 200, 108, 193, 171, 250,
		36, 225, 123, 8, 12, 189, 177, 74, 120, 136,
		149, 139, 227, 99, 232, 109, 233, 203, 213, 254,
		59, 0, 29, 57, 242, 239, 183, 14, 102, 88,
		208, 228, 166, 119, 114, 248, 235, 117, 75, 10,
		49, 68, 80, 180, 143, 237, 31, 26, 219, 153,
		141, 51, 159, 17, 131, 20
	};

	public string AlgorithmName => "MD2";

	public MD2Digest()
	{
		Reset();
	}

	public MD2Digest(MD2Digest t)
	{
		CopyIn(t);
	}

	private void CopyIn(MD2Digest t)
	{
		Array.Copy(t.X, 0, X, 0, t.X.Length);
		xOff = t.xOff;
		Array.Copy(t.M, 0, M, 0, t.M.Length);
		mOff = t.mOff;
		Array.Copy(t.C, 0, C, 0, t.C.Length);
		COff = t.COff;
	}

	public int GetDigestSize()
	{
		return 16;
	}

	public int GetByteLength()
	{
		return 16;
	}

	public int DoFinal(byte[] output, int outOff)
	{
		byte b = (byte)(M.Length - mOff);
		for (int i = mOff; i < M.Length; i++)
		{
			M[i] = b;
		}
		ProcessChecksum(M);
		ProcessBlock(M);
		ProcessBlock(C);
		Array.Copy(X, xOff, output, outOff, 16);
		Reset();
		return 16;
	}

	public void Reset()
	{
		xOff = 0;
		for (int i = 0; i != X.Length; i++)
		{
			X[i] = 0;
		}
		mOff = 0;
		for (int j = 0; j != M.Length; j++)
		{
			M[j] = 0;
		}
		COff = 0;
		for (int k = 0; k != C.Length; k++)
		{
			C[k] = 0;
		}
	}

	public void Update(byte input)
	{
		M[mOff++] = input;
		if (mOff == 16)
		{
			ProcessChecksum(M);
			ProcessBlock(M);
			mOff = 0;
		}
	}

	public void BlockUpdate(byte[] input, int inOff, int length)
	{
		while (mOff != 0 && length > 0)
		{
			Update(input[inOff]);
			inOff++;
			length--;
		}
		while (length > 16)
		{
			Array.Copy(input, inOff, M, 0, 16);
			ProcessChecksum(M);
			ProcessBlock(M);
			length -= 16;
			inOff += 16;
		}
		while (length > 0)
		{
			Update(input[inOff]);
			inOff++;
			length--;
		}
	}

	internal void ProcessChecksum(byte[] m)
	{
		int num = C[15];
		for (int i = 0; i < 16; i++)
		{
			byte[] c;
			byte[] array = (c = C);
			int num2 = i;
			nint num3 = num2;
			array[num2] = (byte)(c[num3] ^ S[(m[i] ^ num) & 0xFF]);
			num = C[i];
		}
	}

	internal void ProcessBlock(byte[] m)
	{
		for (int i = 0; i < 16; i++)
		{
			X[i + 16] = m[i];
			X[i + 32] = (byte)(m[i] ^ X[i]);
		}
		int num = 0;
		for (int j = 0; j < 18; j++)
		{
			for (int k = 0; k < 48; k++)
			{
				byte[] x;
				byte[] array = (x = X);
				int num2 = k;
				nint num3 = num2;
				byte b;
				array[num2] = (b = (byte)(x[num3] ^ S[num]));
				num = b;
				num &= 0xFF;
			}
			num = (num + j) % 256;
		}
	}

	public IMemoable Copy()
	{
		return new MD2Digest(this);
	}

	public void Reset(IMemoable other)
	{
		MD2Digest t = (MD2Digest)other;
		CopyIn(t);
	}
}
