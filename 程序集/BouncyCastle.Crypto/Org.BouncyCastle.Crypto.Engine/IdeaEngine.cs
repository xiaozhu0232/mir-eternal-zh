using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class IdeaEngine : IBlockCipher
{
	private const int BLOCK_SIZE = 8;

	private int[] workingKey;

	private static readonly int MASK = 65535;

	private static readonly int BASE = 65537;

	public virtual string AlgorithmName => "IDEA";

	public virtual bool IsPartialBlockOkay => false;

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("invalid parameter passed to IDEA init - " + Platform.GetTypeName(parameters));
		}
		workingKey = GenerateWorkingKey(forEncryption, ((KeyParameter)parameters).GetKey());
	}

	public virtual int GetBlockSize()
	{
		return 8;
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (workingKey == null)
		{
			throw new InvalidOperationException("IDEA engine not initialised");
		}
		Check.DataLength(input, inOff, 8, "input buffer too short");
		Check.OutputLength(output, outOff, 8, "output buffer too short");
		IdeaFunc(workingKey, input, inOff, output, outOff);
		return 8;
	}

	public virtual void Reset()
	{
	}

	private int BytesToWord(byte[] input, int inOff)
	{
		return ((input[inOff] << 8) & 0xFF00) + (input[inOff + 1] & 0xFF);
	}

	private void WordToBytes(int word, byte[] outBytes, int outOff)
	{
		outBytes[outOff] = (byte)((uint)word >> 8);
		outBytes[outOff + 1] = (byte)word;
	}

	private int Mul(int x, int y)
	{
		if (x == 0)
		{
			x = BASE - y;
		}
		else if (y == 0)
		{
			x = BASE - x;
		}
		else
		{
			int num = x * y;
			y = num & MASK;
			x = (int)((uint)num >> 16);
			x = y - x + ((y < x) ? 1 : 0);
		}
		return x & MASK;
	}

	private void IdeaFunc(int[] workingKey, byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = 0;
		int x = BytesToWord(input, inOff);
		int num2 = BytesToWord(input, inOff + 2);
		int num3 = BytesToWord(input, inOff + 4);
		int x2 = BytesToWord(input, inOff + 6);
		for (int i = 0; i < 8; i++)
		{
			x = Mul(x, workingKey[num++]);
			num2 += workingKey[num++];
			num2 &= MASK;
			num3 += workingKey[num++];
			num3 &= MASK;
			x2 = Mul(x2, workingKey[num++]);
			int num4 = num2;
			int num5 = num3;
			num3 ^= x;
			num2 ^= x2;
			num3 = Mul(num3, workingKey[num++]);
			num2 += num3;
			num2 &= MASK;
			num2 = Mul(num2, workingKey[num++]);
			num3 += num2;
			num3 &= MASK;
			x ^= num2;
			x2 ^= num3;
			num2 ^= num5;
			num3 ^= num4;
		}
		WordToBytes(Mul(x, workingKey[num++]), outBytes, outOff);
		WordToBytes(num3 + workingKey[num++], outBytes, outOff + 2);
		WordToBytes(num2 + workingKey[num++], outBytes, outOff + 4);
		WordToBytes(Mul(x2, workingKey[num]), outBytes, outOff + 6);
	}

	private int[] ExpandKey(byte[] uKey)
	{
		int[] array = new int[52];
		if (uKey.Length < 16)
		{
			byte[] array2 = new byte[16];
			Array.Copy(uKey, 0, array2, array2.Length - uKey.Length, uKey.Length);
			uKey = array2;
		}
		for (int i = 0; i < 8; i++)
		{
			array[i] = BytesToWord(uKey, i * 2);
		}
		for (int j = 8; j < 52; j++)
		{
			if ((j & 7) < 6)
			{
				array[j] = (((array[j - 7] & 0x7F) << 9) | (array[j - 6] >> 7)) & MASK;
			}
			else if ((j & 7) == 6)
			{
				array[j] = (((array[j - 7] & 0x7F) << 9) | (array[j - 14] >> 7)) & MASK;
			}
			else
			{
				array[j] = (((array[j - 15] & 0x7F) << 9) | (array[j - 14] >> 7)) & MASK;
			}
		}
		return array;
	}

	private int MulInv(int x)
	{
		if (x < 2)
		{
			return x;
		}
		int num = 1;
		int num2 = BASE / x;
		int num3 = BASE % x;
		while (num3 != 1)
		{
			int num4 = x / num3;
			x %= num3;
			num = (num + num2 * num4) & MASK;
			if (x == 1)
			{
				return num;
			}
			num4 = num3 / x;
			num3 %= x;
			num2 = (num2 + num * num4) & MASK;
		}
		return (1 - num2) & MASK;
	}

	private int AddInv(int x)
	{
		return -x & MASK;
	}

	private int[] InvertKey(int[] inKey)
	{
		int num = 52;
		int[] array = new int[52];
		int num2 = 0;
		int num3 = MulInv(inKey[num2++]);
		int num4 = AddInv(inKey[num2++]);
		int num5 = AddInv(inKey[num2++]);
		int num6 = MulInv(inKey[num2++]);
		array[--num] = num6;
		array[--num] = num5;
		array[--num] = num4;
		array[--num] = num3;
		for (int i = 1; i < 8; i++)
		{
			num3 = inKey[num2++];
			num4 = inKey[num2++];
			array[--num] = num4;
			array[--num] = num3;
			num3 = MulInv(inKey[num2++]);
			num4 = AddInv(inKey[num2++]);
			num5 = AddInv(inKey[num2++]);
			num6 = MulInv(inKey[num2++]);
			array[--num] = num6;
			array[--num] = num4;
			array[--num] = num5;
			array[--num] = num3;
		}
		num3 = inKey[num2++];
		num4 = inKey[num2++];
		array[--num] = num4;
		array[--num] = num3;
		num3 = MulInv(inKey[num2++]);
		num4 = AddInv(inKey[num2++]);
		num5 = AddInv(inKey[num2++]);
		num6 = MulInv(inKey[num2]);
		array[--num] = num6;
		array[--num] = num5;
		array[--num] = num4;
		array[--num] = num3;
		return array;
	}

	private int[] GenerateWorkingKey(bool forEncryption, byte[] userKey)
	{
		if (forEncryption)
		{
			return ExpandKey(userKey);
		}
		return InvertKey(ExpandKey(userKey));
	}
}
