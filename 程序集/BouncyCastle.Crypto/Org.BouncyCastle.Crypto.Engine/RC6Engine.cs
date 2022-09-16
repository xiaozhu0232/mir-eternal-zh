using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class RC6Engine : IBlockCipher
{
	private static readonly int wordSize = 32;

	private static readonly int bytesPerWord = wordSize / 8;

	private static readonly int _noRounds = 20;

	private int[] _S;

	private static readonly int P32 = -1209970333;

	private static readonly int Q32 = -1640531527;

	private static readonly int LGW = 5;

	private bool forEncryption;

	public virtual string AlgorithmName => "RC6";

	public virtual bool IsPartialBlockOkay => false;

	public virtual int GetBlockSize()
	{
		return 4 * bytesPerWord;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("invalid parameter passed to RC6 init - " + Platform.GetTypeName(parameters));
		}
		this.forEncryption = forEncryption;
		KeyParameter keyParameter = (KeyParameter)parameters;
		SetKey(keyParameter.GetKey());
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		int blockSize = GetBlockSize();
		if (_S == null)
		{
			throw new InvalidOperationException("RC6 engine not initialised");
		}
		Check.DataLength(input, inOff, blockSize, "input buffer too short");
		Check.OutputLength(output, outOff, blockSize, "output buffer too short");
		if (!forEncryption)
		{
			return DecryptBlock(input, inOff, output, outOff);
		}
		return EncryptBlock(input, inOff, output, outOff);
	}

	public virtual void Reset()
	{
	}

	private void SetKey(byte[] key)
	{
		if ((key.Length + (bytesPerWord - 1)) / bytesPerWord == 0)
		{
			int num = 1;
		}
		int[] array = new int[(key.Length + bytesPerWord - 1) / bytesPerWord];
		for (int num2 = key.Length - 1; num2 >= 0; num2--)
		{
			array[num2 / bytesPerWord] = (array[num2 / bytesPerWord] << 8) + (key[num2] & 0xFF);
		}
		_S = new int[2 + 2 * _noRounds + 2];
		_S[0] = P32;
		for (int i = 1; i < _S.Length; i++)
		{
			_S[i] = _S[i - 1] + Q32;
		}
		int num3 = ((array.Length <= _S.Length) ? (3 * _S.Length) : (3 * array.Length));
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		for (int j = 0; j < num3; j++)
		{
			num4 = (_S[num6] = RotateLeft(_S[num6] + num4 + num5, 3));
			num5 = (array[num7] = RotateLeft(array[num7] + num4 + num5, num4 + num5));
			num6 = (num6 + 1) % _S.Length;
			num7 = (num7 + 1) % array.Length;
		}
	}

	private int EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = BytesToWord(input, inOff);
		int num2 = BytesToWord(input, inOff + bytesPerWord);
		int num3 = BytesToWord(input, inOff + bytesPerWord * 2);
		int num4 = BytesToWord(input, inOff + bytesPerWord * 3);
		num2 += _S[0];
		num4 += _S[1];
		for (int i = 1; i <= _noRounds; i++)
		{
			int num5 = 0;
			int num6 = 0;
			num5 = num2 * (2 * num2 + 1);
			num5 = RotateLeft(num5, 5);
			num6 = num4 * (2 * num4 + 1);
			num6 = RotateLeft(num6, 5);
			num ^= num5;
			num = RotateLeft(num, num6);
			num += _S[2 * i];
			num3 ^= num6;
			num3 = RotateLeft(num3, num5);
			num3 += _S[2 * i + 1];
			int num7 = num;
			num = num2;
			num2 = num3;
			num3 = num4;
			num4 = num7;
		}
		num += _S[2 * _noRounds + 2];
		num3 += _S[2 * _noRounds + 3];
		WordToBytes(num, outBytes, outOff);
		WordToBytes(num2, outBytes, outOff + bytesPerWord);
		WordToBytes(num3, outBytes, outOff + bytesPerWord * 2);
		WordToBytes(num4, outBytes, outOff + bytesPerWord * 3);
		return 4 * bytesPerWord;
	}

	private int DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = BytesToWord(input, inOff);
		int num2 = BytesToWord(input, inOff + bytesPerWord);
		int num3 = BytesToWord(input, inOff + bytesPerWord * 2);
		int num4 = BytesToWord(input, inOff + bytesPerWord * 3);
		num3 -= _S[2 * _noRounds + 3];
		num -= _S[2 * _noRounds + 2];
		for (int num5 = _noRounds; num5 >= 1; num5--)
		{
			int num6 = 0;
			int num7 = 0;
			int num8 = num4;
			num4 = num3;
			num3 = num2;
			num2 = num;
			num = num8;
			num6 = num2 * (2 * num2 + 1);
			num6 = RotateLeft(num6, LGW);
			num7 = num4 * (2 * num4 + 1);
			num7 = RotateLeft(num7, LGW);
			num3 -= _S[2 * num5 + 1];
			num3 = RotateRight(num3, num6);
			num3 ^= num7;
			num -= _S[2 * num5];
			num = RotateRight(num, num7);
			num ^= num6;
		}
		num4 -= _S[1];
		num2 -= _S[0];
		WordToBytes(num, outBytes, outOff);
		WordToBytes(num2, outBytes, outOff + bytesPerWord);
		WordToBytes(num3, outBytes, outOff + bytesPerWord * 2);
		WordToBytes(num4, outBytes, outOff + bytesPerWord * 3);
		return 4 * bytesPerWord;
	}

	private int RotateLeft(int x, int y)
	{
		return (x << (y & (wordSize - 1))) | (int)((uint)x >> wordSize - (y & (wordSize - 1)));
	}

	private int RotateRight(int x, int y)
	{
		return (int)((uint)x >> (y & (wordSize - 1))) | (x << wordSize - (y & (wordSize - 1)));
	}

	private int BytesToWord(byte[] src, int srcOff)
	{
		int num = 0;
		for (int num2 = bytesPerWord - 1; num2 >= 0; num2--)
		{
			num = (num << 8) + (src[num2 + srcOff] & 0xFF);
		}
		return num;
	}

	private void WordToBytes(int word, byte[] dst, int dstOff)
	{
		for (int i = 0; i < bytesPerWord; i++)
		{
			dst[i + dstOff] = (byte)word;
			word = (int)((uint)word >> 8);
		}
	}
}
