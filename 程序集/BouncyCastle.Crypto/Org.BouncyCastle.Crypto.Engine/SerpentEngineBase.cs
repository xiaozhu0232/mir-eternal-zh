using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public abstract class SerpentEngineBase : IBlockCipher
{
	internal const int ROUNDS = 32;

	internal const int PHI = -1640531527;

	protected static readonly int BlockSize = 16;

	protected bool encrypting;

	protected int[] wKey;

	protected int X0;

	protected int X1;

	protected int X2;

	protected int X3;

	public virtual string AlgorithmName => "Serpent";

	public virtual bool IsPartialBlockOkay => false;

	public virtual void Init(bool encrypting, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("invalid parameter passed to " + AlgorithmName + " init - " + Platform.GetTypeName(parameters));
		}
		this.encrypting = encrypting;
		wKey = MakeWorkingKey(((KeyParameter)parameters).GetKey());
	}

	public virtual int GetBlockSize()
	{
		return BlockSize;
	}

	public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (wKey == null)
		{
			throw new InvalidOperationException(AlgorithmName + " not initialised");
		}
		Check.DataLength(input, inOff, BlockSize, "input buffer too short");
		Check.OutputLength(output, outOff, BlockSize, "output buffer too short");
		if (encrypting)
		{
			EncryptBlock(input, inOff, output, outOff);
		}
		else
		{
			DecryptBlock(input, inOff, output, outOff);
		}
		return BlockSize;
	}

	public virtual void Reset()
	{
	}

	protected static int RotateLeft(int x, int bits)
	{
		return (x << bits) | (int)((uint)x >> 32 - bits);
	}

	private static int RotateRight(int x, int bits)
	{
		return (int)((uint)x >> bits) | (x << 32 - bits);
	}

	protected void Sb0(int a, int b, int c, int d)
	{
		int num = a ^ d;
		int num2 = c ^ num;
		int num3 = b ^ num2;
		X3 = (a & d) ^ num3;
		int num4 = a ^ (b & num);
		X2 = num3 ^ (c | num4);
		int num5 = X3 & (num2 ^ num4);
		X1 = ~num2 ^ num5;
		X0 = num5 ^ ~num4;
	}

	protected void Ib0(int a, int b, int c, int d)
	{
		int num = ~a;
		int num2 = a ^ b;
		int num3 = d ^ (num | num2);
		int num4 = c ^ num3;
		X2 = num2 ^ num4;
		int num5 = num ^ (d & num2);
		X1 = num3 ^ (X2 & num5);
		X3 = (a & num3) ^ (num4 | X1);
		X0 = X3 ^ (num4 ^ num5);
	}

	protected void Sb1(int a, int b, int c, int d)
	{
		int num = b ^ ~a;
		int num2 = c ^ (a | num);
		X2 = d ^ num2;
		int num3 = b ^ (d | num);
		int num4 = num ^ X2;
		X3 = num4 ^ (num2 & num3);
		int num5 = num2 ^ num3;
		X1 = X3 ^ num5;
		X0 = num2 ^ (num4 & num5);
	}

	protected void Ib1(int a, int b, int c, int d)
	{
		int num = b ^ d;
		int num2 = a ^ (b & num);
		int num3 = num ^ num2;
		X3 = c ^ num3;
		int num4 = b ^ (num & num2);
		int num5 = X3 | num4;
		X1 = num2 ^ num5;
		int num6 = ~X1;
		int num7 = X3 ^ num4;
		X0 = num6 ^ num7;
		X2 = num3 ^ (num6 | num7);
	}

	protected void Sb2(int a, int b, int c, int d)
	{
		int num = ~a;
		int num2 = b ^ d;
		int num3 = c & num;
		X0 = num2 ^ num3;
		int num4 = c ^ num;
		int num5 = c ^ X0;
		int num6 = b & num5;
		X3 = num4 ^ num6;
		X2 = a ^ ((d | num6) & (X0 | num4));
		X1 = num2 ^ X3 ^ (X2 ^ (d | num));
	}

	protected void Ib2(int a, int b, int c, int d)
	{
		int num = b ^ d;
		int num2 = ~num;
		int num3 = a ^ c;
		int num4 = c ^ num;
		int num5 = b & num4;
		X0 = num3 ^ num5;
		int num6 = a | num2;
		int num7 = d ^ num6;
		int num8 = num3 | num7;
		X3 = num ^ num8;
		int num9 = ~num4;
		int num10 = X0 | X3;
		X1 = num9 ^ num10;
		X2 = (d & num9) ^ (num3 ^ num10);
	}

	protected void Sb3(int a, int b, int c, int d)
	{
		int num = a ^ b;
		int num2 = a & c;
		int num3 = a | d;
		int num4 = c ^ d;
		int num5 = num & num3;
		int num6 = num2 | num5;
		X2 = num4 ^ num6;
		int num7 = b ^ num3;
		int num8 = num6 ^ num7;
		int num9 = num4 & num8;
		X0 = num ^ num9;
		int num10 = X2 & X0;
		X1 = num8 ^ num10;
		X3 = (b | d) ^ (num4 ^ num10);
	}

	protected void Ib3(int a, int b, int c, int d)
	{
		int num = a | b;
		int num2 = b ^ c;
		int num3 = b & num2;
		int num4 = a ^ num3;
		int num5 = c ^ num4;
		int num6 = d | num4;
		X0 = num2 ^ num6;
		int num7 = num2 | num6;
		int num8 = d ^ num7;
		X2 = num5 ^ num8;
		int num9 = num ^ num8;
		int num10 = X0 & num9;
		X3 = num4 ^ num10;
		X1 = X3 ^ (X0 ^ num9);
	}

	protected void Sb4(int a, int b, int c, int d)
	{
		int num = a ^ d;
		int num2 = d & num;
		int num3 = c ^ num2;
		int num4 = b | num3;
		X3 = num ^ num4;
		int num5 = ~b;
		int num6 = num | num5;
		X0 = num3 ^ num6;
		int num7 = a & X0;
		int num8 = num ^ num5;
		int num9 = num4 & num8;
		X2 = num7 ^ num9;
		X1 = a ^ num3 ^ (num8 & X2);
	}

	protected void Ib4(int a, int b, int c, int d)
	{
		int num = c | d;
		int num2 = a & num;
		int num3 = b ^ num2;
		int num4 = a & num3;
		int num5 = c ^ num4;
		X1 = d ^ num5;
		int num6 = ~a;
		int num7 = num5 & X1;
		X3 = num3 ^ num7;
		int num8 = X1 | num6;
		int num9 = d ^ num8;
		X0 = X3 ^ num9;
		X2 = (num3 & num9) ^ (X1 ^ num6);
	}

	protected void Sb5(int a, int b, int c, int d)
	{
		int num = ~a;
		int num2 = a ^ b;
		int num3 = a ^ d;
		int num4 = c ^ num;
		int num5 = num2 | num3;
		X0 = num4 ^ num5;
		int num6 = d & X0;
		int num7 = num2 ^ X0;
		X1 = num6 ^ num7;
		int num8 = num | X0;
		int num9 = num2 | num6;
		int num10 = num3 ^ num8;
		X2 = num9 ^ num10;
		X3 = b ^ num6 ^ (X1 & num10);
	}

	protected void Ib5(int a, int b, int c, int d)
	{
		int num = ~c;
		int num2 = b & num;
		int num3 = d ^ num2;
		int num4 = a & num3;
		int num5 = b ^ num;
		X3 = num4 ^ num5;
		int num6 = b | X3;
		int num7 = a & num6;
		X1 = num3 ^ num7;
		int num8 = a | d;
		int num9 = num ^ num6;
		X0 = num8 ^ num9;
		X2 = (b & num8) ^ (num4 | (a ^ c));
	}

	protected void Sb6(int a, int b, int c, int d)
	{
		int num = ~a;
		int num2 = a ^ d;
		int num3 = b ^ num2;
		int num4 = num | num2;
		int num5 = c ^ num4;
		X1 = b ^ num5;
		int num6 = num2 | X1;
		int num7 = d ^ num6;
		int num8 = num5 & num7;
		X2 = num3 ^ num8;
		int num9 = num5 ^ num7;
		X0 = X2 ^ num9;
		X3 = ~num5 ^ (num3 & num9);
	}

	protected void Ib6(int a, int b, int c, int d)
	{
		int num = ~a;
		int num2 = a ^ b;
		int num3 = c ^ num2;
		int num4 = c | num;
		int num5 = d ^ num4;
		X1 = num3 ^ num5;
		int num6 = num3 & num5;
		int num7 = num2 ^ num6;
		int num8 = b | num7;
		X3 = num5 ^ num8;
		int num9 = b | X3;
		X0 = num7 ^ num9;
		X2 = (d & num) ^ (num3 ^ num9);
	}

	protected void Sb7(int a, int b, int c, int d)
	{
		int num = b ^ c;
		int num2 = c & num;
		int num3 = d ^ num2;
		int num4 = a ^ num3;
		int num5 = d | num;
		int num6 = num4 & num5;
		X1 = b ^ num6;
		int num7 = num3 | X1;
		int num8 = a & num4;
		X3 = num ^ num8;
		int num9 = num4 ^ num7;
		int num10 = X3 & num9;
		X2 = num3 ^ num10;
		X0 = ~num9 ^ (X3 & X2);
	}

	protected void Ib7(int a, int b, int c, int d)
	{
		int num = c | (a & b);
		int num2 = d & (a | b);
		X3 = num ^ num2;
		int num3 = ~d;
		int num4 = b ^ num2;
		int num5 = num4 | (X3 ^ num3);
		X1 = a ^ num5;
		X0 = c ^ num4 ^ (d | X1);
		X2 = num ^ X1 ^ (X0 ^ (a & X3));
	}

	protected void LT()
	{
		int num = RotateLeft(X0, 13);
		int num2 = RotateLeft(X2, 3);
		int x = X1 ^ num ^ num2;
		int x2 = X3 ^ num2 ^ (num << 3);
		X1 = RotateLeft(x, 1);
		X3 = RotateLeft(x2, 7);
		X0 = RotateLeft(num ^ X1 ^ X3, 5);
		X2 = RotateLeft(num2 ^ X3 ^ (X1 << 7), 22);
	}

	protected void InverseLT()
	{
		int num = RotateRight(X2, 22) ^ X3 ^ (X1 << 7);
		int num2 = RotateRight(X0, 5) ^ X1 ^ X3;
		int num3 = RotateRight(X3, 7);
		int num4 = RotateRight(X1, 1);
		X3 = num3 ^ num ^ (num2 << 3);
		X1 = num4 ^ num2 ^ num;
		X2 = RotateRight(num, 3);
		X0 = RotateRight(num2, 13);
	}

	protected abstract int[] MakeWorkingKey(byte[] key);

	protected abstract void EncryptBlock(byte[] input, int inOff, byte[] output, int outOff);

	protected abstract void DecryptBlock(byte[] input, int inOff, byte[] output, int outOff);
}
