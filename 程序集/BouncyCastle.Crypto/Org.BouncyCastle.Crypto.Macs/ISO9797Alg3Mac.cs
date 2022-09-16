using System;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Macs;

public class ISO9797Alg3Mac : IMac
{
	private byte[] mac;

	private byte[] buf;

	private int bufOff;

	private IBlockCipher cipher;

	private IBlockCipherPadding padding;

	private int macSize;

	private KeyParameter lastKey2;

	private KeyParameter lastKey3;

	public string AlgorithmName => "ISO9797Alg3";

	public ISO9797Alg3Mac(IBlockCipher cipher)
		: this(cipher, cipher.GetBlockSize() * 8, null)
	{
	}

	public ISO9797Alg3Mac(IBlockCipher cipher, IBlockCipherPadding padding)
		: this(cipher, cipher.GetBlockSize() * 8, padding)
	{
	}

	public ISO9797Alg3Mac(IBlockCipher cipher, int macSizeInBits)
		: this(cipher, macSizeInBits, null)
	{
	}

	public ISO9797Alg3Mac(IBlockCipher cipher, int macSizeInBits, IBlockCipherPadding padding)
	{
		if (macSizeInBits % 8 != 0)
		{
			throw new ArgumentException("MAC size must be multiple of 8");
		}
		if (!(cipher is DesEngine))
		{
			throw new ArgumentException("cipher must be instance of DesEngine");
		}
		this.cipher = new CbcBlockCipher(cipher);
		this.padding = padding;
		macSize = macSizeInBits / 8;
		mac = new byte[cipher.GetBlockSize()];
		buf = new byte[cipher.GetBlockSize()];
		bufOff = 0;
	}

	public void Init(ICipherParameters parameters)
	{
		Reset();
		if (!(parameters is KeyParameter) && !(parameters is ParametersWithIV))
		{
			throw new ArgumentException("parameters must be an instance of KeyParameter or ParametersWithIV");
		}
		KeyParameter keyParameter = ((!(parameters is KeyParameter)) ? ((KeyParameter)((ParametersWithIV)parameters).Parameters) : ((KeyParameter)parameters));
		byte[] key = keyParameter.GetKey();
		KeyParameter parameters2;
		if (key.Length == 16)
		{
			parameters2 = new KeyParameter(key, 0, 8);
			lastKey2 = new KeyParameter(key, 8, 8);
			lastKey3 = parameters2;
		}
		else
		{
			if (key.Length != 24)
			{
				throw new ArgumentException("Key must be either 112 or 168 bit long");
			}
			parameters2 = new KeyParameter(key, 0, 8);
			lastKey2 = new KeyParameter(key, 8, 8);
			lastKey3 = new KeyParameter(key, 16, 8);
		}
		if (parameters is ParametersWithIV)
		{
			cipher.Init(forEncryption: true, new ParametersWithIV(parameters2, ((ParametersWithIV)parameters).GetIV()));
		}
		else
		{
			cipher.Init(forEncryption: true, parameters2);
		}
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
			Array.Copy(input, inOff, buf, bufOff, num2);
			num += cipher.ProcessBlock(buf, 0, mac, 0);
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
		Array.Copy(input, inOff, buf, bufOff, len);
		bufOff += len;
	}

	public int DoFinal(byte[] output, int outOff)
	{
		int blockSize = cipher.GetBlockSize();
		if (padding == null)
		{
			while (bufOff < blockSize)
			{
				buf[bufOff++] = 0;
			}
		}
		else
		{
			if (bufOff == blockSize)
			{
				cipher.ProcessBlock(buf, 0, mac, 0);
				bufOff = 0;
			}
			padding.AddPadding(buf, bufOff);
		}
		cipher.ProcessBlock(buf, 0, mac, 0);
		DesEngine desEngine = new DesEngine();
		desEngine.Init(forEncryption: false, lastKey2);
		desEngine.ProcessBlock(mac, 0, mac, 0);
		desEngine.Init(forEncryption: true, lastKey3);
		desEngine.ProcessBlock(mac, 0, mac, 0);
		Array.Copy(mac, 0, output, outOff, macSize);
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
