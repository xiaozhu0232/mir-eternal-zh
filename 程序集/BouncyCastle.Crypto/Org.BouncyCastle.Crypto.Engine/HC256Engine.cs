using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class HC256Engine : IStreamCipher
{
	private uint[] p = new uint[1024];

	private uint[] q = new uint[1024];

	private uint cnt = 0u;

	private byte[] key;

	private byte[] iv;

	private bool initialised;

	private byte[] buf = new byte[4];

	private int idx = 0;

	public virtual string AlgorithmName => "HC-256";

	private uint Step()
	{
		uint num = cnt & 0x3FFu;
		uint result;
		if (cnt < 1024)
		{
			uint num2 = p[(num - 3) & 0x3FF];
			uint num3 = p[(num - 1023) & 0x3FF];
			uint[] array;
			nint num4;
			(array = p)[num4 = (nint)num] = array[num4] + (p[(num - 10) & 0x3FF] + (RotateRight(num2, 10) ^ RotateRight(num3, 23)) + q[(num2 ^ num3) & 0x3FF]);
			num2 = p[(num - 12) & 0x3FF];
			result = (q[num2 & 0xFF] + q[((num2 >> 8) & 0xFF) + 256] + q[((num2 >> 16) & 0xFF) + 512] + q[((num2 >> 24) & 0xFF) + 768]) ^ p[num];
		}
		else
		{
			uint num5 = q[(num - 3) & 0x3FF];
			uint num6 = q[(num - 1023) & 0x3FF];
			uint[] array;
			nint num4;
			(array = q)[num4 = (nint)num] = array[num4] + (q[(num - 10) & 0x3FF] + (RotateRight(num5, 10) ^ RotateRight(num6, 23)) + p[(num5 ^ num6) & 0x3FF]);
			num5 = q[(num - 12) & 0x3FF];
			result = (p[num5 & 0xFF] + p[((num5 >> 8) & 0xFF) + 256] + p[((num5 >> 16) & 0xFF) + 512] + p[((num5 >> 24) & 0xFF) + 768]) ^ q[num];
		}
		cnt = (cnt + 1) & 0x7FFu;
		return result;
	}

	private void Init()
	{
		if (key.Length != 32 && key.Length != 16)
		{
			throw new ArgumentException("The key must be 128/256 bits long");
		}
		if (iv.Length < 16)
		{
			throw new ArgumentException("The IV must be at least 128 bits long");
		}
		if (key.Length != 32)
		{
			byte[] destinationArray = new byte[32];
			Array.Copy(key, 0, destinationArray, 0, key.Length);
			Array.Copy(key, 0, destinationArray, 16, key.Length);
			key = destinationArray;
		}
		if (iv.Length < 32)
		{
			byte[] array = new byte[32];
			Array.Copy(iv, 0, array, 0, iv.Length);
			Array.Copy(iv, 0, array, iv.Length, array.Length - iv.Length);
			iv = array;
		}
		idx = 0;
		cnt = 0u;
		uint[] array2 = new uint[2560];
		for (int i = 0; i < 32; i++)
		{
			uint[] array3;
			uint[] array4 = (array3 = array2);
			int num = i >> 2;
			nint num2 = num;
			array4[num] = array3[num2] | (uint)(key[i] << 8 * (i & 3));
		}
		for (int j = 0; j < 32; j++)
		{
			uint[] array3;
			uint[] array5 = (array3 = array2);
			int num3 = (j >> 2) + 8;
			nint num2 = num3;
			array5[num3] = array3[num2] | (uint)(iv[j] << 8 * (j & 3));
		}
		for (uint num4 = 16u; num4 < 2560; num4++)
		{
			uint num5 = array2[num4 - 2];
			uint num6 = array2[num4 - 15];
			array2[num4] = (RotateRight(num5, 17) ^ RotateRight(num5, 19) ^ (num5 >> 10)) + array2[num4 - 7] + (RotateRight(num6, 7) ^ RotateRight(num6, 18) ^ (num6 >> 3)) + array2[num4 - 16] + num4;
		}
		Array.Copy(array2, 512, p, 0, 1024);
		Array.Copy(array2, 1536, q, 0, 1024);
		for (int k = 0; k < 4096; k++)
		{
			Step();
		}
		cnt = 0u;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		ICipherParameters cipherParameters = parameters;
		if (parameters is ParametersWithIV)
		{
			iv = ((ParametersWithIV)parameters).GetIV();
			cipherParameters = ((ParametersWithIV)parameters).Parameters;
		}
		else
		{
			iv = new byte[0];
		}
		if (cipherParameters is KeyParameter)
		{
			key = ((KeyParameter)cipherParameters).GetKey();
			Init();
			initialised = true;
			return;
		}
		throw new ArgumentException("Invalid parameter passed to HC256 init - " + Platform.GetTypeName(parameters), "parameters");
	}

	private byte GetByte()
	{
		if (idx == 0)
		{
			Pack.UInt32_To_LE(Step(), buf);
		}
		byte result = buf[idx];
		idx = (idx + 1) & 3;
		return result;
	}

	public virtual void ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
	{
		if (!initialised)
		{
			throw new InvalidOperationException(AlgorithmName + " not initialised");
		}
		Check.DataLength(input, inOff, len, "input buffer too short");
		Check.OutputLength(output, outOff, len, "output buffer too short");
		for (int i = 0; i < len; i++)
		{
			output[outOff + i] = (byte)(input[inOff + i] ^ GetByte());
		}
	}

	public virtual void Reset()
	{
		Init();
	}

	public virtual byte ReturnByte(byte input)
	{
		return (byte)(input ^ GetByte());
	}

	private static uint RotateRight(uint x, int bits)
	{
		return (x >> bits) | (x << -bits);
	}
}
