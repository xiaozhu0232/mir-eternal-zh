using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm;

internal abstract class GcmUtilities
{
	private const uint E1 = 3774873600u;

	private const ulong E1UL = 16212958658533785600uL;

	internal static byte[] OneAsBytes()
	{
		return new byte[16]
		{
			128, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
	}

	internal static uint[] OneAsUints()
	{
		return new uint[4] { 2147483648u, 0u, 0u, 0u };
	}

	internal static ulong[] OneAsUlongs()
	{
		return new ulong[2] { 9223372036854775808uL, 0uL };
	}

	internal static byte[] AsBytes(uint[] x)
	{
		return Pack.UInt32_To_BE(x);
	}

	internal static void AsBytes(uint[] x, byte[] z)
	{
		Pack.UInt32_To_BE(x, z, 0);
	}

	internal static byte[] AsBytes(ulong[] x)
	{
		byte[] array = new byte[16];
		Pack.UInt64_To_BE(x, array, 0);
		return array;
	}

	internal static void AsBytes(ulong[] x, byte[] z)
	{
		Pack.UInt64_To_BE(x, z, 0);
	}

	internal static uint[] AsUints(byte[] bs)
	{
		uint[] array = new uint[4];
		Pack.BE_To_UInt32(bs, 0, array);
		return array;
	}

	internal static void AsUints(byte[] bs, uint[] output)
	{
		Pack.BE_To_UInt32(bs, 0, output);
	}

	internal static ulong[] AsUlongs(byte[] x)
	{
		ulong[] array = new ulong[2];
		Pack.BE_To_UInt64(x, 0, array);
		return array;
	}

	internal static void AsUlongs(byte[] x, ulong[] z)
	{
		Pack.BE_To_UInt64(x, 0, z);
	}

	internal static void AsUlongs(byte[] x, ulong[] z, int zOff)
	{
		Pack.BE_To_UInt64(x, 0, z, zOff, 2);
	}

	internal static void Copy(uint[] x, uint[] z)
	{
		z[0] = x[0];
		z[1] = x[1];
		z[2] = x[2];
		z[3] = x[3];
	}

	internal static void Copy(ulong[] x, ulong[] z)
	{
		z[0] = x[0];
		z[1] = x[1];
	}

	internal static void Copy(ulong[] x, int xOff, ulong[] z, int zOff)
	{
		z[zOff] = x[xOff];
		z[zOff + 1] = x[xOff + 1];
	}

	internal static void DivideP(ulong[] x, ulong[] z)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = (ulong)((long)num >> 63);
		num ^= num3 & 0xE100000000000000uL;
		z[0] = (num << 1) | (num2 >> 63);
		z[1] = (num2 << 1) | (0L - num3);
	}

	internal static void DivideP(ulong[] x, int xOff, ulong[] z, int zOff)
	{
		ulong num = x[xOff];
		ulong num2 = x[xOff + 1];
		ulong num3 = (ulong)((long)num >> 63);
		num ^= num3 & 0xE100000000000000uL;
		z[zOff] = (num << 1) | (num2 >> 63);
		z[zOff + 1] = (num2 << 1) | (0L - num3);
	}

	internal static void Multiply(byte[] x, byte[] y)
	{
		ulong[] x2 = AsUlongs(x);
		ulong[] y2 = AsUlongs(y);
		Multiply(x2, y2);
		AsBytes(x2, x);
	}

	internal static void Multiply(uint[] x, uint[] y)
	{
		uint num = y[0];
		uint num2 = y[1];
		uint num3 = y[2];
		uint num4 = y[3];
		uint num5 = 0u;
		uint num6 = 0u;
		uint num7 = 0u;
		uint num8 = 0u;
		for (int i = 0; i < 4; i++)
		{
			int num9 = (int)x[i];
			for (int j = 0; j < 32; j++)
			{
				uint num10 = (uint)(num9 >> 31);
				num9 <<= 1;
				num5 ^= num & num10;
				num6 ^= num2 & num10;
				num7 ^= num3 & num10;
				num8 ^= num4 & num10;
				uint num11 = (uint)((int)(num4 << 31) >> 8);
				num4 = (num4 >> 1) | (num3 << 31);
				num3 = (num3 >> 1) | (num2 << 31);
				num2 = (num2 >> 1) | (num << 31);
				num = (num >> 1) ^ (num11 & 0xE1000000u);
			}
		}
		x[0] = num5;
		x[1] = num6;
		x[2] = num7;
		x[3] = num8;
	}

