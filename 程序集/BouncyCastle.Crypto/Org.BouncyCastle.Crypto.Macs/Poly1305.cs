using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Macs;

public class Poly1305 : IMac
{
	private const int BlockSize = 16;

	private readonly IBlockCipher cipher;

	private readonly byte[] singleByte = new byte[1];

	private uint r0;

	private uint r1;

	private uint r2;

	private uint r3;

	private uint r4;

	private uint s1;

	private uint s2;

	private uint s3;

	private uint s4;

	private uint k0;

	private uint k1;

	private uint k2;

	private uint k3;

	private byte[] currentBlock = new byte[16];

	private int currentBlockOffset = 0;

	private uint h0;

	private uint h1;

	private uint h2;

	private uint h3;

	private uint h4;

	public string AlgorithmName
	{
		get
		{
			if (cipher != null)
			{
				return "Poly1305-" + cipher.AlgorithmName;
			}
			return "Poly1305";
		}
	}

	public Poly1305()
	{
		cipher = null;
	}

	public Poly1305(IBlockCipher cipher)
	{
		if (cipher.GetBlockSize() != 16)
		{
			throw new ArgumentException("Poly1305 requires a 128 bit block cipher.");
		}
		this.cipher = cipher;
	}

	public void Init(ICipherParameters parameters)
	{
		byte[] nonce = null;
		if (cipher != null)
		{
			if (!(parameters is ParametersWithIV))
			{
				throw new ArgumentException("Poly1305 requires an IV when used with a block cipher.", "parameters");
			}
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			nonce = parametersWithIV.GetIV();
			parameters = parametersWithIV.Parameters;
		}
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("Poly1305 requires a key.");
		}
		KeyParameter keyParameter = (KeyParameter)parameters;
		SetKey(keyParameter.GetKey(), nonce);
		Reset();
	}

	private void SetKey(byte[] key, byte[] nonce)
	{
		if (key.Length != 32)
		{
			throw new ArgumentException("Poly1305 key must be 256 bits.");
		}
		if (cipher != null && (nonce == null || nonce.Length != 16))
		{
			throw new ArgumentException("Poly1305 requires a 128 bit IV.");
		}
		uint num = Pack.LE_To_UInt32(key, 0);
		uint num2 = Pack.LE_To_UInt32(key, 4);
		uint num3 = Pack.LE_To_UInt32(key, 8);
		uint num4 = Pack.LE_To_UInt32(key, 12);
		r0 = num & 0x3FFFFFFu;
		r1 = ((num >> 26) | (num2 << 6)) & 0x3FFFF03u;
		r2 = ((num2 >> 20) | (num3 << 12)) & 0x3FFC0FFu;
		r3 = ((num3 >> 14) | (num4 << 18)) & 0x3F03FFFu;
		r4 = (num4 >> 8) & 0xFFFFFu;
		s1 = r1 * 5;
		s2 = r2 * 5;
		s3 = r3 * 5;
		s4 = r4 * 5;
		byte[] array;
		int num5;
		if (cipher == null)
		{
			array = key;
			num5 = 16;
		}
		else
		{
			array = new byte[16];
			num5 = 0;
			cipher.Init(forEncryption: true, new KeyParameter(key, 16, 16));
			cipher.ProcessBlock(nonce, 0, array, 0);
		}
		k0 = Pack.LE_To_UInt32(array, num5);
		k1 = Pack.LE_To_UInt32(array, num5 + 4);
		k2 = Pack.LE_To_UInt32(array, num5 + 8);
		k3 = Pack.LE_To_UInt32(array, num5 + 12);
	}

	public int GetMacSize()
	{
		return 16;
	}

	public void Update(byte input)
	{
		singleByte[0] = input;
		BlockUpdate(singleByte, 0, 1);
	}

	public void BlockUpdate(byte[] input, int inOff, int len)
	{
		int num = 0;
		while (len > num)
		{
			if (currentBlockOffset == 16)
			{
				ProcessBlock();
				currentBlockOffset = 0;
			}
			int num2 = System.Math.Min(len - num, 16 - currentBlockOffset);
			Array.Copy(input, num + inOff, currentBlock, currentBlockOffset, num2);
			num += num2;
			currentBlockOffset += num2;
		}
	}

	private void ProcessBlock()
	{
		if (currentBlockOffset < 16)
		{
			currentBlock[currentBlockOffset] = 1;
			for (int i = currentBlockOffset + 1; i < 16; i++)
			{
				currentBlock[i] = 0;
			}
		}
		ulong num = Pack.LE_To_UInt32(currentBlock, 0);
		ulong num2 = Pack.LE_To_UInt32(currentBlock, 4);
		ulong num3 = Pack.LE_To_UInt32(currentBlock, 8);
		ulong num4 = Pack.LE_To_UInt32(currentBlock, 12);
		h0 += (uint)(int)(num & 0x3FFFFFF);
		h1 += (uint)(int)((((num2 << 32) | num) >> 26) & 0x3FFFFFF);
		h2 += (uint)(int)((((num3 << 32) | num2) >> 20) & 0x3FFFFFF);
		h3 += (uint)(int)((((num4 << 32) | num3) >> 14) & 0x3FFFFFF);
		h4 += (uint)(int)(num4 >> 8);
		if (currentBlockOffset == 16)
		{
			h4 += 16777216u;
		}
		ulong num5 = mul32x32_64(h0, r0) + mul32x32_64(h1, s4) + mul32x32_64(h2, s3) + mul32x32_64(h3, s2) + mul32x32_64(h4, s1);
		ulong num6 = mul32x32_64(h0, r1) + mul32x32_64(h1, r0) + mul32x32_64(h2, s4) + mul32x32_64(h3, s3) + mul32x32_64(h4, s2);
		ulong num7 = mul32x32_64(h0, r2) + mul32x32_64(h1, r1) + mul32x32_64(h2, r0) + mul32x32_64(h3, s4) + mul32x32_64(h4, s3);
		ulong num8 = mul32x32_64(h0, r3) + mul32x32_64(h1, r2) + mul32x32_64(h2, r1) + mul32x32_64(h3, r0) + mul32x32_64(h4, s4);
		ulong num9 = mul32x32_64(h0, r4) + mul32x32_64(h1, r3) + mul32x32_64(h2, r2) + mul32x32_64(h3, r1) + mul32x32_64(h4, r0);
		h0 = (uint)(int)num5 & 0x3FFFFFFu;
		num6 += num5 >> 26;
		h1 = (uint)(int)num6 & 0x3FFFFFFu;
		num7 += num6 >> 26;
		h2 = (uint)(int)num7 & 0x3FFFFFFu;
		num8 += num7 >> 26;
		h3 = (uint)(int)num8 & 0x3FFFFFFu;
		num9 += num8 >> 26;
		h4 = (uint)(int)num9 & 0x3FFFFFFu;
		h0 += (uint)((int)(num9 >> 26) * 5);
		h1 += h0 >> 26;
		h0 &= 67108863u;
	}

	public int DoFinal(byte[] output, int outOff)
	{
		Check.DataLength(output, outOff, 16, "Output buffer is too short.");
		if (currentBlockOffset > 0)
		{
			ProcessBlock();
		}
		h1 += h0 >> 26;
		h0 &= 67108863u;
		h2 += h1 >> 26;
		h1 &= 67108863u;
		h3 += h2 >> 26;
		h2 &= 67108863u;
		h4 += h3 >> 26;
		h3 &= 67108863u;
		h0 += (h4 >> 26) * 5;
		h4 &= 67108863u;
		h1 += h0 >> 26;
		h0 &= 67108863u;
		uint num = h0 + 5;
		uint num2 = num >> 26;
		num &= 0x3FFFFFFu;
		uint num3 = h1 + num2;
		num2 = num3 >> 26;
		num3 &= 0x3FFFFFFu;
		uint num4 = h2 + num2;
		num2 = num4 >> 26;
		num4 &= 0x3FFFFFFu;
		uint num5 = h3 + num2;
		num2 = num5 >> 26;
		num5 &= 0x3FFFFFFu;
		uint num6 = h4 + num2 - 67108864;
		num2 = (num6 >> 31) - 1;
		uint num7 = ~num2;
		h0 = (h0 & num7) | (num & num2);
		h1 = (h1 & num7) | (num3 & num2);
		h2 = (h2 & num7) | (num4 & num2);
		h3 = (h3 & num7) | (num5 & num2);
		h4 = (h4 & num7) | (num6 & num2);
		ulong num8 = (ulong)(h0 | (h1 << 26)) + (ulong)k0;
		ulong num9 = (ulong)((h1 >> 6) | (h2 << 20)) + (ulong)k1;
		ulong num10 = (ulong)((h2 >> 12) | (h3 << 14)) + (ulong)k2;
		ulong num11 = (ulong)((h3 >> 18) | (h4 << 8)) + (ulong)k3;
		Pack.UInt32_To_LE((uint)num8, output, outOff);
		num9 += num8 >> 32;
		Pack.UInt32_To_LE((uint)num9, output, outOff + 4);
		num10 += num9 >> 32;
		Pack.UInt32_To_LE((uint)num10, output, outOff + 8);
		num11 += num10 >> 32;
		Pack.UInt32_To_LE((uint)num11, output, outOff + 12);
		Reset();
		return 16;
	}

	public void Reset()
	{
		currentBlockOffset = 0;
		h0 = (h1 = (h2 = (h3 = (h4 = 0u))));
	}

	private static ulong mul32x32_64(uint i1, uint i2)
	{
		return (ulong)i1 * (ulong)i2;
	}
}
