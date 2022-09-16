using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Macs;

public class VmpcMac : IMac
{
	private byte g;

	private byte n = 0;

	private byte[] P = null;

	private byte s = 0;

	private byte[] T;

	private byte[] workingIV;

	private byte[] workingKey;

	private byte x1;

	private byte x2;

	private byte x3;

	private byte x4;

	public virtual string AlgorithmName => "VMPC-MAC";

	public virtual int DoFinal(byte[] output, int outOff)
	{
		for (int i = 1; i < 25; i++)
		{
			s = P[(s + P[n & 0xFF]) & 0xFF];
			x4 = P[(x4 + x3 + i) & 0xFF];
			x3 = P[(x3 + x2 + i) & 0xFF];
			x2 = P[(x2 + x1 + i) & 0xFF];
			x1 = P[(x1 + s + i) & 0xFF];
			T[g & 0x1F] = (byte)(T[g & 0x1F] ^ x1);
			T[(g + 1) & 0x1F] = (byte)(T[(g + 1) & 0x1F] ^ x2);
			T[(g + 2) & 0x1F] = (byte)(T[(g + 2) & 0x1F] ^ x3);
			T[(g + 3) & 0x1F] = (byte)(T[(g + 3) & 0x1F] ^ x4);
			g = (byte)((uint)(g + 4) & 0x1Fu);
			byte b = P[n & 0xFF];
			P[n & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b;
			n = (byte)((uint)(n + 1) & 0xFFu);
		}
		for (int j = 0; j < 768; j++)
		{
			s = P[(s + P[j & 0xFF] + T[j & 0x1F]) & 0xFF];
			byte b2 = P[j & 0xFF];
			P[j & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b2;
		}
		byte[] array = new byte[20];
		for (int k = 0; k < 20; k++)
		{
			s = P[(s + P[k & 0xFF]) & 0xFF];
			array[k] = P[(P[P[s & 0xFF] & 0xFF] + 1) & 0xFF];
			byte b3 = P[k & 0xFF];
			P[k & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b3;
		}
		Array.Copy(array, 0, output, outOff, array.Length);
		Reset();
		return array.Length;
	}

	public virtual int GetMacSize()
	{
		return 20;
	}

	public virtual void Init(ICipherParameters parameters)
	{
		if (!(parameters is ParametersWithIV))
		{
			throw new ArgumentException("VMPC-MAC Init parameters must include an IV", "parameters");
		}
		ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
		KeyParameter keyParameter = (KeyParameter)parametersWithIV.Parameters;
		if (!(parametersWithIV.Parameters is KeyParameter))
		{
			throw new ArgumentException("VMPC-MAC Init parameters must include a key", "parameters");
		}
		workingIV = parametersWithIV.GetIV();
		if (workingIV == null || workingIV.Length < 1 || workingIV.Length > 768)
		{
			throw new ArgumentException("VMPC-MAC requires 1 to 768 bytes of IV", "parameters");
		}
		workingKey = keyParameter.GetKey();
		Reset();
	}

	private void initKey(byte[] keyBytes, byte[] ivBytes)
	{
		s = 0;
		P = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			P[i] = (byte)i;
		}
		for (int j = 0; j < 768; j++)
		{
			s = P[(s + P[j & 0xFF] + keyBytes[j % keyBytes.Length]) & 0xFF];
			byte b = P[j & 0xFF];
			P[j & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b;
		}
		for (int k = 0; k < 768; k++)
		{
			s = P[(s + P[k & 0xFF] + ivBytes[k % ivBytes.Length]) & 0xFF];
			byte b2 = P[k & 0xFF];
			P[k & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b2;
		}
		n = 0;
	}

	public virtual void Reset()
	{
		initKey(workingKey, workingIV);
		g = (x1 = (x2 = (x3 = (x4 = (n = 0)))));
		T = new byte[32];
		for (int i = 0; i < 32; i++)
		{
			T[i] = 0;
		}
	}

	public virtual void Update(byte input)
	{
		s = P[(s + P[n & 0xFF]) & 0xFF];
		byte b = (byte)(input ^ P[(P[P[s & 0xFF] & 0xFF] + 1) & 0xFF]);
		x4 = P[(x4 + x3) & 0xFF];
		x3 = P[(x3 + x2) & 0xFF];
		x2 = P[(x2 + x1) & 0xFF];
		x1 = P[(x1 + s + b) & 0xFF];
		T[g & 0x1F] = (byte)(T[g & 0x1F] ^ x1);
		T[(g + 1) & 0x1F] = (byte)(T[(g + 1) & 0x1F] ^ x2);
		T[(g + 2) & 0x1F] = (byte)(T[(g + 2) & 0x1F] ^ x3);
		T[(g + 3) & 0x1F] = (byte)(T[(g + 3) & 0x1F] ^ x4);
		g = (byte)((uint)(g + 4) & 0x1Fu);
		byte b2 = P[n & 0xFF];
		P[n & 0xFF] = P[s & 0xFF];
		P[s & 0xFF] = b2;
		n = (byte)((uint)(n + 1) & 0xFFu);
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int len)
	{
		if (inOff + len > input.Length)
		{
			throw new DataLengthException("input buffer too short");
		}
		for (int i = 0; i < len; i++)
		{
			Update(input[inOff + i]);
		}
	}
}
