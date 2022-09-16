using System;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Macs;

public class CMac : IMac
{
	private const byte CONSTANT_128 = 135;

	private const byte CONSTANT_64 = 27;

	private byte[] ZEROES;

	private byte[] mac;

	private byte[] buf;

	private int bufOff;

	private IBlockCipher cipher;

	private int macSize;

	private byte[] L;

	private byte[] Lu;

	private byte[] Lu2;

	public string AlgorithmName => cipher.AlgorithmName;

	public CMac(IBlockCipher cipher)
		: this(cipher, cipher.GetBlockSize() * 8)
	{
	}

	public CMac(IBlockCipher cipher, int macSizeInBits)
	{
		if (macSizeInBits % 8 != 0)
		{
			throw new ArgumentException("MAC size must be multiple of 8");
		}
		if (macSizeInBits > cipher.GetBlockSize() * 8)
		{
			throw new ArgumentException("MAC size must be less or equal to " + cipher.GetBlockSize() * 8);
		}
		if (cipher.GetBlockSize() != 8 && cipher.GetBlockSize() != 16)
		{
			throw new ArgumentException("Block size must be either 64 or 128 bits");
		}
		this.cipher = new CbcBlockCipher(cipher);
		macSize = macSizeInBits / 8;
		mac = new byte[cipher.GetBlockSize()];
		buf = new byte[cipher.GetBlockSize()];
		ZEROES = new byte[cipher.GetBlockSize()];
		bufOff = 0;
	}

	private static int ShiftLeft(byte[] block, byte[] output)
	{
		int num = block.Length;
		uint num2 = 0u;
		while (--num >= 0)
		{
			uint num3 = block[num];
			output[num] = (byte)((num3 << 1) | num2);
			num2 = (num3 >> 7) & 1u;
		}
		return (int)num2;
	}

	private static byte[] DoubleLu(byte[] input)
	{
		byte[] array = new byte[input.Length];
		int num = ShiftLeft(input, array);
		int num2 = ((input.Length == 16) ? 135 : 27);
		byte[] array2;
		byte[] array3 = (array2 = array);
		int num3 = input.Length - 1;
		nint num4 = num3;
		array3[num3] = (byte)(array2[num4] ^ (byte)(num2 >> (1 - num << 3)));
		return array;
	}

	public void Init(ICipherParameters parameters)
	{
		if (parameters is KeyParameter)
		{
			cipher.Init(forEncryption: true, parameters);
			L = new byte[ZEROES.Length];
			cipher.ProcessBlock(ZEROES, 0, L, 0);
			Lu = DoubleLu(L);
			Lu2 = DoubleLu(Lu);
		}
		else if (parameters != null)
		{
			throw new ArgumentException("CMac mode only permits key to be set.", "parameters");
		}
		Reset();
	}

	public int GetMacSize()
	{
		return macSize;
	}

	public void Update(byte input)
	{
		if (bufOff == buf.Length)
		{
			cipher.ProcessBlock(buf, 0, mac, 0);
			bufOff = 0;
		}
		buf[bufOff++] = input;
	}

	public void BlockUpdate(byte[] inBytes, int inOff, int len)
	{
		if (len < 0)
		{
			throw new ArgumentException("Can't have a negative input length!");
		}
		int blockSize = cipher.GetBlockSize();
		int num = blockSize - bufOff;
		if (len > num)
		{
			Array.Copy(inBytes, inOff, buf, bufOff, num);
			cipher.ProcessBlock(buf, 0, mac, 0);
			bufOff = 0;
			len -= num;
			inOff += num;
			while (len > blockSize)
			{
				cipher.ProcessBlock(inBytes, inOff, mac, 0);
				len -= blockSize;
				inOff += blockSize;
			}
		}
		Array.Copy(inBytes, inOff, buf, bufOff, len);
		bufOff += len;
	}

	public int DoFinal(byte[] outBytes, int outOff)
	{
		int blockSize = cipher.GetBlockSize();
		byte[] array;
		if (bufOff == blockSize)
		{
			array = Lu;
		}
		else
		{
			new ISO7816d4Padding().AddPadding(buf, bufOff);
			array = Lu2;
		}
		for (int i = 0; i < mac.Length; i++)
		{
			byte[] array2;
			byte[] array3 = (array2 = buf);
			int num = i;
			nint num2 = num;
			array3[num] = (byte)(array2[num2] ^ array[i]);
		}
		cipher.ProcessBlock(buf, 0, mac, 0);
		Array.Copy(mac, 0, outBytes, outOff, macSize);
		Reset();
		return macSize;
	}

	public void Reset()
	{
		Array.Clear(buf, 0, buf.Length);
		bufOff = 0;
		cipher.Reset();
	}
}
