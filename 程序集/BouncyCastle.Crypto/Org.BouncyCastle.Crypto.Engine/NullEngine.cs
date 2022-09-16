using System;

namespace Org.BouncyCastle.Crypto.Engines;

public class NullEngine : IBlockCipher
{
	private const int BlockSize = 1;

	private bool initialised;

	public virtual string AlgorithmName => "Null";

	public virtual bool IsPartialBlockOkay => true;

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		initialised = true;
	}

	public virtual int GetBlockSize()
	{
		return 1;
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (!initialised)
		{
			throw new InvalidOperationException("Null engine not initialised");
		}
		Check.DataLength(input, inOff, 1, "input buffer too short");
		Check.OutputLength(output, outOff, 1, "output buffer too short");
		for (int i = 0; i < 1; i++)
		{
			output[outOff + i] = input[inOff + i];
		}
		return 1;
	}

	public virtual void Reset()
	{
	}
}