	internal static void Multiply(ulong[] x, ulong[] y)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = y[0];
		ulong num4 = y[1];
		ulong num5 = Longs.Reverse(num);
		ulong num6 = Longs.Reverse(num2);
		ulong num7 = Longs.Reverse(num3);
		ulong num8 = Longs.Reverse(num4);
		ulong num9 = Longs.Reverse(ImplMul64(num5, num7));
		ulong num10 = ImplMul64(num, num3) << 1;
		ulong num11 = Longs.Reverse(ImplMul64(num6, num8));
		ulong num12 = ImplMul64(num2, num4) << 1;
		ulong num13 = Longs.Reverse(ImplMul64(num5 ^ num6, num7 ^ num8));
		ulong num14 = ImplMul64(num ^ num2, num3 ^ num4) << 1;
		ulong num15 = num9;
		ulong num16 = num10 ^ num9 ^ num11 ^ num13;
		ulong num17 = num11 ^ num10 ^ num12 ^ num14;
		ulong num18 = num12;
		num16 ^= num18 ^ (num18 >> 1) ^ (num18 >> 2) ^ (num18 >> 7);
		num17 ^= (num18 << 62) ^ (num18 << 57);
		num15 ^= num17 ^ (num17 >> 1) ^ (num17 >> 2) ^ (num17 >> 7);
		num16 ^= (num17 << 63) ^ (num17 << 62) ^ (num17 << 57);
		x[0] = num15;
		x[1] = num16;
	}

	internal static void MultiplyP(uint[] x)
	{
		uint num = x[0];
		uint num2 = x[1];
		uint num3 = x[2];
		uint num4 = x[3];
		uint num5 = (uint)((int)(num4 << 31) >> 31);
		x[0] = (num >> 1) ^ (num5 & 0xE1000000u);
		x[1] = (num2 >> 1) | (num << 31);
		x[2] = (num3 >> 1) | (num2 << 31);
		x[3] = (num4 >> 1) | (num3 << 31);
	}

	internal static void MultiplyP(uint[] x, uint[] z)
	{
		uint num = x[0];
		uint num2 = x[1];
		uint num3 = x[2];
		uint num4 = x[3];
		uint num5 = (uint)((int)(num4 << 31) >> 31);
		z[0] = (num >> 1) ^ (num5 & 0xE1000000u);
		z[1] = (num2 >> 1) | (num << 31);
		z[2] = (num3 >> 1) | (num2 << 31);
		z[3] = (num4 >> 1) | (num3 << 31);
	}

	internal static void MultiplyP(ulong[] x)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = (ulong)((long)(num2 << 63) >> 63);
		x[0] = (num >> 1) ^ (num3 & 0xE100000000000000uL);
		x[1] = (num2 >> 1) | (num << 63);
	}

	internal static void MultiplyP(ulong[] x, ulong[] z)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = (ulong)((long)(num2 << 63) >> 63);
		z[0] = (num >> 1) ^ (num3 & 0xE100000000000000uL);
		z[1] = (num2 >> 1) | (num << 63);
	}

	internal static void MultiplyP3(ulong[] x, ulong[] z)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = num2 << 61;
		z[0] = (num >> 3) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		z[1] = (num2 >> 3) | (num << 61);
	}

	internal static void MultiplyP3(ulong[] x, int xOff, ulong[] z, int zOff)
	{
		ulong num = x[xOff];
		ulong num2 = x[xOff + 1];
		ulong num3 = num2 << 61;
		z[zOff] = (num >> 3) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		z[zOff + 1] = (num2 >> 3) | (num << 61);
	}

	internal static void MultiplyP4(ulong[] x, ulong[] z)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = num2 << 60;
		z[0] = (num >> 4) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		z[1] = (num2 >> 4) | (num << 60);
	}

	internal static void MultiplyP4(ulong[] x, int xOff, ulong[] z, int zOff)
	{
		ulong num = x[xOff];
		ulong num2 = x[xOff + 1];
		ulong num3 = num2 << 60;
		z[zOff] = (num >> 4) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		z[zOff + 1] = (num2 >> 4) | (num << 60);
	}

	internal static void MultiplyP7(ulong[] x, ulong[] z)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = num2 << 57;
		z[0] = (num >> 7) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		z[1] = (num2 >> 7) | (num << 57);
	}

	internal static void MultiplyP7(ulong[] x, int xOff, ulong[] z, int zOff)
	{
		ulong num = x[xOff];
		ulong num2 = x[xOff + 1];
		ulong num3 = num2 << 57;
		z[zOff] = (num >> 7) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		z[zOff + 1] = (num2 >> 7) | (num << 57);
	}

	internal static void MultiplyP8(uint[] x)
	{
		uint num = x[0];
		uint num2 = x[1];
		uint num3 = x[2];
		uint num4 = x[3];
		uint num5 = num4 << 24;
		x[0] = (num >> 8) ^ num5 ^ (num5 >> 1) ^ (num5 >> 2) ^ (num5 >> 7);
		x[1] = (num2 >> 8) | (num << 24);
		x[2] = (num3 >> 8) | (num2 << 24);
		x[3] = (num4 >> 8) | (num3 << 24);
	}

	internal static void MultiplyP8(uint[] x, uint[] y)
	{
		uint num = x[0];
		uint num2 = x[1];
		uint num3 = x[2];
		uint num4 = x[3];
		uint num5 = num4 << 24;
		y[0] = (num >> 8) ^ num5 ^ (num5 >> 1) ^ (num5 >> 2) ^ (num5 >> 7);
		y[1] = (num2 >> 8) | (num << 24);
		y[2] = (num3 >> 8) | (num2 << 24);
		y[3] = (num4 >> 8) | (num3 << 24);
	}

	internal static void MultiplyP8(ulong[] x)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = num2 << 56;
		x[0] = (num >> 8) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		x[1] = (num2 >> 8) | (num << 56);
	}

	internal static void MultiplyP8(ulong[] x, ulong[] y)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = num2 << 56;
		y[0] = (num >> 8) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		y[1] = (num2 >> 8) | (num << 56);
	}

	internal static void MultiplyP8(ulong[] x, int xOff, ulong[] y, int yOff)
	{
		ulong num = x[xOff];
		ulong num2 = x[xOff + 1];
		ulong num3 = num2 << 56;
		y[yOff] = (num >> 8) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		y[yOff + 1] = (num2 >> 8) | (num << 56);
	}

	internal static void Square(ulong[] x, ulong[] z)
	{
		ulong[] array = new ulong[4];
		Interleave.Expand64To128Rev(x[0], array, 0);
		Interleave.Expand64To128Rev(x[1], array, 2);
		ulong num = array[0];
		ulong num2 = array[1];
		ulong num3 = array[2];
		ulong num4 = array[3];
		num2 ^= num4 ^ (num4 >> 1) ^ (num4 >> 2) ^ (num4 >> 7);
		num3 ^= (num4 << 62) ^ (num4 << 57);
		num ^= num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		num2 ^= (num3 << 63) ^ (num3 << 62) ^ (num3 << 57);
		z[0] = num;
		z[1] = num2;
	}

	internal static void Xor(byte[] x, byte[] y)
	{
		int num = 0;
		do
		{
			byte[] array;
			byte[] array2 = (array = x);
			int num2 = num;
			nint num3 = num2;
			array2[num2] = (byte)(array[num3] ^ y[num]);
			num++;
			byte[] array3 = (array = x);
			int num4 = num;
			num3 = num4;
			array3[num4] = (byte)(array[num3] ^ y[num]);
			num++;
			byte[] array4 = (array = x);
			int num5 = num;
			num3 = num5;
			array4[num5] = (byte)(array[num3] ^ y[num]);
			num++;
			byte[] array5 = (array = x);
			int num6 = num;
			num3 = num6;
			array5[num6] = (byte)(array[num3] ^ y[num]);
			num++;
		}
		while (num < 16);
	}

	internal static void Xor(byte[] x, byte[] y, int yOff)
	{
		int num = 0;
		do
		{
			byte[] array;
			byte[] array2 = (array = x);
			int num2 = num;
			nint num3 = num2;
			array2[num2] = (byte)(array[num3] ^ y[yOff + num]);
			num++;
			byte[] array3 = (array = x);
			int num4 = num;
			num3 = num4;
			array3[num4] = (byte)(array[num3] ^ y[yOff + num]);
			num++;
			byte[] array4 = (array = x);
			int num5 = num;
			num3 = num5;
			array4[num5] = (byte)(array[num3] ^ y[yOff + num]);
			num++;
			byte[] array5 = (array = x);
			int num6 = num;
			num3 = num6;
			array5[num6] = (byte)(array[num3] ^ y[yOff + num]);
			num++;
		}
		while (num < 16);
	}

	internal static void Xor(byte[] x, int xOff, byte[] y, int yOff, byte[] z, int zOff)
	{
		int num = 0;
		do
		{
			z[zOff + num] = (byte)(x[xOff + num] ^ y[yOff + num]);
			num++;
			z[zOff + num] = (byte)(x[xOff + num] ^ y[yOff + num]);
			num++;
			z[zOff + num] = (byte)(x[xOff + num] ^ y[yOff + num]);
			num++;
			z[zOff + num] = (byte)(x[xOff + num] ^ y[yOff + num]);
			num++;
		}
		while (num < 16);
	}

	internal static void Xor(byte[] x, byte[] y, int yOff, int yLen)
	{
		while (--yLen >= 0)
		{
			byte[] array;
			byte[] array2 = (array = x);
			int num = yLen;
			nint num2 = num;
			array2[num] = (byte)(array[num2] ^ y[yOff + yLen]);
		}
	}

	internal static void Xor(byte[] x, int xOff, byte[] y, int yOff, int len)
	{
		while (--len >= 0)
		{
			byte[] array;
			byte[] array2 = (array = x);
			int num = xOff + len;
			nint num2 = num;
			array2[num] = (byte)(array[num2] ^ y[yOff + len]);
		}
	}

	internal static void Xor(byte[] x, byte[] y, byte[] z)
	{
		int num = 0;
		do
		{
			z[num] = (byte)(x[num] ^ y[num]);
			num++;
			z[num] = (byte)(x[num] ^ y[num]);
			num++;
			z[num] = (byte)(x[num] ^ y[num]);
			num++;
			z[num] = (byte)(x[num] ^ y[num]);
			num++;
		}
		while (num < 16);
	}

	internal static void Xor(uint[] x, uint[] y)
	{
		uint[] array;
		(array = x)[0] = array[0] ^ y[0];
		(array = x)[1] = array[1] ^ y[1];
		(array = x)[2] = array[2] ^ y[2];
		(array = x)[3] = array[3] ^ y[3];
	}

	internal static void Xor(uint[] x, uint[] y, uint[] z)
	{
		z[0] = x[0] ^ y[0];
		z[1] = x[1] ^ y[1];
		z[2] = x[2] ^ y[2];
		z[3] = x[3] ^ y[3];
	}

	internal static void Xor(ulong[] x, ulong[] y)
	{
		ulong[] array;
		(array = x)[0] = array[0] ^ y[0];
		(array = x)[1] = array[1] ^ y[1];
	}

	internal static void Xor(ulong[] x, int xOff, ulong[] y, int yOff)
	{
		ulong[] array;
		ulong[] array2 = (array = x);
		nint num = xOff;
		array2[xOff] = array[num] ^ y[yOff];
		ulong[] array3 = (array = x);
		int num2 = xOff + 1;
		num = num2;
		array3[num2] = array[num] ^ y[yOff + 1];
	}

	internal static void Xor(ulong[] x, ulong[] y, ulong[] z)
	{
		z[0] = x[0] ^ y[0];
		z[1] = x[1] ^ y[1];
	}

	internal static void Xor(ulong[] x, int xOff, ulong[] y, int yOff, ulong[] z, int zOff)
	{
		z[zOff] = x[xOff] ^ y[yOff];
		z[zOff + 1] = x[xOff + 1] ^ y[yOff + 1];
	}

	private static ulong ImplMul64(ulong x, ulong y)
	{
		ulong num = x & 0x1111111111111111uL;
		ulong num2 = x & 0x2222222222222222uL;
		ulong num3 = x & 0x4444444444444444uL;
		ulong num4 = x & 0x8888888888888888uL;
		ulong num5 = y & 0x1111111111111111uL;
		ulong num6 = y & 0x2222222222222222uL;
		ulong num7 = y & 0x4444444444444444uL;
		ulong num8 = y & 0x8888888888888888uL;
		ulong num9 = (num * num5) ^ (num2 * num8) ^ (num3 * num7) ^ (num4 * num6);
		ulong num10 = (num * num6) ^ (num2 * num5) ^ (num3 * num8) ^ (num4 * num7);
		ulong num11 = (num * num7) ^ (num2 * num6) ^ (num3 * num5) ^ (num4 * num8);
		ulong num12 = (num * num8) ^ (num2 * num7) ^ (num3 * num6) ^ (num4 * num5);
		num9 &= 0x1111111111111111uL;
		num10 &= 0x2222222222222222uL;
		num11 &= 0x4444444444444444uL;
		num12 &= 0x8888888888888888uL;
		return num9 | num10 | num11 | num12;
	}
}
