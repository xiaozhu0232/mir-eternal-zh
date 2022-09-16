using System;

namespace Org.BouncyCastle.Crypto.Modes;

public class CtsBlockCipher : BufferedBlockCipher
{
	private readonly int blockSize;

	public CtsBlockCipher(IBlockCipher cipher)
	{
		if (cipher is OfbBlockCipher || cipher is CfbBlockCipher)
		{
			throw new ArgumentException("CtsBlockCipher can only accept ECB, or CBC ciphers");
		}
		base.cipher = cipher;
		blockSize = cipher.GetBlockSize();
		buf = new byte[blockSize * 2];
		bufOff = 0;
	}

	public override int GetUpdateOutputSize(int length)
	{
		int num = length + bufOff;
		int num2 = num % buf.Length;
		if (num2 == 0)
		{
			return num - buf.Length;
		}
		return num - num2;
	}

	public override int GetOutputSize(int length)
	{
		return length + bufOff;
	}

	public override int ProcessByte(byte input, byte[] output, int outOff)
	{
		int result = 0;
		if (bufOff == buf.Length)
		{
			result = cipher.ProcessBlock(buf, 0, output, outOff);
			Array.Copy(buf, blockSize, buf, 0, blockSize);
			bufOff = blockSize;
		}
		buf[bufOff++] = input;
		return result;
	}

	public override int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
	{
		if (length < 0)
		{
			throw new ArgumentException("Can't have a negative input outLength!");
		}
		int num = GetBlockSize();
		int updateOutputSize = GetUpdateOutputSize(length);
		if (updateOutputSize > 0 && outOff + updateOutputSize > output.Length)
		{
			throw new DataLengthException("output buffer too short");
		}
		int num2 = 0;
		int num3 = buf.Length - bufOff;
		if (length > num3)
		{
			Array.Copy(input, inOff, buf, bufOff, num3);
			num2 += cipher.ProcessBlock(buf, 0, output, outOff);
			Array.Copy(buf, num, buf, 0, num);
			bufOff = num;
			length -= num3;
			inOff += num3;
			while (length > num)
			{
				Array.Copy(input, inOff, buf, bufOff, num);
				num2 += cipher.ProcessBlock(buf, 0, output, outOff + num2);
				Array.Copy(buf, num, buf, 0, num);
				length -= num;
				inOff += num;
			}
		}
		Array.Copy(input, inOff, buf, bufOff, length);
		bufOff += length;
		return num2;
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		if (bufOff + outOff > output.Length)
		{
			throw new DataLengthException("output buffer too small in doFinal");
		}
		int num = cipher.GetBlockSize();
		int length = bufOff - num;
		byte[] array = new byte[num];
		if (forEncryption)
		{
			cipher.ProcessBlock(buf, 0, array, 0);
			if (bufOff < num)
			{
				throw new DataLengthException("need at least one block of input for CTS");
			}
			for (int i = bufOff; i != buf.Length; i++)
			{
				buf[i] = array[i - num];
			}
			for (int j = num; j != bufOff; j++)
			{
				byte[] array2;
				byte[] array3 = (array2 = buf);
				int num2 = j;
				nint num3 = num2;
				array3[num2] = (byte)(array2[num3] ^ array[j - num]);
			}
			IBlockCipher blockCipher = ((cipher is CbcBlockCipher) ? ((CbcBlockCipher)cipher).GetUnderlyingCipher() : cipher);
			blockCipher.ProcessBlock(buf, num, output, outOff);
			Array.Copy(array, 0, output, outOff + num, length);
		}
		else
		{
			byte[] array4 = new byte[num];
			IBlockCipher blockCipher2 = ((cipher is CbcBlockCipher) ? ((CbcBlockCipher)cipher).GetUnderlyingCipher() : cipher);
			blockCipher2.ProcessBlock(buf, 0, array, 0);
			for (int k = num; k != bufOff; k++)
			{
				array4[k - num] = (byte)(array[k - num] ^ buf[k]);
			}
			Array.Copy(buf, num, array, 0, length);
			cipher.ProcessBlock(array, 0, output, outOff);
			Array.Copy(array4, 0, output, outOff + num, length);
		}
		int result = bufOff;
		Reset();
		return result;
	}
}
