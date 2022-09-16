using System;
using System.Collections;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class Dstu7624WrapEngine : IWrapper
{
	private KeyParameter param;

	private Dstu7624Engine engine;

	private bool forWrapping;

	private int blockSize;

	public string AlgorithmName => "Dstu7624WrapEngine";

	public Dstu7624WrapEngine(int blockSizeBits)
	{
		engine = new Dstu7624Engine(blockSizeBits);
		param = null;
		blockSize = blockSizeBits / 8;
	}

	public void Init(bool forWrapping, ICipherParameters parameters)
	{
		this.forWrapping = forWrapping;
		if (parameters is KeyParameter)
		{
			param = (KeyParameter)parameters;
			engine.Init(forWrapping, param);
			return;
		}
		throw new ArgumentException("Bad parameters passed to Dstu7624WrapEngine");
	}

	public byte[] Wrap(byte[] input, int inOff, int length)
	{
		if (!forWrapping)
		{
			throw new InvalidOperationException("Not set for wrapping");
		}
		if (length % blockSize != 0)
		{
			throw new ArgumentException("Padding not supported");
		}
		int num = 2 * (1 + length / blockSize);
		int num2 = (num - 1) * 6;
		byte[] array = new byte[length + blockSize];
		Array.Copy(input, inOff, array, 0, length);
		byte[] array2 = new byte[blockSize / 2];
		Array.Copy(array, 0, array2, 0, blockSize / 2);
		IList list = Platform.CreateArrayList();
		int num3 = array.Length - blockSize / 2;
		int num4 = blockSize / 2;
		while (num3 != 0)
		{
			byte[] array3 = new byte[blockSize / 2];
			Array.Copy(array, num4, array3, 0, blockSize / 2);
			list.Add(array3);
			num3 -= blockSize / 2;
			num4 += blockSize / 2;
		}
		for (int i = 0; i < num2; i++)
		{
			Array.Copy(array2, 0, array, 0, blockSize / 2);
			Array.Copy((byte[])list[0], 0, array, blockSize / 2, blockSize / 2);
			engine.ProcessBlock(array, 0, array, 0);
			byte[] array4 = Pack.UInt32_To_LE((uint)(i + 1));
			for (int j = 0; j < array4.Length; j++)
			{
				byte[] array5;
				byte[] array6 = (array5 = array);
				int num5 = j + blockSize / 2;
				nint num6 = num5;
				array6[num5] = (byte)(array5[num6] ^ array4[j]);
			}
			Array.Copy(array, blockSize / 2, array2, 0, blockSize / 2);
			for (int k = 2; k < num; k++)
			{
				Array.Copy((byte[])list[k - 1], 0, (byte[])list[k - 2], 0, blockSize / 2);
			}
			Array.Copy(array, 0, (byte[])list[num - 2], 0, blockSize / 2);
		}
		Array.Copy(array2, 0, array, 0, blockSize / 2);
		num4 = blockSize / 2;
		for (int l = 0; l < num - 1; l++)
		{
			Array.Copy((byte[])list[l], 0, array, num4, blockSize / 2);
			num4 += blockSize / 2;
		}
		return array;
	}

	public byte[] Unwrap(byte[] input, int inOff, int length)
	{
		if (forWrapping)
		{
			throw new InvalidOperationException("not set for unwrapping");
		}
		if (length % blockSize != 0)
		{
			throw new ArgumentException("Padding not supported");
		}
		int num = 2 * length / blockSize;
		int num2 = (num - 1) * 6;
		byte[] array = new byte[length];
		Array.Copy(input, inOff, array, 0, length);
		byte[] array2 = new byte[blockSize / 2];
		Array.Copy(array, 0, array2, 0, blockSize / 2);
		IList list = Platform.CreateArrayList();
		int num3 = array.Length - blockSize / 2;
		int num4 = blockSize / 2;
		while (num3 != 0)
		{
			byte[] array3 = new byte[blockSize / 2];
			Array.Copy(array, num4, array3, 0, blockSize / 2);
			list.Add(array3);
			num3 -= blockSize / 2;
			num4 += blockSize / 2;
		}
		for (int i = 0; i < num2; i++)
		{
			Array.Copy((byte[])list[num - 2], 0, array, 0, blockSize / 2);
			Array.Copy(array2, 0, array, blockSize / 2, blockSize / 2);
			byte[] array4 = Pack.UInt32_To_LE((uint)(num2 - i));
			for (int j = 0; j < array4.Length; j++)
			{
				byte[] array5;
				byte[] array6 = (array5 = array);
				int num5 = j + blockSize / 2;
				nint num6 = num5;
				array6[num5] = (byte)(array5[num6] ^ array4[j]);
			}
			engine.ProcessBlock(array, 0, array, 0);
			Array.Copy(array, 0, array2, 0, blockSize / 2);
			for (int k = 2; k < num; k++)
			{
				Array.Copy((byte[])list[num - k - 1], 0, (byte[])list[num - k], 0, blockSize / 2);
			}
			Array.Copy(array, blockSize / 2, (byte[])list[0], 0, blockSize / 2);
		}
		Array.Copy(array2, 0, array, 0, blockSize / 2);
		num4 = blockSize / 2;
		for (int l = 0; l < num - 1; l++)
		{
			Array.Copy((byte[])list[l], 0, array, num4, blockSize / 2);
			num4 += blockSize / 2;
		}
		byte b = 0;
		for (int m = array.Length - blockSize; m < array.Length; m++)
		{
			b = (byte)(b | array[m]);
		}
		if (b != 0)
		{
			throw new InvalidCipherTextException("checksum failed");
		}
		return Arrays.CopyOfRange(array, 0, array.Length - blockSize);
	}
}
