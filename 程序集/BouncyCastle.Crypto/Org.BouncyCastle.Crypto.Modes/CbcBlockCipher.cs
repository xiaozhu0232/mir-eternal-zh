using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Modes;

public class CbcBlockCipher : IBlockCipher
{
	private byte[] IV;

	private byte[] cbcV;

	private byte[] cbcNextV;

	private int blockSize;

	private IBlockCipher cipher;

	private bool encrypting;

	public string AlgorithmName => cipher.AlgorithmName + "/CBC";

	public bool IsPartialBlockOkay => false;

	public CbcBlockCipher(IBlockCipher cipher)
	{
		this.cipher = cipher;
		blockSize = cipher.GetBlockSize();
		IV = new byte[blockSize];
		cbcV = new byte[blockSize];
		cbcNextV = new byte[blockSize];
	}

	public IBlockCipher GetUnderlyingCipher()
	{
		return cipher;
	}

	public void Init(bool forEncryption, ICipherParameters parameters)
	{
		bool flag = encrypting;
		encrypting = forEncryption;
		if (parameters is ParametersWithIV)
		{
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			byte[] iV = parametersWithIV.GetIV();
			if (iV.Length != blockSize)
			{
				throw new ArgumentException("initialisation vector must be the same length as block size");
			}
			Array.Copy(iV, 0, IV, 0, iV.Length);
			parameters = parametersWithIV.Parameters;
		}
		Reset();
		if (parameters != null)
		{
			cipher.Init(encrypting, parameters);
		}
		else if (flag != encrypting)
		{
			throw new ArgumentException("cannot change encrypting state without providing key.");
		}
	}

	public int GetBlockSize()
	{
		return cipher.GetBlockSize();
	}

	public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (!encrypting)
		{
			return DecryptBlock(input, inOff, output, outOff);
		}
		return EncryptBlock(input, inOff, output, outOff);
	}

	public void Reset()
	{
		Array.Copy(IV, 0, cbcV, 0, IV.Length);
		Array.Clear(cbcNextV, 0, cbcNextV.Length);
		cipher.Reset();
	}

	private int EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		if (inOff + blockSize > input.Length)
		{
			throw new DataLengthException("input buffer too short");
		}
		for (int i = 0; i < blockSize; i++)
		{
			byte[] array;
			byte[] array2 = (array = cbcV);
			int num = i;
			nint num2 = num;
			array2[num] = (byte)(array[num2] ^ input[inOff + i]);
		}
		int result = cipher.ProcessBlock(cbcV, 0, outBytes, outOff);
		Array.Copy(outBytes, outOff, cbcV, 0, cbcV.Length);
		return result;
	}

	private int DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		if (inOff + blockSize > input.Length)
		{
			throw new DataLengthException("input buffer too short");
		}
		Array.Copy(input, inOff, cbcNextV, 0, blockSize);
		int result = cipher.ProcessBlock(input, inOff, outBytes, outOff);
		for (int i = 0; i < blockSize; i++)
		{
			byte[] array;
			byte[] array2 = (array = outBytes);
			int num = outOff + i;
			nint num2 = num;
			array2[num] = (byte)(array[num2] ^ cbcV[i]);
		}
		byte[] array3 = cbcV;
		cbcV = cbcNextV;
		cbcNextV = array3;
		return result;
	}
}
