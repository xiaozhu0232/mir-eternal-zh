using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Engines;

public class VmpcEngine : IStreamCipher
{
	protected byte n = 0;

	protected byte[] P = null;

	protected byte s = 0;

	protected byte[] workingIV;

	protected byte[] workingKey;

	public virtual string AlgorithmName => "VMPC";

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is ParametersWithIV))
		{
			throw new ArgumentException("VMPC Init parameters must include an IV");
		}
		ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
		if (!(parametersWithIV.Parameters is KeyParameter))
		{
			throw new ArgumentException("VMPC Init parameters must include a key");
		}
		KeyParameter keyParameter = (KeyParameter)parametersWithIV.Parameters;
		workingIV = parametersWithIV.GetIV();
		if (workingIV == null || workingIV.Length < 1 || workingIV.Length > 768)
		{
			throw new ArgumentException("VMPC requires 1 to 768 bytes of IV");
		}
		workingKey = keyParameter.GetKey();
		InitKey(workingKey, workingIV);
	}

	protected virtual void InitKey(byte[] keyBytes, byte[] ivBytes)
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

	public virtual void ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
	{
		Check.DataLength(input, inOff, len, "input buffer too short");
		Check.OutputLength(output, outOff, len, "output buffer too short");
		for (int i = 0; i < len; i++)
		{
			s = P[(s + P[n & 0xFF]) & 0xFF];
			byte b = P[(P[P[s & 0xFF] & 0xFF] + 1) & 0xFF];
			byte b2 = P[n & 0xFF];
			P[n & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b2;
			n = (byte)((uint)(n + 1) & 0xFFu);
			output[i + outOff] = (byte)(input[i + inOff] ^ b);
		}
	}

	public virtual void Reset()
	{
		InitKey(workingKey, workingIV);
	}

	public virtual byte ReturnByte(byte input)
	{
		s = P[(s + P[n & 0xFF]) & 0xFF];
		byte b = P[(P[P[s & 0xFF] & 0xFF] + 1) & 0xFF];
		byte b2 = P[n & 0xFF];
		P[n & 0xFF] = P[s & 0xFF];
		P[s & 0xFF] = b2;
		n = (byte)((uint)(n + 1) & 0xFFu);
		return (byte)(input ^ b);
	}
}
