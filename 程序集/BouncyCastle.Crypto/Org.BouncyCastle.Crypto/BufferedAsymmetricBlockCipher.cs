using System;

namespace Org.BouncyCastle.Crypto;

public class BufferedAsymmetricBlockCipher : BufferedCipherBase
{
	private readonly IAsymmetricBlockCipher cipher;

	private byte[] buffer;

	private int bufOff;

	public override string AlgorithmName => cipher.AlgorithmName;

	public BufferedAsymmetricBlockCipher(IAsymmetricBlockCipher cipher)
	{
		this.cipher = cipher;
	}

	internal int GetBufferPosition()
	{
		return bufOff;
	}

	public override int GetBlockSize()
	{
		return cipher.GetInputBlockSize();
	}

	public override int GetOutputSize(int length)
	{
		return cipher.GetOutputBlockSize();
	}

	public override int GetUpdateOutputSize(int length)
	{
		return 0;
	}

	public override void Init(bool forEncryption, ICipherParameters parameters)
	{
		Reset();
		cipher.Init(forEncryption, parameters);
		buffer = new byte[cipher.GetInputBlockSize() + (forEncryption ? 1 : 0)];
		bufOff = 0;
	}

	public override byte[] ProcessByte(byte input)
	{
		if (bufOff >= buffer.Length)
		{
			throw new DataLengthException("attempt to process message to long for cipher");
		}
		buffer[bufOff++] = input;
		return null;
	}

	public override byte[] ProcessBytes(byte[] input, int inOff, int length)
	{
		if (length < 1)
		{
			return null;
		}
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (bufOff + length > buffer.Length)
		{
			throw new DataLengthException("attempt to process message to long for cipher");
		}
		Array.Copy(input, inOff, buffer, bufOff, length);
		bufOff += length;
		return null;
	}

	public override byte[] DoFinal()
	{
		byte[] result = ((bufOff > 0) ? cipher.ProcessBlock(buffer, 0, bufOff) : BufferedCipherBase.EmptyBuffer);
		Reset();
		return result;
	}

	public override byte[] DoFinal(byte[] input, int inOff, int length)
	{
		ProcessBytes(input, inOff, length);
		return DoFinal();
	}

	public override void Reset()
	{
		if (buffer != null)
		{
			Array.Clear(buffer, 0, buffer.Length);
			bufOff = 0;
		}
	}
}
