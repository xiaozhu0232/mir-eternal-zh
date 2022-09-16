using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Macs;

public class Gost28147Mac : IMac
{
	private const int blockSize = 8;

	private const int macSize = 4;

	private int bufOff;

	private byte[] buf;

	private byte[] mac;

	private bool firstStep = true;

	private int[] workingKey;

	private byte[] macIV = null;

	private byte[] S = new byte[128]
	{
		9, 6, 3, 2, 8, 11, 1, 7, 10, 4,
		14, 15, 12, 0, 13, 5, 3, 7, 14, 9,
		8, 10, 15, 0, 5, 2, 6, 12, 11, 4,
		13, 1, 14, 4, 6, 2, 11, 3, 13, 8,
		12, 15, 5, 10, 0, 7, 1, 9, 14, 7,
		10, 12, 13, 1, 3, 9, 0, 2, 11, 4,
		15, 8, 5, 6, 11, 5, 1, 9, 8, 13,
		15, 0, 14, 4, 2, 3, 12, 7, 10, 6,
		3, 10, 13, 12, 1, 2, 0, 11, 7, 5,
		9, 4, 8, 15, 14, 6, 1, 13, 2, 9,
		7, 10, 6, 0, 8, 12, 4, 5, 15, 3,
		11, 14, 11, 10, 15, 5, 0, 12, 14, 8,
		6, 2, 3, 9, 1, 7, 13, 4
	};

	public string AlgorithmName => "Gost28147Mac";

	public Gost28147Mac()
	{
		mac = new byte[8];
		buf = new byte[8];
		bufOff = 0;
	}

	private static int[] GenerateWorkingKey(byte[] userKey)
	{
		if (userKey.Length != 32)
		{
			throw new ArgumentException("Key length invalid. Key needs to be 32 byte - 256 bit!!!");
		}
		int[] array = new int[8];
		for (int i = 0; i != 8; i++)
		{
			array[i] = bytesToint(userKey, i * 4);
		}
		return array;
	}

	public void Init(ICipherParameters parameters)
	{
		Reset();
		buf = new byte[8];
		macIV = null;
		if (parameters is ParametersWithSBox)
		{
			ParametersWithSBox parametersWithSBox = (ParametersWithSBox)parameters;
			parametersWithSBox.GetSBox().CopyTo(S, 0);
			if (parametersWithSBox.Parameters != null)
			{
				workingKey = GenerateWorkingKey(((KeyParameter)parametersWithSBox.Parameters).GetKey());
			}
			return;
		}
		if (parameters is KeyParameter)
		{
			workingKey = GenerateWorkingKey(((KeyParameter)parameters).GetKey());
			return;
		}
		if (parameters is ParametersWithIV)
		{
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			workingKey = GenerateWorkingKey(((KeyParameter)parametersWithIV.Parameters).GetKey());
			Array.Copy(parametersWithIV.GetIV(), 0, mac, 0, mac.Length);
			macIV = parametersWithIV.GetIV();
			return;
		}
		throw new ArgumentException("invalid parameter passed to Gost28147 init - " + Platform.GetTypeName(parameters));
	}

	public int GetMacSize()
	{
		return 4;
	}

	private int gost28147_mainStep(int n1, int key)
	{
		int num = key + n1;
		int num2 = S[num & 0xF];
		num2 += S[16 + ((num >> 4) & 0xF)] << 4;
		num2 += S[32 + ((num >> 8) & 0xF)] << 8;
		num2 += S[48 + ((num >> 12) & 0xF)] << 12;
		num2 += S[64 + ((num >> 16) & 0xF)] << 16;
		num2 += S[80 + ((num >> 20) & 0xF)] << 20;
		num2 += S[96 + ((num >> 24) & 0xF)] << 24;
		num2 += S[112 + ((num >> 28) & 0xF)] << 28;
		int num3 = num2 << 11;
		int num4 = (int)((uint)num2 >> 21);
		return num3 | num4;
	}

	private void gost28147MacFunc(int[] workingKey, byte[] input, int inOff, byte[] output, int outOff)
	{
		int num = bytesToint(input, inOff);
		int num2 = bytesToint(input, inOff + 4);
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int num3 = num;
				num = num2 ^ gost28147_mainStep(num, workingKey[j]);
				num2 = num3;
			}
		}
		intTobytes(num, output, outOff);
		intTobytes(num2, output, outOff + 4);
	}

	private static int bytesToint(byte[] input, int inOff)
	{
		return (int)((input[inOff + 3] << 24) & 0xFF000000u) + ((input[inOff + 2] << 16) & 0xFF0000) + ((input[inOff + 1] << 8) & 0xFF00) + (input[inOff] & 0xFF);
	}

	private static void intTobytes(int num, byte[] output, int outOff)
	{
		output[outOff + 3] = (byte)(num >> 24);
		output[outOff + 2] = (byte)(num >> 16);
		output[outOff + 1] = (byte)(num >> 8);
		output[outOff] = (byte)num;
	}

	private static byte[] CM5func(byte[] buf, int bufOff, byte[] mac)
	{
		byte[] array = new byte[buf.Length - bufOff];
		Array.Copy(buf, bufOff, array, 0, mac.Length);
		for (int i = 0; i != mac.Length; i++)
		{
			array[i] = (byte)(array[i] ^ mac[i]);
		}
		return array;
	}

	public void Update(byte input)
	{
		if (bufOff == buf.Length)
		{
			byte[] array = new byte[buf.Length];
			Array.Copy(buf, 0, array, 0, mac.Length);
			if (firstStep)
			{
				firstStep = false;
				if (macIV != null)
				{
					array = CM5func(buf, 0, macIV);
				}
			}
			else
			{
				array = CM5func(buf, 0, mac);
			}
			gost28147MacFunc(workingKey, array, 0, mac, 0);
			bufOff = 0;
		}
		buf[bufOff++] = input;
	}

	public void BlockUpdate(byte[] input, int inOff, int len)
	{
		if (len < 0)
		{
			throw new ArgumentException("Can't have a negative input length!");
		}
		int num = 8 - bufOff;
		if (len > num)
		{
			Array.Copy(input, inOff, buf, bufOff, num);
			byte[] array = new byte[buf.Length];
			Array.Copy(buf, 0, array, 0, mac.Length);
			if (firstStep)
			{
				firstStep = false;
				if (macIV != null)
				{
					array = CM5func(buf, 0, macIV);
				}
			}
			else
			{
				array = CM5func(buf, 0, mac);
			}
			gost28147MacFunc(workingKey, array, 0, mac, 0);
			bufOff = 0;
			len -= num;
			inOff += num;
			while (len > 8)
			{
				array = CM5func(input, inOff, mac);
				gost28147MacFunc(workingKey, array, 0, mac, 0);
				len -= 8;
				inOff += 8;
			}
		}
		Array.Copy(input, inOff, buf, bufOff, len);
		bufOff += len;
	}

	public int DoFinal(byte[] output, int outOff)
	{
		while (bufOff < 8)
		{
			buf[bufOff++] = 0;
		}
		byte[] array = new byte[buf.Length];
		Array.Copy(buf, 0, array, 0, mac.Length);
		if (firstStep)
		{
			firstStep = false;
		}
		else
		{
			array = CM5func(buf, 0, mac);
		}
		gost28147MacFunc(workingKey, array, 0, mac, 0);
		Array.Copy(mac, mac.Length / 2 - 4, output, outOff, 4);
		Reset();
		return 4;
	}

	public void Reset()
	{
		Array.Clear(buf, 0, buf.Length);
		bufOff = 0;
		firstStep = true;
	}
}
