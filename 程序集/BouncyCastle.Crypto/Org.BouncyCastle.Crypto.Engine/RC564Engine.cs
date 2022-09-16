using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class RC564Engine : IBlockCipher
{
	private static readonly int wordSize = 64;

	private static readonly int bytesPerWord = wordSize / 8;

	private int _noRounds;

	private long[] _S;

	private static readonly long P64 = -5196783011329398165L;

	private static readonly long Q64 = -7046029254386353131L;

	private bool forEncryption;

	public virtual string AlgorithmName => "RC5-64";

	public virtual bool IsPartialBlockOkay => false;

	public RC564Engine()
	{
		_noRounds = 12;
	}

	public virtual int GetBlockSize()
	{
		return 2 * bytesPerWord;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!typeof(RC5Parameters).IsInstanceOfType(parameters))
		{
			throw new ArgumentException("invalid parameter passed to RC564 init - " + Platform.GetTypeName(parameters));
		}
		RC5Parameters rC5Parameters = (RC5Parameters)parameters;
		this.forEncryption = forEncryption;
		_noRounds = rC5Parameters.Rounds;
		SetKey(rC5Parameters.GetKey());
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
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
		long[] array = new long[(key.Length + (bytesPerWord - 1)) / bytesPerWord];
		for (int i = 0; i != key.Length; i++)
		{
			long[] array2;
			long[] array3 = (array2 = array);
			int num = i / bytesPerWord;
			nint num2 = num;
			array3[num] = array2[num2] + ((long)(key[i] & 0xFF) << 8 * (i % bytesPerWord));
		}
		_S = new long[2 * (_noRounds + 1)];
		_S[0] = P64;
		for (int j = 1; j < _S.Length; j++)
		{
			_S[j] = _S[j - 1] + Q64;
		}
		int num3 = ((array.Length <= _S.Length) ? (3 * _S.Length) : (3 * array.Length));
		long num4 = 0L;
		long num5 = 0L;
		int num6 = 0;
		int num7 = 0;
		for (int k = 0; k < num3; k++)
		{
			num4 = (_S[num6] = RotateLeft(_S[num6] + num4 + num5, 3L));
			num5 = (array[num7] = RotateLeft(array[num7] + num4 + num5, num4 + num5));
			num6 = (num6 + 1) % _S.Length;
			num7 = (num7 + 1) % array.Length;
		}
	}

	private int EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		long num = BytesToWord(input, inOff) + _S[0];
		long num2 = BytesToWord(input, inOff + bytesPerWord) + _S[1];
		for (int i = 1; i <= _noRounds; i++)
		{
			num = RotateLeft(num ^ num2, num2) + _S[2 * i];
			num2 = RotateLeft(num2 ^ num, num) + _S[2 * i + 1];
		}
		WordToBytes(num, outBytes, outOff);
		WordToBytes(num2, outBytes, outOff + bytesPerWord);
		return 2 * bytesPerWord;
	}

	private int DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		long num = BytesToWord(input, inOff);
		long num2 = BytesToWord(input, inOff + bytesPerWord);
		for (int num3 = _noRounds; num3 >= 1; num3--)
		{
			num2 = RotateRight(num2 - _S[2 * num3 + 1], num) ^ num;
			num = RotateRight(num - _S[2 * num3], num2) ^ num2;
		}
		WordToBytes(num - _S[0], outBytes, outOff);
		WordToBytes(num2 - _S[1], outBytes, outOff + bytesPerWord);
		return 2 * bytesPerWord;
	}

	private long RotateLeft(long x, long y)
	{
		return (x << (int)(y & (wordSize - 1))) | (long)((ulong)x >> (int)(wordSize - (y & (wordSize - 1))));
	}

	private long RotateRight(long x, long y)
	{
		return (long)((ulong)x >> (int)(y & (wordSize - 1))) | (x << (int)(wordSize - (y & (wordSize - 1))));
	}

	private long BytesToWord(byte[] src, int srcOff)
	{
		long num = 0L;
		for (int num2 = bytesPerWord - 1; num2 >= 0; num2--)
		{
			num = (num << 8) + (src[num2 + srcOff] & 0xFF);
		}
		return num;
	}

	private void WordToBytes(long word, byte[] dst, int dstOff)
	{
		for (int i = 0; i < bytesPerWord; i++)
		{
			dst[i + dstOff] = (byte)word;
			word = (long)((ulong)word >> 8);
		}
	}
}
