using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public sealed class WhirlpoolDigest : IDigest, IMemoable
{
	private const int BYTE_LENGTH = 64;

	private const int DIGEST_LENGTH_BYTES = 64;

	private const int ROUNDS = 10;

	private const int REDUCTION_POLYNOMIAL = 285;

	private const int BITCOUNT_ARRAY_SIZE = 32;

	private static readonly int[] SBOX;

	private static readonly long[] C0;

	private static readonly long[] C1;

	private static readonly long[] C2;

	private static readonly long[] C3;

	private static readonly long[] C4;

	private static readonly long[] C5;

	private static readonly long[] C6;

	private static readonly long[] C7;

	private readonly long[] _rc = new long[11];

	private static readonly short[] EIGHT;

	private byte[] _buffer = new byte[64];

	private int _bufferPos;

	private short[] _bitCount = new short[32];

	private long[] _hash = new long[8];

	private long[] _K = new long[8];

	private long[] _L = new long[8];

	private long[] _block = new long[8];

	private long[] _state = new long[8];

	public string AlgorithmName => "Whirlpool";

	static WhirlpoolDigest()
	{
		SBOX = new int[256]
		{
			24, 35, 198, 232, 135, 184, 1, 79, 54, 166,
			210, 245, 121, 111, 145, 82, 96, 188, 155, 142,
			163, 12, 123, 53, 29, 224, 215, 194, 46, 75,
			254, 87, 21, 119, 55, 229, 159, 240, 74, 218,
			88, 201, 41, 10, 177, 160, 107, 133, 189, 93,
			16, 244, 203, 62, 5, 103, 228, 39, 65, 139,
			167, 125, 149, 216, 251, 238, 124, 102, 221, 23,
			71, 158, 202, 45, 191, 7, 173, 90, 131, 51,
			99, 2, 170, 113, 200, 25, 73, 217, 242, 227,
			91, 136, 154, 38, 50, 176, 233, 15, 213, 128,
			190, 205, 52, 72, 255, 122, 144, 95, 32, 104,
			26, 174, 180, 84, 147, 34, 100, 241, 115, 18,
			64, 8, 195, 236, 219, 161, 141, 61, 151, 0,
			207, 43, 118, 130, 214, 27, 181, 175, 106, 80,
			69, 243, 48, 239, 63, 85, 162, 234, 101, 186,
			47, 192, 222, 28, 253, 77, 146, 117, 6, 138,
			178, 230, 14, 31, 98, 212, 168, 150, 249, 197,
			37, 89, 132, 114, 57, 76, 94, 120, 56, 140,
			209, 165, 226, 97, 179, 33, 156, 30, 67, 199,
			252, 4, 81, 153, 109, 13, 250, 223, 126, 36,
			59, 171, 206, 17, 143, 78, 183, 235, 60, 129,
			148, 247, 185, 19, 44, 211, 231, 110, 196, 3,
			86, 68, 127, 169, 42, 187, 193, 83, 220, 11,
			157, 108, 49, 116, 246, 70, 172, 137, 20, 225,
			22, 58, 105, 9, 112, 182, 208, 237, 204, 66,
			152, 164, 40, 92, 248, 134
		};
		C0 = new long[256];
		C1 = new long[256];
		C2 = new long[256];
		C3 = new long[256];
		C4 = new long[256];
		C5 = new long[256];
		C6 = new long[256];
		C7 = new long[256];
		EIGHT = new short[32];
		EIGHT[31] = 8;
		for (int i = 0; i < 256; i++)
		{
			int num = SBOX[i];
			int num2 = maskWithReductionPolynomial(num << 1);
			int num3 = maskWithReductionPolynomial(num2 << 1);
			int num4 = num3 ^ num;
			int num5 = maskWithReductionPolynomial(num3 << 1);
			int num6 = num5 ^ num;
			C0[i] = packIntoLong(num, num, num3, num, num5, num4, num2, num6);
			C1[i] = packIntoLong(num6, num, num, num3, num, num5, num4, num2);
			C2[i] = packIntoLong(num2, num6, num, num, num3, num, num5, num4);
			C3[i] = packIntoLong(num4, num2, num6, num, num, num3, num, num5);
			C4[i] = packIntoLong(num5, num4, num2, num6, num, num, num3, num);
			C5[i] = packIntoLong(num, num5, num4, num2, num6, num, num, num3);
			C6[i] = packIntoLong(num3, num, num5, num4, num2, num6, num, num);
			C7[i] = packIntoLong(num, num3, num, num5, num4, num2, num6, num);
		}
	}

	public WhirlpoolDigest()
	{
		_rc[0] = 0L;
		for (int i = 1; i <= 10; i++)
		{
			int num = 8 * (i - 1);
			_rc[i] = (C0[num] & -72057594037927936L) ^ (C1[num + 1] & 0xFF000000000000L) ^ (C2[num + 2] & 0xFF0000000000L) ^ (C3[num + 3] & 0xFF00000000L) ^ (C4[num + 4] & 0xFF000000u) ^ (C5[num + 5] & 0xFF0000) ^ (C6[num + 6] & 0xFF00) ^ (C7[num + 7] & 0xFF);
		}
	}

	private static long packIntoLong(int b7, int b6, int b5, int b4, int b3, int b2, int b1, int b0)
	{
		return ((long)b7 << 56) ^ ((long)b6 << 48) ^ ((long)b5 << 40) ^ ((long)b4 << 32) ^ ((long)b3 << 24) ^ ((long)b2 << 16) ^ ((long)b1 << 8) ^ b0;
	}

	private static int maskWithReductionPolynomial(int input)
	{
		int num = input;
		if ((long)num >= 256L)
		{
			num ^= 0x11D;
		}
		return num;
	}

	public WhirlpoolDigest(WhirlpoolDigest originalDigest)
	{
		Reset(originalDigest);
	}

	public int GetDigestSize()
	{
		return 64;
	}

	public int DoFinal(byte[] output, int outOff)
	{
		finish();
		for (int i = 0; i < 8; i++)
		{
			convertLongToByteArray(_hash[i], output, outOff + i * 8);
		}
		Reset();
		return GetDigestSize();
	}

	public void Reset()
	{
		_bufferPos = 0;
		Array.Clear(_bitCount, 0, _bitCount.Length);
		Array.Clear(_buffer, 0, _buffer.Length);
		Array.Clear(_hash, 0, _hash.Length);
		Array.Clear(_K, 0, _K.Length);
		Array.Clear(_L, 0, _L.Length);
		Array.Clear(_block, 0, _block.Length);
		Array.Clear(_state, 0, _state.Length);
	}

	private void processFilledBuffer()
	{
		for (int i = 0; i < _state.Length; i++)
		{
			_block[i] = bytesToLongFromBuffer(_buffer, i * 8);
		}
		processBlock();
		_bufferPos = 0;
		Array.Clear(_buffer, 0, _buffer.Length);
	}

	private static long bytesToLongFromBuffer(byte[] buffer, int startPos)
	{
		return (long)((((ulong)buffer[startPos] & 0xFFuL) << 56) | (((ulong)buffer[startPos + 1] & 0xFFuL) << 48) | (((ulong)buffer[startPos + 2] & 0xFFuL) << 40) | (((ulong)buffer[startPos + 3] & 0xFFuL) << 32) | (((ulong)buffer[startPos + 4] & 0xFFuL) << 24) | (((ulong)buffer[startPos + 5] & 0xFFuL) << 16) | (((ulong)buffer[startPos + 6] & 0xFFuL) << 8)) | ((long)buffer[startPos + 7] & 0xFFL);
	}

	private static void convertLongToByteArray(long inputLong, byte[] outputArray, int offSet)
	{
		for (int i = 0; i < 8; i++)
		{
			outputArray[offSet + i] = (byte)((inputLong >> 56 - i * 8) & 0xFF);
		}
	}

	private void processBlock()
	{
		for (int i = 0; i < 8; i++)
		{
			_state[i] = _block[i] ^ (_K[i] = _hash[i]);
		}
		for (int j = 1; j <= 10; j++)
		{
			long[] l;
			for (int k = 0; k < 8; k++)
			{
				_L[k] = 0L;
				long[] array = (l = _L);
				int num = k;
				nint num2 = num;
				array[num] = l[num2] ^ C0[(int)(_K[k & 7] >> 56) & 0xFF];
				long[] array2 = (l = _L);
				int num3 = k;
				num2 = num3;
				array2[num3] = l[num2] ^ C1[(int)(_K[(k - 1) & 7] >> 48) & 0xFF];
				long[] array3 = (l = _L);
				int num4 = k;
				num2 = num4;
				array3[num4] = l[num2] ^ C2[(int)(_K[(k - 2) & 7] >> 40) & 0xFF];
				long[] array4 = (l = _L);
				int num5 = k;
				num2 = num5;
				array4[num5] = l[num2] ^ C3[(int)(_K[(k - 3) & 7] >> 32) & 0xFF];
				long[] array5 = (l = _L);
				int num6 = k;
				num2 = num6;
				array5[num6] = l[num2] ^ C4[(int)(_K[(k - 4) & 7] >> 24) & 0xFF];
				long[] array6 = (l = _L);
				int num7 = k;
				num2 = num7;
				array6[num7] = l[num2] ^ C5[(int)(_K[(k - 5) & 7] >> 16) & 0xFF];
				long[] array7 = (l = _L);
				int num8 = k;
				num2 = num8;
				array7[num8] = l[num2] ^ C6[(int)(_K[(k - 6) & 7] >> 8) & 0xFF];
				long[] array8 = (l = _L);
				int num9 = k;
				num2 = num9;
				array8[num9] = l[num2] ^ C7[(int)_K[(k - 7) & 7] & 0xFF];
			}
			Array.Copy(_L, 0, _K, 0, _K.Length);
			(l = _K)[0] = l[0] ^ _rc[j];
			for (int m = 0; m < 8; m++)
			{
				_L[m] = _K[m];
				long[] array9 = (l = _L);
				int num10 = m;
				nint num2 = num10;
				array9[num10] = l[num2] ^ C0[(int)(_state[m & 7] >> 56) & 0xFF];
				long[] array10 = (l = _L);
				int num11 = m;
				num2 = num11;
				array10[num11] = l[num2] ^ C1[(int)(_state[(m - 1) & 7] >> 48) & 0xFF];
				long[] array11 = (l = _L);
				int num12 = m;
				num2 = num12;
				array11[num12] = l[num2] ^ C2[(int)(_state[(m - 2) & 7] >> 40) & 0xFF];
				long[] array12 = (l = _L);
				int num13 = m;
				num2 = num13;
				array12[num13] = l[num2] ^ C3[(int)(_state[(m - 3) & 7] >> 32) & 0xFF];
				long[] array13 = (l = _L);
				int num14 = m;
				num2 = num14;
				array13[num14] = l[num2] ^ C4[(int)(_state[(m - 4) & 7] >> 24) & 0xFF];
				long[] array14 = (l = _L);
				int num15 = m;
				num2 = num15;
				array14[num15] = l[num2] ^ C5[(int)(_state[(m - 5) & 7] >> 16) & 0xFF];
				long[] array15 = (l = _L);
				int num16 = m;
				num2 = num16;
				array15[num16] = l[num2] ^ C6[(int)(_state[(m - 6) & 7] >> 8) & 0xFF];
				long[] array16 = (l = _L);
				int num17 = m;
				num2 = num17;
				array16[num17] = l[num2] ^ C7[(int)_state[(m - 7) & 7] & 0xFF];
			}
			Array.Copy(_L, 0, _state, 0, _state.Length);
		}
		for (int n = 0; n < 8; n++)
		{
			long[] l;
			long[] array17 = (l = _hash);
			int num18 = n;
			nint num2 = num18;
			array17[num18] = l[num2] ^ (_state[n] ^ _block[n]);
		}
	}

	public void Update(byte input)
	{
		_buffer[_bufferPos] = input;
		_bufferPos++;
		if (_bufferPos == _buffer.Length)
		{
			processFilledBuffer();
		}
		increment();
	}

	private void increment()
	{
		int num = 0;
		for (int num2 = _bitCount.Length - 1; num2 >= 0; num2--)
		{
			int num3 = (_bitCount[num2] & 0xFF) + EIGHT[num2] + num;
			num = num3 >> 8;
			_bitCount[num2] = (short)(num3 & 0xFF);
		}
	}

	public void BlockUpdate(byte[] input, int inOff, int length)
	{
		while (length > 0)
		{
			Update(input[inOff]);
			inOff++;
			length--;
		}
	}

	private void finish()
	{
		byte[] array = copyBitLength();
		byte[] buffer;
		byte[] array2 = (buffer = _buffer);
		int num = _bufferPos++;
		nint num2 = num;
		array2[num] = (byte)(buffer[num2] | 0x80u);
		if (_bufferPos == _buffer.Length)
		{
			processFilledBuffer();
		}
		if (_bufferPos > 32)
		{
			while (_bufferPos != 0)
			{
				Update(0);
			}
		}
		while (_bufferPos <= 32)
		{
			Update(0);
		}
		Array.Copy(array, 0, _buffer, 32, array.Length);
		processFilledBuffer();
	}

	private byte[] copyBitLength()
	{
		byte[] array = new byte[32];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (byte)((uint)_bitCount[i] & 0xFFu);
		}
		return array;
	}

	public int GetByteLength()
	{
		return 64;
	}

	public IMemoable Copy()
	{
		return new WhirlpoolDigest(this);
	}

	public void Reset(IMemoable other)
	{
		WhirlpoolDigest whirlpoolDigest = (WhirlpoolDigest)other;
		Array.Copy(whirlpoolDigest._rc, 0, _rc, 0, _rc.Length);
		Array.Copy(whirlpoolDigest._buffer, 0, _buffer, 0, _buffer.Length);
		_bufferPos = whirlpoolDigest._bufferPos;
		Array.Copy(whirlpoolDigest._bitCount, 0, _bitCount, 0, _bitCount.Length);
		Array.Copy(whirlpoolDigest._hash, 0, _hash, 0, _hash.Length);
		Array.Copy(whirlpoolDigest._K, 0, _K, 0, _K.Length);
		Array.Copy(whirlpoolDigest._L, 0, _L, 0, _L.Length);
		Array.Copy(whirlpoolDigest._block, 0, _block, 0, _block.Length);
		Array.Copy(whirlpoolDigest._state, 0, _state, 0, _state.Length);
	}
}
