using System;
using Org.BouncyCastle.Crypto.Paddings;

namespace Org.BouncyCastle.Crypto.Macs;

public class CfbBlockCipherMac : IMac
{
	private byte[] mac;

	private byte[] Buffer;

	private int bufOff;

	private MacCFBBlockCipher cipher;

	private IBlockCipherPadding padding;

	private int macSize;

	public string AlgorithmName => cipher.AlgorithmName;

	public CfbBlockCipherMac(IBlockCipher cipher)
		: this(cipher, 8, cipher.GetBlockSize() * 8 / 2, null)
	{
	}

	public CfbBlockCipherMac(IBlockCipher cipher, IBlockCipherPadding padding)
		: this(cipher, 8, cipher.GetBlockSize() * 8 / 2, padding)
	{
	}

	public CfbBlockCipherMac(IBlockCipher cipher, int cfbBitSize, int macSizeInBits)
		: this(cipher, cfbBitSize, macSizeInBits, null)
	{
	}

	public CfbBlockCipherMac(IBlockCipher cipher, int cfbBitSize, int macSizeInBits, IBlockCipherPadding padding)
	{
		if (macSizeInBits % 8 != 0)
		{
			throw new ArgumentException("MAC size must be multiple of 8");
		}
		mac = new byte[cipher.GetBlockSize()];
		this.cipher = new MacCFBBlockCipher(cipher, cfbBitSize);
		this.padding = padding;
		macSize = macSizeInBits / 8;
		Buffer = new byte[this.cipher.GetBlockSize()];
		bufOff = 0;
	}

	public void Init(ICipherParameters parameters)
	{
		Reset();
		cipher.Init(forEncryption: true, parameters);
	}

	public int GetMacSize()
	{
		return macSize;
	}

	public void Update(byte input)
	{
		if (bufOff == Buffer.Length)
		{
			cipher.ProcessBlock(Buffer, 0, mac, 0);
			bufOff = 0;
		}
		Buffer[bufOff++] = input;
	}

	public void BlockUpdate(byte[] input, int inOff, int len)
	{
		if (len < 0)
		{
			throw new ArgumentException("Can't have a negative input length!");
		}
		int blockSize = cipher.GetBlockSize();
		int num = 0;
		int num2 = blockSize - bufOff;
		if (len > num2)
		{
			Array.Copy(input, inOff, Buffer, bufOff, num2);
			num += cipher.ProcessBlock(Buffer, 0, mac, 0);
			bufOff = 0;
			len -= num2;
			inOff += num2;
			while (len > blockSize)
			{
				num += cipher.ProcessBlock(input, inOff, mac, 0);
				len -= blockSize;
				inOff += blockSize;
			}
		}
		Array.Copy(input, inOff, Buffer, bufOff, len);
		bufOff += len;
	}

	public int DoFinal(byte[] output, int outOff)
	{
		int blockSize = cipher.GetBlockSize();
		if (padding == null)
		{
			while (bufOff < blockSize)
			{
				Buffer[bufOff++] = 0;
			}
		}
		else
		{
			padding.AddPadding(Buffer, bufOff);
		}
		cipher.ProcessBlock(Buffer, 0, mac, 0);
		cipher.GetMacBlock(mac);
		Array.Copy(mac, 0, output, outOff, macSize);
		Reset();
		return macSize;
	}

	public void Reset()
	{
		Array.Clear(Buffer, 0, Buffer.Length);
		bufOff = 0;
		cipher.Reset();
	}
}
