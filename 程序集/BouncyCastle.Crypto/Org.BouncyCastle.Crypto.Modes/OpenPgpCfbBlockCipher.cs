using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Modes;

public class OpenPgpCfbBlockCipher : IBlockCipher
{
	private byte[] IV;

	private byte[] FR;

	private byte[] FRE;

	private readonly IBlockCipher cipher;

	private readonly int blockSize;

	private int count;

	private bool forEncryption;

	public string AlgorithmName => cipher.AlgorithmName + "/OpenPGPCFB";

	public bool IsPartialBlockOkay => true;

	public OpenPgpCfbBlockCipher(IBlockCipher cipher)
	{
		this.cipher = cipher;
		blockSize = cipher.GetBlockSize();
		IV = new byte[blockSize];
		FR = new byte[blockSize];
		FRE = new byte[blockSize];
	}

	public IBlockCipher GetUnderlyingCipher()
	{
		return cipher;
	}

	public int GetBlockSize()
	{
		return cipher.GetBlockSize();
	}

	public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (!forEncryption)
		{
			return DecryptBlock(input, inOff, output, outOff);
		}
		return EncryptBlock(input, inOff, output, outOff);
	}

	public void Reset()
	{
		count = 0;
		Array.Copy(IV, 0, FR, 0, FR.Length);
		cipher.Reset();
	}

	public void Init(bool forEncryption, ICipherParameters parameters)
	{
		this.forEncryption = forEncryption;
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
		cipher.Init(forEncryption: true, parameters);
	}

	private byte EncryptByte(byte data, int blockOff)
	{
		return (byte)(FRE[blockOff] ^ data);
	}

	private int EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		if (inOff + blockSize > input.Length)
		{
			throw new DataLengthException("input buffer too short");
		}
		if (outOff + blockSize > outBytes.Length)
		{
			throw new DataLengthException("output buffer too short");
		}
		if (count > blockSize)
		{
			byte[] fR = FR;
			int num = blockSize - 2;
			byte b;
			outBytes[outOff] = (b = EncryptByte(input[inOff], blockSize - 2));
			fR[num] = b;
			byte[] fR2 = FR;
			int num2 = blockSize - 1;
			outBytes[outOff + 1] = (b = EncryptByte(input[inOff + 1], blockSize - 1));
			fR2[num2] = b;
			cipher.ProcessBlock(FR, 0, FRE, 0);
			for (int i = 2; i < blockSize; i++)
			{
				byte[] fR3 = FR;
				int num3 = i - 2;
				outBytes[outOff + i] = (b = EncryptByte(input[inOff + i], i - 2));
				fR3[num3] = b;
			}
		}
		else if (count == 0)
		{
			cipher.ProcessBlock(FR, 0, FRE, 0);
			for (int j = 0; j < blockSize; j++)
			{
				byte[] fR4 = FR;
				int num4 = j;
				byte b;
				outBytes[outOff + j] = (b = EncryptByte(input[inOff + j], j));
				fR4[num4] = b;
			}
			count += blockSize;
		}
		else if (count == blockSize)
		{
			cipher.ProcessBlock(FR, 0, FRE, 0);
			outBytes[outOff] = EncryptByte(input[inOff], 0);
			outBytes[outOff + 1] = EncryptByte(input[inOff + 1], 1);
			Array.Copy(FR, 2, FR, 0, blockSize - 2);
			Array.Copy(outBytes, outOff, FR, blockSize - 2, 2);
			cipher.ProcessBlock(FR, 0, FRE, 0);
			for (int k = 2; k < blockSize; k++)
			{
				byte[] fR5 = FR;
				int num5 = k - 2;
				byte b;
				outBytes[outOff + k] = (b = EncryptByte(input[inOff + k], k - 2));
				fR5[num5] = b;
			}
			count += blockSize;
		}
		return blockSize;
	}

	private int DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		if (inOff + blockSize > input.Length)
		{
			throw new DataLengthException("input buffer too short");
		}
		if (outOff + blockSize > outBytes.Length)
		{
			throw new DataLengthException("output buffer too short");
		}
		if (count > blockSize)
		{
			byte b = input[inOff];
			FR[blockSize - 2] = b;
			outBytes[outOff] = EncryptByte(b, blockSize - 2);
			b = input[inOff + 1];
			FR[blockSize - 1] = b;
			outBytes[outOff + 1] = EncryptByte(b, blockSize - 1);
			cipher.ProcessBlock(FR, 0, FRE, 0);
			for (int i = 2; i < blockSize; i++)
			{
				b = input[inOff + i];
				FR[i - 2] = b;
				outBytes[outOff + i] = EncryptByte(b, i - 2);
			}
		}
		else if (count == 0)
		{
			cipher.ProcessBlock(FR, 0, FRE, 0);
			for (int j = 0; j < blockSize; j++)
			{
				FR[j] = input[inOff + j];
				outBytes[j] = EncryptByte(input[inOff + j], j);
			}
			count += blockSize;
		}
		else if (count == blockSize)
		{
			cipher.ProcessBlock(FR, 0, FRE, 0);
			byte b2 = input[inOff];
			byte b3 = input[inOff + 1];
			outBytes[outOff] = EncryptByte(b2, 0);
			outBytes[outOff + 1] = EncryptByte(b3, 1);
			Array.Copy(FR, 2, FR, 0, blockSize - 2);
			FR[blockSize - 2] = b2;
			FR[blockSize - 1] = b3;
			cipher.ProcessBlock(FR, 0, FRE, 0);
			for (int k = 2; k < blockSize; k++)
			{
				byte b4 = input[inOff + k];
				FR[k - 2] = b4;
				outBytes[outOff + k] = EncryptByte(b4, k - 2);
			}
			count += blockSize;
		}
		return blockSize;
	}
}
