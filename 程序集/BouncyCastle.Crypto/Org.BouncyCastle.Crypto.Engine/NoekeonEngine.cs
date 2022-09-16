using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class NoekeonEngine : IBlockCipher
{
	private const int Size = 16;

	private static readonly byte[] RoundConstants = new byte[17]
	{
		128, 27, 54, 108, 216, 171, 77, 154, 47, 94,
		188, 99, 198, 151, 53, 106, 212
	};

	private readonly uint[] k = new uint[4];

	private bool _initialised;

	private bool _forEncryption;

	public virtual string AlgorithmName => "Noekeon";

	public virtual bool IsPartialBlockOkay => false;

	public NoekeonEngine()
	{
		_initialised = false;
	}

	public virtual int GetBlockSize()
	{
		return 16;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("Invalid parameters passed to Noekeon init - " + Platform.GetTypeName(parameters), "parameters");
		}
		_forEncryption = forEncryption;
		_initialised = true;
		KeyParameter keyParameter = (KeyParameter)parameters;
		Pack.BE_To_UInt32(keyParameter.GetKey(), 0, k, 0, 4);
		if (!forEncryption)
		{
			uint num = k[0];
			uint num2 = k[1];
			uint num3 = k[2];
			uint num4 = k[3];
			uint num5 = num ^ num3;
			num5 ^= Integers.RotateLeft(num5, 8) ^ Integers.RotateLeft(num5, 24);
			num2 ^= num5;
			num4 ^= num5;
			num5 = num2 ^ num4;
			num5 ^= Integers.RotateLeft(num5, 8) ^ Integers.RotateLeft(num5, 24);
			num ^= num5;
			num3 ^= num5;
			k[0] = num;
			k[1] = num2;
			k[2] = num3;
			k[3] = num4;
		}
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (!_initialised)
		{
			throw new InvalidOperationException(AlgorithmName + " not initialised");
		}
		Check.DataLength(input, inOff, 16, "input buffer too short");
		Check.OutputLength(output, outOff, 16, "output buffer too short");
		if (!_forEncryption)
		{
			return DecryptBlock(input, inOff, output, outOff);
		}
		return EncryptBlock(input, inOff, output, outOff);
	}

	public virtual void Reset()
	{
	}

	private int EncryptBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		uint num = Pack.BE_To_UInt32(input, inOff);
		uint num2 = Pack.BE_To_UInt32(input, inOff + 4);
		uint num3 = Pack.BE_To_UInt32(input, inOff + 8);
		uint num4 = Pack.BE_To_UInt32(input, inOff + 12);
		uint num5 = k[0];
		uint num6 = k[1];
		uint num7 = k[2];
		uint num8 = k[3];
		int num9 = 0;
		while (true)
		{
			num ^= RoundConstants[num9];
			uint num10 = num ^ num3;
			num10 ^= Integers.RotateLeft(num10, 8) ^ Integers.RotateLeft(num10, 24);
			num2 ^= num10;
			num4 ^= num10;
			num ^= num5;
			num2 ^= num6;
			num3 ^= num7;
			num4 ^= num8;
			num10 = num2 ^ num4;
			num10 ^= Integers.RotateLeft(num10, 8) ^ Integers.RotateLeft(num10, 24);
			num ^= num10;
			num3 ^= num10;
			if (++num9 > 16)
			{
				break;
			}
			num2 = Integers.RotateLeft(num2, 1);
			num3 = Integers.RotateLeft(num3, 5);
			num4 = Integers.RotateLeft(num4, 2);
			num2 ^= ~num4 & ~num3;
			num ^= num3 & num2;
			num10 = num4;
			num4 = num;
			num = num10;
			num3 ^= num ^ num2 ^ num4;
			num2 ^= ~num4 & ~num3;
			num ^= num3 & num2;
			num2 = Integers.RotateLeft(num2, 31);
			num3 = Integers.RotateLeft(num3, 27);
			num4 = Integers.RotateLeft(num4, 30);
		}
		Pack.UInt32_To_BE(num, output, outOff);
		Pack.UInt32_To_BE(num2, output, outOff + 4);
		Pack.UInt32_To_BE(num3, output, outOff + 8);
		Pack.UInt32_To_BE(num4, output, outOff + 12);
		return 16;
	}

	private int DecryptBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		uint num = Pack.BE_To_UInt32(input, inOff);
		uint num2 = Pack.BE_To_UInt32(input, inOff + 4);
		uint num3 = Pack.BE_To_UInt32(input, inOff + 8);
		uint num4 = Pack.BE_To_UInt32(input, inOff + 12);
		uint num5 = k[0];
		uint num6 = k[1];
		uint num7 = k[2];
		uint num8 = k[3];
		int num9 = 16;
		while (true)
		{
			uint num10 = num ^ num3;
			num10 ^= Integers.RotateLeft(num10, 8) ^ Integers.RotateLeft(num10, 24);
			num2 ^= num10;
			num4 ^= num10;
			num ^= num5;
			num2 ^= num6;
			num3 ^= num7;
			num4 ^= num8;
			num10 = num2 ^ num4;
			num10 ^= Integers.RotateLeft(num10, 8) ^ Integers.RotateLeft(num10, 24);
			num ^= num10;
			num3 ^= num10;
			num ^= RoundConstants[num9];
			if (--num9 < 0)
			{
				break;
			}
			num2 = Integers.RotateLeft(num2, 1);
			num3 = Integers.RotateLeft(num3, 5);
			num4 = Integers.RotateLeft(num4, 2);
			num2 ^= ~num4 & ~num3;
			num ^= num3 & num2;
			num10 = num4;
			num4 = num;
			num = num10;
			num3 ^= num ^ num2 ^ num4;
			num2 ^= ~num4 & ~num3;
			num ^= num3 & num2;
			num2 = Integers.RotateLeft(num2, 31);
			num3 = Integers.RotateLeft(num3, 27);
			num4 = Integers.RotateLeft(num4, 30);
		}
		Pack.UInt32_To_BE(num, output, outOff);
		Pack.UInt32_To_BE(num2, output, outOff + 4);
		Pack.UInt32_To_BE(num3, output, outOff + 8);
		Pack.UInt32_To_BE(num4, output, outOff + 12);
		return 16;
	}
}
