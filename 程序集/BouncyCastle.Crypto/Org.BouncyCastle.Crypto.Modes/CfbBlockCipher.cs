using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Modes;

public class CfbBlockCipher : IBlockCipher
{
	private byte[] IV;

	private byte[] cfbV;

	private byte[] cfbOutV;

	private bool encrypting;

	private readonly int blockSize;

	private readonly IBlockCipher cipher;

	public string AlgorithmName => cipher.AlgorithmName + "/CFB" + blockSize * 8;

	public bool IsPartialBlockOkay => true;

	public CfbBlockCipher(IBlockCipher cipher, int bitBlockSize)
	{
		if (bitBlockSize < 8 || ((uint)bitBlockSize & 7u) != 0)
		{
			throw new ArgumentException("CFB" + bitBlockSize + " not supported", "bitBlockSize");
		}
		this.cipher = cipher;
		blockSize = bitBlockSize / 8;
		IV = new byte[cipher.GetBlockSize()];
		cfbV = new byte[cipher.GetBlockSize()];
		cfbOutV = new byte[cipher.GetBlockSize()];
	}

	public IBlockCipher GetUnderlyingCipher()
	{
		return cipher;
	}

	public void Init(bool forEncryption, ICipherParameters parameters)
	{
		encrypting = forEncryption;
		if (parameters is ParametersWithIV)
		{
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			byte[] iV = parametersWithIV.GetIV();
			int num = IV.Length - iV.Length;
			Array.Copy(iV, 0, IV, num, iV.Length);
			Array.Clear(IV, 0, num);
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
		if (!encrypting)
		{
			return DecryptBlock(input, inOff, output, outOff);
		}
		return EncryptBlock(input, inOff, output, outOff);
	}

	public int EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
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

	public int DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
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
		Array.Copy(cfbV, blockSize, cfbV, 0, cfbV.Length - blockSize);
		Array.Copy(input, inOff, cfbV, cfbV.Length - blockSize, blockSize);
		for (int i = 0; i < blockSize; i++)
		{
			outBytes[outOff + i] = (byte)(cfbOutV[i] ^ input[inOff + i]);
		}
		return blockSize;
	}

	public void Reset()
	{
		Array.Copy(IV, 0, cfbV, 0, IV.Length);
		cipher.Reset();
	}
}
