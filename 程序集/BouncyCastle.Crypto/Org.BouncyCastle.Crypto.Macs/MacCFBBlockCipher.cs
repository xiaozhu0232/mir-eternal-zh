using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Macs;

internal class MacCFBBlockCipher : IBlockCipher
{
	private byte[] IV;

	private byte[] cfbV;

	private byte[] cfbOutV;

	private readonly int blockSize;

	private readonly IBlockCipher cipher;

	public string AlgorithmName => cipher.AlgorithmName + "/CFB" + blockSize * 8;

	public bool IsPartialBlockOkay => true;

	public MacCFBBlockCipher(IBlockCipher cipher, int bitBlockSize)
	{
		this.cipher = cipher;
		blockSize = bitBlockSize / 8;
		IV = new byte[cipher.GetBlockSize()];
		cfbV = new byte[cipher.GetBlockSize()];
		cfbOutV = new byte[cipher.GetBlockSize()];
	}

	public void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (parameters is ParametersWithIV)
		{
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			byte[] iV = parametersWithIV.GetIV();
			if (iV.Length < IV.Length)
			{
				Array.Copy(iV, 0, IV, IV.Length - iV.Length, iV.Length);
			}
			else
			{
				Array.Copy(iV, 0, IV, 0, IV.Length);
			}
			parameters = parametersWithIV.Parameters;
		}
		Reset();
		cipher.Init(forEncryption: true, parameters);
	}

	public int GetBlockSize()
	{
		return blockSize;
	}

	public int ProcessBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		if (inOff + blockSize > input.Length)
		{
			throw new DataLengthException("input buffer too short");
		}
		if (outOff + blockSize > outBytes.Length)
		{
			throw new DataLengthException("output buffer too short");
		}
		cipher.ProcessBlock(cfbV, 0, cfbOutV, 0);
		for (int i = 0; i < blockSize; i++)
		{
			outBytes[outOff + i] = (byte)(cfbOutV[i] ^ input[inOff + i]);
		}
		Array.Copy(cfbV, blockSize, cfbV, 0, cfbV.Length - blockSize);
		Array.Copy(outBytes, outOff, cfbV, cfbV.Length - blockSize, blockSize);
		return blockSize;
	}

	public void Reset()
	{
		IV.CopyTo(cfbV, 0);
		cipher.Reset();
	}

	public void GetMacBlock(byte[] mac)
	{
		cipher.ProcessBlock(cfbV, 0, mac, 0);
	}
}
