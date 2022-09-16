using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Macs;

public class Dstu7564Mac : IMac
{
	private Dstu7564Digest engine;

	private int macSize;

	private ulong inputLength;

	private byte[] paddedKey;

	private byte[] invertedKey;

	public string AlgorithmName => "DSTU7564Mac";

	public Dstu7564Mac(int macSizeBits)
	{
		engine = new Dstu7564Digest(macSizeBits);
		macSize = macSizeBits / 8;
	}

	public void Init(ICipherParameters parameters)
	{
		if (parameters is KeyParameter)
		{
			byte[] key = ((KeyParameter)parameters).GetKey();
			invertedKey = new byte[key.Length];
			paddedKey = PadKey(key);
			for (int i = 0; i < invertedKey.Length; i++)
			{
				invertedKey[i] = (byte)(key[i] ^ 0xFFu);
			}
			engine.BlockUpdate(paddedKey, 0, paddedKey.Length);
			return;
		}
		throw new ArgumentException("Bad parameter passed");
	}

	public int GetMacSize()
	{
		return macSize;
	}

	public void BlockUpdate(byte[] input, int inOff, int len)
	{
		Check.DataLength(input, inOff, len, "Input buffer too short");
		if (paddedKey == null)
		{
			throw new InvalidOperationException(AlgorithmName + " not initialised");
		}
		engine.BlockUpdate(input, inOff, len);
		inputLength += (ulong)len;
	}

	public void Update(byte input)
	{
		engine.Update(input);
		inputLength++;
	}

	public int DoFinal(byte[] output, int outOff)
	{
		Check.OutputLength(output, outOff, macSize, "Output buffer too short");
		if (paddedKey == null)
		{
			throw new InvalidOperationException(AlgorithmName + " not initialised");
		}
		Pad();
		engine.BlockUpdate(invertedKey, 0, invertedKey.Length);
		inputLength = 0uL;
		return engine.DoFinal(output, outOff);
	}

	public void Reset()
	{
		inputLength = 0uL;
		engine.Reset();
		if (paddedKey != null)
		{
			engine.BlockUpdate(paddedKey, 0, paddedKey.Length);
		}
	}

	private void Pad()
	{
		int num = engine.GetByteLength() - (int)(inputLength % (ulong)engine.GetByteLength());
		if (num < 13)
		{
			num += engine.GetByteLength();
		}
		byte[] array = new byte[num];
		array[0] = 128;
		Pack.UInt64_To_LE(inputLength * 8, array, array.Length - 12);
		engine.BlockUpdate(array, 0, array.Length);
	}

	private byte[] PadKey(byte[] input)
	{
		int num = (input.Length + engine.GetByteLength() - 1) / engine.GetByteLength() * engine.GetByteLength();
		int num2 = engine.GetByteLength() - input.Length % engine.GetByteLength();
		if (num2 < 13)
		{
			num += engine.GetByteLength();
		}
		byte[] array = new byte[num];
		Array.Copy(input, 0, array, 0, input.Length);
		array[input.Length] = 128;
		Pack.UInt32_To_LE((uint)(input.Length * 8), array, array.Length - 12);
		return array;
	}
}
