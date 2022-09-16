using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class RC532Engine : IBlockCipher
{
	private int _noRounds;

	private int[] _S;

	private static readonly int P32 = -1209970333;

	private static readonly int Q32 = -1640531527;

	private bool forEncryption;

	public virtual string AlgorithmName => "RC5-32";

	public virtual bool IsPartialBlockOkay => false;

	public RC532Engine()
	{
		_noRounds = 12;
	}

	public virtual int GetBlockSize()
	{
		return 8;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (typeof(RC5Parameters).IsInstanceOfType(parameters))
		{
			RC5Parameters rC5Parameters = (RC5Parameters)parameters;
			_noRounds = rC5Parameters.Rounds;
			SetKey(rC5Parameters.GetKey());
		}
		else
		{
			if (!typeof(KeyParameter).IsInstanceOfType(parameters))
			{
				throw new ArgumentException("invalid parameter passed to RC532 init - " + Platform.GetTypeName(parameters));
			}
			KeyParameter keyParameter = (KeyParameter)parameters;
			SetKey(keyParameter.GetKey());
		}
		this.forEncryption = forEncryption;
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
		int[] array = new int[(key.Length + 3) / 4];
		for (int i = 0; i != key.Length; i++)
		{
			int[] array2;
			int[] array3 = (array2 = array);
			int num = i / 4;
			nint num2 = num;
			array3[num] = array2[num2] + ((key[i] & 0xFF) << 8 * (i % 4));
		}
		_S = new int[2 * (_noRounds + 1)];
		_S[0] = P32;
		for (int j = 1; j < _S.Length; j++)
		{
			_S[j] = _S[j - 1] + Q32;
		}
		int num3 = ((array.Length <= _S.Length) ? (3 * _S.Length) : (3 * array.Length));
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		for (int k = 0; k < num3; k++)
		{
			num4 = (_S[num6] = RotateLeft(_S[num6] + num4 + num5, 3));
			num5 = (array[num7] = RotateLeft(array[num7] + num4 + num5, num4 + num5));
			num6 = (num6 + 1) % _S.Length;
			num7 = (num7 + 1) % array.Length;
		}
	}

	private int EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = BytesToWord(input, inOff) + _S[0];
		int num2 = BytesToWord(input, inOff + 4) + _S[1];
		for (int i = 1; i <= _noRounds; i++)
		{
			num = RotateLeft(num ^ num2, num2) + _S[2 * i];
			num2 = RotateLeft(num2 ^ num, num) + _S[2 * i + 1];
		}
		WordToBytes(num, outBytes, outOff);
		WordToBytes(num2, outBytes, outOff + 4);
		return 8;
	}

	private int DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = BytesToWord(input, inOff);
		int num2 = BytesToWord(input, inOff + 4);
		for (int num3 = _noRounds; num3 >= 1; num3--)
		{
			num2 = RotateRight(num2 - _S[2 * num3 + 1], num) ^ num;
			num = RotateRight(num - _S[2 * num3], num2) ^ num2;
		}
		WordToBytes(num - _S[0], outBytes, outOff);
		WordToBytes(num2 - _S[1], outBytes, outOff + 4);
		return 8;
	}

	private int RotateLeft(int x, int y)
	{
		return (x << (y & 0x1F)) | (int)((uint)x >> 32 - (y & 0x1F));
	}

	private int RotateRight(int x, int y)
	{
		return (int)((uint)x >> (y & 0x1F)) | (x << 32 - (y & 0x1F));
	}

	private int BytesToWord(byte[] src, int srcOff)
	{
		return (src[srcOff] & 0xFF) | ((src[srcOff + 1] & 0xFF) << 8) | ((src[srcOff + 2] & 0xFF) << 16) | ((src[srcOff + 3] & 0xFF) << 24);
	}

	private void WordToBytes(int word, byte[] dst, int dstOff)
	{
		dst[dstOff] = (byte)word;
		dst[dstOff + 1] = (byte)(word >> 8);
		dst[dstOff + 2] = (byte)(word >> 16);
		dst[dstOff + 3] = (byte)(word >> 24);
	}
}
