using System;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Macs;

public class KMac : IMac, IXof, IDigest
{
	private static readonly byte[] padding = new byte[100];

	private readonly CShakeDigest cshake;

	private readonly int bitLength;

	private readonly int outputLength;

	private byte[] key;

	private bool initialised;

	private bool firstOutput;

	public string AlgorithmName => "KMAC" + cshake.AlgorithmName.Substring(6);

	public KMac(int bitLength, byte[] S)
	{
		cshake = new CShakeDigest(bitLength, Encoding.ASCII.GetBytes("KMAC"), S);
		this.bitLength = bitLength;
		outputLength = bitLength * 2 / 8;
	}

	public void BlockUpdate(byte[] input, int inOff, int len)
	{
		if (!initialised)
		{
			throw new InvalidOperationException("KMAC not initialized");
		}
		cshake.BlockUpdate(input, inOff, len);
	}

	public int DoFinal(byte[] output, int outOff)
	{
		if (firstOutput)
		{
			if (!initialised)
			{
				throw new InvalidOperationException("KMAC not initialized");
			}
			byte[] array = XofUtilities.RightEncode(GetMacSize() * 8);
			cshake.BlockUpdate(array, 0, array.Length);
		}
		int result = cshake.DoFinal(output, outOff, GetMacSize());
		Reset();
		return result;
	}

	public int DoFinal(byte[] output, int outOff, int outLen)
	{
		if (firstOutput)
		{
			if (!initialised)
			{
				throw new InvalidOperationException("KMAC not initialized");
			}
			byte[] array = XofUtilities.RightEncode(outLen * 8);
			cshake.BlockUpdate(array, 0, array.Length);
		}
		int result = cshake.DoFinal(output, outOff, outLen);
		Reset();
		return result;
	}

	public int DoOutput(byte[] output, int outOff, int outLen)
	{
		if (firstOutput)
		{
			if (!initialised)
			{
				throw new InvalidOperationException("KMAC not initialized");
			}
			byte[] array = XofUtilities.RightEncode(0L);
			cshake.BlockUpdate(array, 0, array.Length);
			firstOutput = false;
		}
		return cshake.DoOutput(output, outOff, outLen);
	}

	public int GetByteLength()
	{
		return cshake.GetByteLength();
	}

	public int GetDigestSize()
	{
		return outputLength;
	}

	public int GetMacSize()
	{
		return outputLength;
	}

	public void Init(ICipherParameters parameters)
	{
		KeyParameter keyParameter = (KeyParameter)parameters;
		key = Arrays.Clone(keyParameter.GetKey());
		initialised = true;
		Reset();
	}

	public void Reset()
	{
		cshake.Reset();
		if (key != null)
		{
			if (bitLength == 128)
			{
				bytePad(key, 168);
			}
			else
			{
				bytePad(key, 136);
			}
		}
		firstOutput = true;
	}

	private void bytePad(byte[] X, int w)
	{
		byte[] array = XofUtilities.LeftEncode(w);
		BlockUpdate(array, 0, array.Length);
		byte[] array2 = encode(X);
		BlockUpdate(array2, 0, array2.Length);
		int num = w - (array.Length + array2.Length) % w;
		if (num > 0 && num != w)
		{
			while (num > padding.Length)
			{
				BlockUpdate(padding, 0, padding.Length);
				num -= padding.Length;
			}
			BlockUpdate(padding, 0, num);
		}
	}

	private static byte[] encode(byte[] X)
	{
		return Arrays.Concatenate(XofUtilities.LeftEncode(X.Length * 8), X);
	}

	public void Update(byte input)
	{
		if (!initialised)
		{
			throw new InvalidOperationException("KMAC not initialized");
		}
		cshake.Update(input);
	}
}
