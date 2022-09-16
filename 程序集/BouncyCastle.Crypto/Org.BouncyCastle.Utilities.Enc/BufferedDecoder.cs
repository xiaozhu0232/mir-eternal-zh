using System;

namespace Org.BouncyCastle.Utilities.Encoders;

public class BufferedDecoder
{
	internal byte[] buffer;

	internal int bufOff;

	internal ITranslator translator;

	public BufferedDecoder(ITranslator translator, int bufferSize)
	{
		this.translator = translator;
		if (bufferSize % translator.GetEncodedBlockSize() != 0)
		{
			throw new ArgumentException("buffer size not multiple of input block size");
		}
		buffer = new byte[bufferSize];
	}

	public int ProcessByte(byte input, byte[] output, int outOff)
	{
		int result = 0;
		buffer[bufOff++] = input;
		if (bufOff == buffer.Length)
		{
			result = translator.Decode(buffer, 0, buffer.Length, output, outOff);
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
		int num2 = buffer.Length - bufOff;
		if (len > num2)
		{
			Array.Copy(input, inOff, buffer, bufOff, num2);
			num += translator.Decode(buffer, 0, buffer.Length, outBytes, outOff);
			bufOff = 0;
			len -= num2;
			inOff += num2;
			outOff += num;
			int num3 = len - len % buffer.Length;
			num += translator.Decode(input, inOff, num3, outBytes, outOff);
			len -= num3;
			inOff += num3;
		}
		if (len != 0)
		{
			Array.Copy(input, inOff, buffer, bufOff, len);
			bufOff += len;
		}
		return num;
	}
}
