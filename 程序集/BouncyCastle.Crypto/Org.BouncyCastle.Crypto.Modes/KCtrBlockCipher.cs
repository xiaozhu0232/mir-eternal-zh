using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Modes;

public class KCtrBlockCipher : IStreamCipher, IBlockCipher
{
	private byte[] IV;

	private byte[] ofbV;

	private byte[] ofbOutV;

	private bool initialised;

	private int byteCount;

	private readonly int blockSize;

	private readonly IBlockCipher cipher;

	public string AlgorithmName => cipher.AlgorithmName + "/KCTR";

	public bool IsPartialBlockOkay => true;

	public KCtrBlockCipher(IBlockCipher cipher)
	{
		this.cipher = cipher;
		IV = new byte[cipher.GetBlockSize()];
		blockSize = cipher.GetBlockSize();
		ofbV = new byte[cipher.GetBlockSize()];
		ofbOutV = new byte[cipher.GetBlockSize()];
	}

	public IBlockCipher GetUnderlyingCipher()
	{
		return cipher;
	}

	public void Init(bool forEncryption, ICipherParameters parameters)
	{
		initialised = true;
		if (parameters is ParametersWithIV)
		{
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			byte[] iV = parametersWithIV.GetIV();
			int destinationIndex = IV.Length - iV.Length;
			Array.Clear(IV, 0, IV.Length);
			Array.Copy(iV, 0, IV, destinationIndex, iV.Length);
			parameters = parametersWithIV.Parameters;
			if (parameters != null)
			{
				cipher.Init(forEncryption: true, parameters);
			}
			Reset();
			return;
		}
		throw new ArgumentException("Invalid parameter passed");
	}

	public int GetBlockSize()
	{
		return cipher.GetBlockSize();
	}

	public byte ReturnByte(byte input)
	{
		return CalculateByte(input);
	}

	public void ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
	{
		if (outOff + len > output.Length)
		{
			throw new DataLengthException("Output buffer too short");
		}
		if (inOff + len > input.Length)
		{
			throw new DataLengthException("Input buffer too small");
		}
		int num = inOff;
		int num2 = inOff + len;
		int num3 = outOff;
		while (num < num2)
		{
			output[num3++] = CalculateByte(input[num++]);
		}
	}

	protected byte CalculateByte(byte b)
	{
		if (byteCount == 0)
		{
			incrementCounterAt(0);
			checkCounter();
			cipher.ProcessBlock(ofbV, 0, ofbOutV, 0);
			return (byte)(ofbOutV[byteCount++] ^ b);
		}
		byte result = (byte)(ofbOutV[byteCount++] ^ b);
		if (byteCount == ofbV.Length)
		{
			byteCount = 0;
		}
		return result;
	}

	public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (input.Length - inOff < GetBlockSize())
		{
			throw new DataLengthException("Input buffer too short");
		}
		if (output.Length - outOff < GetBlockSize())
		{
			throw new DataLengthException("Output buffer too short");
		}
		ProcessBytes(input, inOff, GetBlockSize(), output, outOff);
		return GetBlockSize();
	}

	public void Reset()
	{
		if (initialised)
		{
			cipher.ProcessBlock(IV, 0, ofbV, 0);
		}
		cipher.Reset();
		byteCount = 0;
	}

	private void incrementCounterAt(int pos)
	{
		int num = pos;
		while (num < ofbV.Length)
		{
			byte[] array;
			byte[] array2 = (array = ofbV);
			int num2 = num++;
			nint num3 = num2;
			byte b;
			array2[num2] = (b = (byte)(array[num3] + 1));
			if (b != 0)
			{
				break;
			}
		}
	}

	private void checkCounter()
	{
	}
}
