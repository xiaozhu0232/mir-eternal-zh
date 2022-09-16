using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Modes;

public class GOfbBlockCipher : IBlockCipher
{
	private const int C1 = 16843012;

	private const int C2 = 16843009;

	private byte[] IV;

	private byte[] ofbV;

	private byte[] ofbOutV;

	private readonly int blockSize;

	private readonly IBlockCipher cipher;

	private bool firstStep = true;

	private int N3;

	private int N4;

	public string AlgorithmName => cipher.AlgorithmName + "/GCTR";

	public bool IsPartialBlockOkay => true;

	public GOfbBlockCipher(IBlockCipher cipher)
	{
		this.cipher = cipher;
		blockSize = cipher.GetBlockSize();
		if (blockSize != 8)
		{
			throw new ArgumentException("GCTR only for 64 bit block ciphers");
		}
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
		firstStep = true;
		N3 = 0;
		N4 = 0;
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
		if (firstStep)
		{
			firstStep = false;
			cipher.ProcessBlock(ofbV, 0, ofbOutV, 0);
			N3 = bytesToint(ofbOutV, 0);
			N4 = bytesToint(ofbOutV, 4);
		}
		N3 += 16843009;
		N4 += 16843012;
		if (N4 < 16843012 && N4 > 0)
		{
			N4++;
		}
		intTobytes(N3, ofbV, 0);
		intTobytes(N4, ofbV, 4);
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

	private int bytesToint(byte[] inBytes, int inOff)
	{
		return (int)((inBytes[inOff + 3] << 24) & 0xFF000000u) + ((inBytes[inOff + 2] << 16) & 0xFF0000) + ((inBytes[inOff + 1] << 8) & 0xFF00) + (inBytes[inOff] & 0xFF);
	}

	private void intTobytes(int num, byte[] outBytes, int outOff)
	{
		outBytes[outOff + 3] = (byte)(num >> 24);
		outBytes[outOff + 2] = (byte)(num >> 16);
		outBytes[outOff + 1] = (byte)(num >> 8);
		outBytes[outOff] = (byte)num;
	}
}
