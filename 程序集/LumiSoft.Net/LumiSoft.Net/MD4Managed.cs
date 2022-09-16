using System;

namespace LumiSoft.Net;

internal class MD4Managed : _MD4
{
	private uint[] state;

	private byte[] buffer;

	private uint[] count;

	private uint[] x;

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

	private byte[] digest;

	public MD4Managed()
	{
		state = new uint[4];
		count = new uint[2];
		buffer = new byte[64];
		digest = new byte[16];
		x = new uint[16];
		Initialize();
	}

	public override void Initialize()
	{
		count[0] = 0u;
		count[1] = 0u;
		state[0] = 1732584193u;
		state[1] = 4023233417u;
		state[2] = 2562383102u;
		state[3] = 271733878u;
		Array.Clear(buffer, 0, 64);
		Array.Clear(x, 0, 16);
	}

	protected override void HashCore(byte[] array, int ibStart, int cbSize)
	{
		int num = (int)((count[0] >> 3) & 0x3F);
		count[0] += (uint)(cbSize << 3);
		if (count[0] < cbSize << 3)
		{
			count[1]++;
		}
		count[1] += (uint)(cbSize >> 29);
		int num2 = 64 - num;
		int i = 0;
		if (cbSize >= num2)
		{
			Buffer.BlockCopy(array, ibStart, buffer, num, num2);
			MD4Transform(state, buffer, 0);
			for (i = num2; i + 63 < cbSize; i += 64)
			{
				MD4Transform(state, array, i);
			}
			num = 0;
		}
		Buffer.BlockCopy(array, ibStart + i, buffer, num, cbSize - i);
	}

	protected override byte[] HashFinal()
	{
		byte[] array = new byte[8];
		Encode(array, count);
		uint num = (count[0] >> 3) & 0x3Fu;
		int num2 = (int)((num < 56) ? (56 - num) : (120 - num));
		HashCore(Padding(num2), 0, num2);
		HashCore(array, 0, 8);
		Encode(digest, state);
		Initialize();
		return digest;
	}

	private byte[] Padding(int nLength)
	{
		if (nLength > 0)
		{
			byte[] array = new byte[nLength];
			array[0] = 128;
			return array;
		}
		return null;
	}

	private uint F(uint x, uint y, uint z)
	{
		return (x & y) | (~x & z);
	}

	private uint G(uint x, uint y, uint z)
	{
		return (x & y) | (x & z) | (y & z);
	}

	private uint H(uint x, uint y, uint z)
	{
		return x ^ y ^ z;
	}

	private uint ROL(uint x, byte n)
	{
		return (x << (int)n) | (x >> 32 - n);
	}

	private void FF(ref uint a, uint b, uint c, uint d, uint x, byte s)
	{
		a += F(b, c, d) + x;
		a = ROL(a, s);
	}

	private void GG(ref uint a, uint b, uint c, uint d, uint x, byte s)
	{
		a += G(b, c, d) + x + 1518500249;
		a = ROL(a, s);
	}

	private void HH(ref uint a, uint b, uint c, uint d, uint x, byte s)
	{
		a += H(b, c, d) + x + 1859775393;
		a = ROL(a, s);
	}

	private void Encode(byte[] output, uint[] input)
	{
		int num = 0;
		for (int i = 0; i < output.Length; i += 4)
		{
			output[i] = (byte)input[num];
			output[i + 1] = (byte)(input[num] >> 8);
			output[i + 2] = (byte)(input[num] >> 16);
			output[i + 3] = (byte)(input[num] >> 24);
			num++;
		}
	}

	private void Decode(uint[] output, byte[] input, int index)
	{
		int num = 0;
		int num2 = index;
		while (num < output.Length)
		{
			output[num] = (uint)(input[num2] | (input[num2 + 1] << 8) | (input[num2 + 2] << 16) | (input[num2 + 3] << 24));
			num++;
			num2 += 4;
		}
	}

	private void MD4Transform(uint[] state, byte[] block, int index)
	{
		uint a = state[0];
		uint a2 = state[1];
		uint a3 = state[2];
		uint a4 = state[3];
		Decode(x, block, index);
		FF(ref a, a2, a3, a4, x[0], 3);
		FF(ref a4, a, a2, a3, x[1], 7);
		FF(ref a3, a4, a, a2, x[2], 11);
		FF(ref a2, a3, a4, a, x[3], 19);
		FF(ref a, a2, a3, a4, x[4], 3);
		FF(ref a4, a, a2, a3, x[5], 7);
		FF(ref a3, a4, a, a2, x[6], 11);
		FF(ref a2, a3, a4, a, x[7], 19);
		FF(ref a, a2, a3, a4, x[8], 3);
		FF(ref a4, a, a2, a3, x[9], 7);
		FF(ref a3, a4, a, a2, x[10], 11);
		FF(ref a2, a3, a4, a, x[11], 19);
		FF(ref a, a2, a3, a4, x[12], 3);
		FF(ref a4, a, a2, a3, x[13], 7);
		FF(ref a3, a4, a, a2, x[14], 11);
		FF(ref a2, a3, a4, a, x[15], 19);
		GG(ref a, a2, a3, a4, x[0], 3);
		GG(ref a4, a, a2, a3, x[4], 5);
		GG(ref a3, a4, a, a2, x[8], 9);
		GG(ref a2, a3, a4, a, x[12], 13);
		GG(ref a, a2, a3, a4, x[1], 3);
		GG(ref a4, a, a2, a3, x[5], 5);
		GG(ref a3, a4, a, a2, x[9], 9);
		GG(ref a2, a3, a4, a, x[13], 13);
		GG(ref a, a2, a3, a4, x[2], 3);
		GG(ref a4, a, a2, a3, x[6], 5);
		GG(ref a3, a4, a, a2, x[10], 9);
		GG(ref a2, a3, a4, a, x[14], 13);
		GG(ref a, a2, a3, a4, x[3], 3);
		GG(ref a4, a, a2, a3, x[7], 5);
		GG(ref a3, a4, a, a2, x[11], 9);
		GG(ref a2, a3, a4, a, x[15], 13);
		HH(ref a, a2, a3, a4, x[0], 3);
		HH(ref a4, a, a2, a3, x[8], 9);
		HH(ref a3, a4, a, a2, x[4], 11);
		HH(ref a2, a3, a4, a, x[12], 15);
		HH(ref a, a2, a3, a4, x[2], 3);
		HH(ref a4, a, a2, a3, x[10], 9);
		HH(ref a3, a4, a, a2, x[6], 11);
		HH(ref a2, a3, a4, a, x[14], 15);
		HH(ref a, a2, a3, a4, x[1], 3);
		HH(ref a4, a, a2, a3, x[9], 9);
		HH(ref a3, a4, a, a2, x[5], 11);
		HH(ref a2, a3, a4, a, x[13], 15);
		HH(ref a, a2, a3, a4, x[3], 3);
		HH(ref a4, a, a2, a3, x[11], 9);
		HH(ref a3, a4, a, a2, x[7], 11);
		HH(ref a2, a3, a4, a, x[15], 15);
		state[0] += a;
		state[1] += a2;
		state[2] += a3;
		state[3] += a4;
	}
}
