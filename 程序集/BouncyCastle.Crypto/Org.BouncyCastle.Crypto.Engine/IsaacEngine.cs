using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class IsaacEngine : IStreamCipher
{
	private static readonly int sizeL = 8;

	private static readonly int stateArraySize = sizeL << 5;

	private uint[] engineState = null;

	private uint[] results = null;

	private uint a = 0u;

	private uint b = 0u;

	private uint c = 0u;

	private int index = 0;

	private byte[] keyStream = new byte[stateArraySize << 2];

	private byte[] workingKey = null;

	private bool initialised = false;

	public virtual string AlgorithmName => "ISAAC";

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("invalid parameter passed to ISAAC Init - " + Platform.GetTypeName(parameters), "parameters");
		}
		KeyParameter keyParameter = (KeyParameter)parameters;
		setKey(keyParameter.GetKey());
	}

	public virtual byte ReturnByte(byte input)
	{
		if (index == 0)
		{
			isaac();
			keyStream = Pack.UInt32_To_BE(results);
		}
		byte result = (byte)(keyStream[index] ^ input);
		index = (index + 1) & 0x3FF;
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
			if (index == 0)
			{
				isaac();
				keyStream = Pack.UInt32_To_BE(results);
			}
			output[i + outOff] = (byte)(keyStream[index] ^ input[i + inOff]);
			index = (index + 1) & 0x3FF;
		}
	}

	public virtual void Reset()
	{
		setKey(workingKey);
	}

	private void setKey(byte[] keyBytes)
	{
		workingKey = keyBytes;
		if (engineState == null)
		{
			engineState = new uint[stateArraySize];
		}
		if (results == null)
		{
			results = new uint[stateArraySize];
		}
		for (int i = 0; i < stateArraySize; i++)
		{
			engineState[i] = (results[i] = 0u);
		}
		a = (b = (c = 0u));
		index = 0;
		byte[] array = new byte[keyBytes.Length + (keyBytes.Length & 3)];
		Array.Copy(keyBytes, 0, array, 0, keyBytes.Length);
		for (int i = 0; i < array.Length; i += 4)
		{
			results[i >> 2] = Pack.LE_To_UInt32(array, i);
		}
		uint[] array2 = new uint[sizeL];
		for (int i = 0; i < sizeL; i++)
		{
			array2[i] = 2654435769u;
		}
		for (int i = 0; i < 4; i++)
		{
			mix(array2);
		}
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < stateArraySize; j += sizeL)
			{
				for (int k = 0; k < sizeL; k++)
				{
					uint[] array3;
					uint[] array4 = (array3 = array2);
					int num = k;
					nint num2 = num;
					array4[num] = array3[num2] + ((i < 1) ? results[j + k] : engineState[j + k]);
				}
				mix(array2);
				for (int k = 0; k < sizeL; k++)
				{
					engineState[j + k] = array2[k];
				}
			}
		}
		isaac();
		initialised = true;
	}

	private void isaac()
	{
		b += ++c;
		for (int i = 0; i < stateArraySize; i++)
		{
			uint num = engineState[i];
			switch (i & 3)
			{
			case 0:
				a ^= a << 13;
				break;
			case 1:
				a ^= a >> 6;
				break;
			case 2:
				a ^= a << 2;
				break;
			case 3:
				a ^= a >> 16;
				break;
			}
			a += engineState[(i + 128) & 0xFF];
			uint num2 = (engineState[i] = engineState[(num >> 2) & 0xFF] + a + b);
			results[i] = (b = engineState[(num2 >> 10) & 0xFF] + num);
		}
	}

	private void mix(uint[] x)
	{
		uint[] array;
		(array = x)[0] = array[0] ^ (x[1] << 11);
		(array = x)[3] = array[3] + x[0];
		(array = x)[1] = array[1] + x[2];
		(array = x)[1] = array[1] ^ (x[2] >> 2);
		(array = x)[4] = array[4] + x[1];
		(array = x)[2] = array[2] + x[3];
		(array = x)[2] = array[2] ^ (x[3] << 8);
		(array = x)[5] = array[5] + x[2];
		(array = x)[3] = array[3] + x[4];
		(array = x)[3] = array[3] ^ (x[4] >> 16);
		(array = x)[6] = array[6] + x[3];
		(array = x)[4] = array[4] + x[5];
		(array = x)[4] = array[4] ^ (x[5] << 10);
		(array = x)[7] = array[7] + x[4];
		(array = x)[5] = array[5] + x[6];
		(array = x)[5] = array[5] ^ (x[6] >> 4);
		(array = x)[0] = array[0] + x[5];
		(array = x)[6] = array[6] + x[7];
		(array = x)[6] = array[6] ^ (x[7] << 8);
		(array = x)[1] = array[1] + x[6];
		(array = x)[7] = array[7] + x[0];
		(array = x)[7] = array[7] ^ (x[0] >> 9);
		(array = x)[2] = array[2] + x[7];
		(array = x)[0] = array[0] + x[1];
	}
}
