using System;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Macs;

public class Dstu7624Mac : IMac
{
	private int macSize;

	private Dstu7624Engine engine;

	private int blockSize;

	private byte[] c;

	private byte[] cTemp;

	private byte[] kDelta;

	private byte[] buf;

	private int bufOff;

	public string AlgorithmName => "Dstu7624Mac";

	public Dstu7624Mac(int blockSizeBits, int q)
	{
		engine = new Dstu7624Engine(blockSizeBits);
		blockSize = blockSizeBits / 8;
		macSize = q / 8;
		c = new byte[blockSize];
		cTemp = new byte[blockSize];
		kDelta = new byte[blockSize];
		buf = new byte[blockSize];
	}

	public void Init(ICipherParameters parameters)
	{
		if (parameters is KeyParameter)
		{
			engine.Init(forEncryption: true, (KeyParameter)parameters);
			engine.ProcessBlock(kDelta, 0, kDelta, 0);
			return;
		}
		throw new ArgumentException("invalid parameter passed to Dstu7624Mac init - " + Platform.GetTypeName(parameters));
	}

	public int GetMacSize()
	{
		return macSize;
	}

	public void Update(byte input)
	{
		if (bufOff == buf.Length)
		{
			processBlock(buf, 0);
			bufOff = 0;
		}
		buf[bufOff++] = input;
	}

	public void BlockUpdate(byte[] input, int inOff, int len)
	{
		if (len < 0)
		{
			throw new ArgumentException("Can't have a negative input length!");
		}
		int num = engine.GetBlockSize();
		int num2 = num - bufOff;
		if (len > num2)
		{
			Array.Copy(input, inOff, buf, bufOff, num2);
			processBlock(buf, 0);
			bufOff = 0;
			len -= num2;
			inOff += num2;
			while (len > num)
			{
				processBlock(input, inOff);
				len -= num;
				inOff += num;
			}
		}
		Array.Copy(input, inOff, buf, bufOff, len);
		bufOff += len;
	}

	private void processBlock(byte[] input, int inOff)
	{
		Xor(c, 0, input, inOff, cTemp);
		engine.ProcessBlock(cTemp, 0, c, 0);
	}

	private void Xor(byte[] c, int cOff, byte[] input, int inOff, byte[] xorResult)
	{
		for (int i = 0; i < blockSize; i++)
		{
			xorResult[i] = (byte)(c[i + cOff] ^ input[i + inOff]);
		}
	}

	public int DoFinal(byte[] output, int outOff)
	{
		if (bufOff % buf.Length != 0)
		{
			throw new DataLengthException("Input must be a multiple of blocksize");
		}
		Xor(c, 0, buf, 0, cTemp);
		Xor(cTemp, 0, kDelta, 0, c);
		engine.ProcessBlock(c, 0, c, 0);
		if (macSize + outOff > output.Length)
		{
			throw new DataLengthException("Output buffer too short");
		}
		Array.Copy(c, 0, output, outOff, macSize);
		return macSize;
	}

	public void Reset()
	{
		Arrays.Fill(c, 0);
		Arrays.Fill(cTemp, 0);
		Arrays.Fill(kDelta, 0);
		Arrays.Fill(buf, 0);
		engine.Reset();
		engine.ProcessBlock(kDelta, 0, kDelta, 0);
		bufOff = 0;
	}
}
