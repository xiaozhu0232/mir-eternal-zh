using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto;

public class BufferedBlockCipher : BufferedCipherBase
{
	internal byte[] buf;

	internal int bufOff;

	internal bool forEncryption;

	internal IBlockCipher cipher;

	public override string AlgorithmName => cipher.AlgorithmName;

	protected BufferedBlockCipher()
	{
	}

	public BufferedBlockCipher(IBlockCipher cipher)
	{
		if (cipher == null)
		{
			throw new ArgumentNullException("cipher");
		}
		this.cipher = cipher;
		buf = new byte[cipher.GetBlockSize()];
		bufOff = 0;
	}

	public override void Init(bool forEncryption, ICipherParameters parameters)
	{
		this.forEncryption = forEncryption;
		if (parameters is ParametersWithRandom parametersWithRandom)
		{
			parameters = parametersWithRandom.Parameters;
		}
		Reset();
		cipher.Init(forEncryption, parameters);
	}

	public override int GetBlockSize()
	{
		return cipher.GetBlockSize();
	}

	public override int GetUpdateOutputSize(int length)
	{
		int num = length + bufOff;
		int num2 = num % buf.Length;
		return num - num2;
	}

	public override int GetOutputSize(int length)
	{
		return length + bufOff;
	}

	public override int ProcessByte(byte input, byte[] output, int outOff)
	{
		buf[bufOff++] = input;
		if (bufOff == buf.Length)
		{
			if (outOff + buf.Length > output.Length)
			{
				throw new DataLengthException("output buffer too short");
			}
			bufOff = 0;
			return cipher.ProcessBlock(buf, 0, output, outOff);
		}
		return 0;
	}

	public override byte[] ProcessByte(byte input)
	{
		int updateOutputSize = GetUpdateOutputSize(1);
		byte[] array = ((updateOutputSize > 0) ? new byte[updateOutputSize] : null);
		int num = ProcessByte(input, array, 0);
		if (updateOutputSize > 0 && num < updateOutputSize)
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0, array2, 0, num);
			array = array2;
		}
		return array;
	}

	public override byte[] ProcessBytes(byte[] input, int inOff, int length)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (length < 1)
		{
			return null;
		}
		int updateOutputSize = GetUpdateOutputSize(length);
		byte[] array = ((updateOutputSize > 0) ? new byte[updateOutputSize] : null);
		int num = ProcessBytes(input, inOff, length, array, 0);
		if (updateOutputSize > 0 && num < updateOutputSize)
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0, array2, 0, num);
			array = array2;
		}
		return array;
	}

	public override int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
	{
		if (length < 1)
		{
			if (length < 0)
			{
				throw new ArgumentException("Can't have a negative input length!");
			}
			return 0;
		}
		int blockSize = GetBlockSize();
		int updateOutputSize = GetUpdateOutputSize(length);
		if (updateOutputSize > 0)
		{
			Check.OutputLength(output, outOff, updateOutputSize, "output buffer too short");
		}
		int num = 0;
		int num2 = buf.Length - bufOff;
		if (length > num2)
		{
			Array.Copy(input, inOff, buf, bufOff, num2);
			num += cipher.ProcessBlock(buf, 0, output, outOff);
			bufOff = 0;
			length -= num2;
			inOff += num2;
			while (length > buf.Length)
			{
				num += cipher.ProcessBlock(input, inOff, output, outOff + num);
				length -= blockSize;
				inOff += blockSize;
			}
		}
		Array.Copy(input, inOff, buf, bufOff, length);
		bufOff += length;
		if (bufOff == buf.Length)
		{
			num += cipher.ProcessBlock(buf, 0, output, outOff + num);
			bufOff = 0;
		}
		return num;
	}

	public override byte[] DoFinal()
	{
		byte[] array = BufferedCipherBase.EmptyBuffer;
		int outputSize = GetOutputSize(0);
		if (outputSize > 0)
		{
			array = new byte[outputSize];
			int num = DoFinal(array, 0);
			if (num < array.Length)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, 0, array2, 0, num);
				array = array2;
			}
		}
		else
		{
			Reset();
		}
		return array;
	}

	public override byte[] DoFinal(byte[] input, int inOff, int inLen)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		int outputSize = GetOutputSize(inLen);
		byte[] array = BufferedCipherBase.EmptyBuffer;
		if (outputSize > 0)
		{
			array = new byte[outputSize];
			int num = ((inLen > 0) ? ProcessBytes(input, inOff, inLen, array, 0) : 0);
			num += DoFinal(array, num);
			if (num < array.Length)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, 0, array2, 0, num);
				array = array2;
			}
		}
		else
		{
			Reset();
		}
		return array;
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		try
		{
			if (bufOff != 0)
			{
				Check.DataLength(!cipher.IsPartialBlockOkay, "data not block size aligned");
				Check.OutputLength(output, outOff, bufOff, "output buffer too short for DoFinal()");
				cipher.ProcessBlock(buf, 0, buf, 0);
				Array.Copy(buf, 0, output, outOff, bufOff);
			}
			return bufOff;
		}
		finally
		{
			Reset();
		}
	}

	public override void Reset()
	{
		Array.Clear(buf, 0, buf.Length);
		bufOff = 0;
		cipher.Reset();
	}
}
