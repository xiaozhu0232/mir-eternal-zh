using System;

namespace Org.BouncyCastle.Utilities.Encoders;

public class BufferedEncoder
{
	internal byte[] Buffer;

	internal int bufOff;

	internal ITranslator translator;

	public BufferedEncoder(ITranslator translator, int bufferSize)
	{
		this.translator = translator;
		if (bufferSize % translator.GetEncodedBlockSize() != 0)
		{
			throw new ArgumentException("buffer size not multiple of input block size");
		}
		Buffer = new byte[bufferSize];
	}

	public int ProcessByte(byte input, byte[] outBytes, int outOff)
	{
		int result = 0;
		Buffer[bufOff++] = input;
		if (bufOff == Buffer.Length)
		{
			result = translator.Encode(Buffer, 0, Buffer.Length, outBytes, outOff);
			bufOff = 0;
		}
		return result;
	}

	public int ProcessBytes(byte[] input, int inOff, int len, byte[] outBytes, int outOff)
	{
		if (len < 0)
		{
			throw new ArgumentException("Can't have a negative input length!");
		}
		int num = 0;
		int num2 = Buffer.Length - bufOff;
		if (len > num2)
		{
			Array.Copy(input, inOff, Buffer, bufOff, num2);
			num += translator.Encode(Buffer, 0, Buffer.Length, outBytes, outOff);
			bufOff = 0;
			len -= num2;
			inOff += num2;
			outOff += num;
			int num3 = len - len % Buffer.Length;
			num += translator.Encode(input, inOff, num3, outBytes, outOff);
			len -= num3;
			inOff += num3;
		}
		if (len != 0)
		{
			Array.Copy(input, inOff, Buffer, bufOff, len);
			bufOff += len;
		}
		return num;
	}
}
