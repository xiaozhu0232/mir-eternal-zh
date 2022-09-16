using System;

namespace Org.BouncyCastle.Crypto;

public class StreamBlockCipher : IStreamCipher
{
	private readonly IBlockCipher cipher;

	private readonly byte[] oneByte = new byte[1];

	public string AlgorithmName => cipher.AlgorithmName;

	public StreamBlockCipher(IBlockCipher cipher)
	{
		if (cipher == null)
		{
			throw new ArgumentNullException("cipher");
		}
		if (cipher.GetBlockSize() != 1)
		{
			throw new ArgumentException("block cipher block size != 1.", "cipher");
		}
		this.cipher = cipher;
	}

	public void Init(bool forEncryption, ICipherParameters parameters)
	{
		cipher.Init(forEncryption, parameters);
	}

	public byte ReturnByte(byte input)
	{
		oneByte[0] = input;
		cipher.ProcessBlock(oneByte, 0, oneByte, 0);
		return oneByte[0];
	}

	public void ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
	{
		if (outOff + length > output.Length)
		{
			throw new DataLengthException("output buffer too small in ProcessBytes()");
		}
		for (int i = 0; i != length; i++)
		{
			cipher.ProcessBlock(input, inOff + i, output, outOff + i);
		}
	}

	public void Reset()
	{
		cipher.Reset();
	}
}
