using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Modes;

public class OfbBlockCipher : IBlockCipher
{
	private byte[] IV;

	private byte[] ofbV;

	private byte[] ofbOutV;

	private readonly int blockSize;

	private readonly IBlockCipher cipher;

	public string AlgorithmName => cipher.AlgorithmName + "/OFB" + blockSize * 8;

	public bool IsPartialBlockOkay => true;

	public OfbBlockCipher(IBlockCipher cipher, int blockSize)
	{
		this.cipher = cipher;
		this.blockSize = blockSize / 8;
		IV = new byte[cipher.GetBlockSize()];
		ofbV = new byte[cipher.GetBlockSize()];
		ofbOutV = new byte[cipher.GetBlockSize()];
	}

	public IBlockCipher GetUnderlyingCipher()
	{
		return cipher;
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
				for (int i = 0; i < IV.Length - iV.Length; i++)
				{
					IV[i] = 0;
				}
			}
			else
			{
				Array.Copy(iV, 0, IV, 0, IV.Length);
			}
			parameters = parametersWithIV.Parameters;
		}
		Reset();
		if (parameters != null)
		{
			cipher.Init(forEncryption: true, parameters);
		}
	}

	public int GetBlockSize()
	{
		return blockSize;
	}

	public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (inOff + blockSize > input.Length)
		{
			throw new DataLengthException("input buffer too short");
		}
		if (outOff + blockSize > output.Length)
		{
			throw new DataLengthException("output buffer too short");
		}
		cipher.ProcessBlock(ofbV, 0, ofbOutV, 0);
		for (int i = 0; i < blockSize; i++)
		{
			output[outOff + i] = (byte)(ofbOutV[i] ^ input[inOff + i]);
		}
		Array.Copy(ofbV, blockSize, ofbV, 0, ofbV.Length - blockSize);
		Array.Copy(ofbOutV, 0, ofbV, ofbV.Length - blockSize, blockSize);
		return blockSize;
	}

	public void Reset()
	{
		Array.Copy(IV, 0, ofbV, 0, IV.Length);
		cipher.Reset();
	}
}
