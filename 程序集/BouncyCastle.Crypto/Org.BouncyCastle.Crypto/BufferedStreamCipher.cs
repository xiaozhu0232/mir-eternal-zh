using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto;

public class BufferedStreamCipher : BufferedCipherBase
{
	private readonly IStreamCipher cipher;

	public override string AlgorithmName => cipher.AlgorithmName;

	public BufferedStreamCipher(IStreamCipher cipher)
	{
		if (cipher == null)
		{
			throw new ArgumentNullException("cipher");
		}
		this.cipher = cipher;
	}

	public override void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (parameters is ParametersWithRandom)
		{
			parameters = ((ParametersWithRandom)parameters).Parameters;
		}
		cipher.Init(forEncryption, parameters);
	}

	public override int GetBlockSize()
	{
		return 0;
	}

	public override int GetOutputSize(int inputLen)
	{
		return inputLen;
	}

	public override int GetUpdateOutputSize(int inputLen)
	{
		return inputLen;
	}

	public override byte[] ProcessByte(byte input)
	{
		return new byte[1] { cipher.ReturnByte(input) };
	}

	public override int ProcessByte(byte input, byte[] output, int outOff)
	{
		if (outOff >= output.Length)
		{
			throw new DataLengthException("output buffer too short");
		}
		output[outOff] = cipher.ReturnByte(input);
		return 1;
	}

	public override byte[] ProcessBytes(byte[] input, int inOff, int length)
	{
		if (length < 1)
		{
			return null;
		}
		byte[] array = new byte[length];
		cipher.ProcessBytes(input, inOff, length, array, 0);
		return array;
	}

	public override int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
	{
		if (length < 1)
		{
			return 0;
		}
		if (length > 0)
		{
			cipher.ProcessBytes(input, inOff, length, output, outOff);
		}
		return length;
	}

	public override byte[] DoFinal()
	{
		Reset();
		return BufferedCipherBase.EmptyBuffer;
	}

	public override byte[] DoFinal(byte[] input, int inOff, int length)
	{
		if (length < 1)
		{
			return BufferedCipherBase.EmptyBuffer;
		}
		byte[] result = ProcessBytes(input, inOff, length);
		Reset();
		return result;
	}

	public override void Reset()
	{
		cipher.Reset();
	}
}
