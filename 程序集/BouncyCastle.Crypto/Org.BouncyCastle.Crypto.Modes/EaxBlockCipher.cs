using System;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Modes;

public class EaxBlockCipher : IAeadBlockCipher, IAeadCipher
{
	private enum Tag : byte
	{
		N,
		H,
		C
	}

	private SicBlockCipher cipher;

	private bool forEncryption;

	private int blockSize;

	private IMac mac;

	private byte[] nonceMac;

	private byte[] associatedTextMac;

	private byte[] macBlock;

	private int macSize;

	private byte[] bufBlock;

	private int bufOff;

	private bool cipherInitialized;

	private byte[] initialAssociatedText;

	public virtual string AlgorithmName => cipher.GetUnderlyingCipher().AlgorithmName + "/EAX";

	public EaxBlockCipher(IBlockCipher cipher)
	{
		blockSize = cipher.GetBlockSize();
		mac = new CMac(cipher);
		macBlock = new byte[blockSize];
		associatedTextMac = new byte[mac.GetMacSize()];
		nonceMac = new byte[mac.GetMacSize()];
		this.cipher = new SicBlockCipher(cipher);
	}

	public virtual IBlockCipher GetUnderlyingCipher()
	{
		return cipher;
	}

	public virtual int GetBlockSize()
	{
		return cipher.GetBlockSize();
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		this.forEncryption = forEncryption;
		byte[] array;
		ICipherParameters parameters2;
		if (parameters is AeadParameters)
		{
			AeadParameters aeadParameters = (AeadParameters)parameters;
			array = aeadParameters.GetNonce();
			initialAssociatedText = aeadParameters.GetAssociatedText();
			macSize = aeadParameters.MacSize / 8;
			parameters2 = aeadParameters.Key;
		}
		else
		{
			if (!(parameters is ParametersWithIV))
			{
				throw new ArgumentException("invalid parameters passed to EAX");
			}
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			array = parametersWithIV.GetIV();
			initialAssociatedText = null;
			macSize = mac.GetMacSize() / 2;
			parameters2 = parametersWithIV.Parameters;
		}
		bufBlock = new byte[forEncryption ? blockSize : (blockSize + macSize)];
		byte[] array2 = new byte[blockSize];
		mac.Init(parameters2);
		array2[blockSize - 1] = 0;
		mac.BlockUpdate(array2, 0, blockSize);
		mac.BlockUpdate(array, 0, array.Length);
		mac.DoFinal(nonceMac, 0);
		cipher.Init(forEncryption: true, new ParametersWithIV(null, nonceMac));
		Reset();
	}

	private void InitCipher()
	{
		if (!cipherInitialized)
		{
			cipherInitialized = true;
			mac.DoFinal(associatedTextMac, 0);
			byte[] array = new byte[blockSize];
			array[blockSize - 1] = 2;
			mac.BlockUpdate(array, 0, blockSize);
		}
	}

	private void CalculateMac()
	{
		byte[] array = new byte[blockSize];
		mac.DoFinal(array, 0);
		for (int i = 0; i < macBlock.Length; i++)
		{
			macBlock[i] = (byte)(nonceMac[i] ^ associatedTextMac[i] ^ array[i]);
		}
	}

	public virtual void Reset()
	{
		Reset(clearMac: true);
	}

	private void Reset(bool clearMac)
	{
		cipher.Reset();
		mac.Reset();
		bufOff = 0;
		Array.Clear(bufBlock, 0, bufBlock.Length);
		if (clearMac)
		{
			Array.Clear(macBlock, 0, macBlock.Length);
		}
		byte[] array = new byte[blockSize];
		array[blockSize - 1] = 1;
		mac.BlockUpdate(array, 0, blockSize);
		cipherInitialized = false;
		if (initialAssociatedText != null)
		{
			ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
		}
	}

	public virtual void ProcessAadByte(byte input)
	{
		if (cipherInitialized)
		{
			throw new InvalidOperationException("AAD data cannot be added after encryption/decryption processing has begun.");
		}
		mac.Update(input);
	}

	public virtual void ProcessAadBytes(byte[] inBytes, int inOff, int len)
	{
		if (cipherInitialized)
		{
			throw new InvalidOperationException("AAD data cannot be added after encryption/decryption processing has begun.");
		}
		mac.BlockUpdate(inBytes, inOff, len);
	}

	public virtual int ProcessByte(byte input, byte[] outBytes, int outOff)
	{
		InitCipher();
		return Process(input, outBytes, outOff);
	}

	public virtual int ProcessBytes(byte[] inBytes, int inOff, int len, byte[] outBytes, int outOff)
	{
		InitCipher();
		int num = 0;
		for (int i = 0; i != len; i++)
		{
			num += Process(inBytes[inOff + i], outBytes, outOff + num);
		}
		return num;
	}

	public virtual int DoFinal(byte[] outBytes, int outOff)
	{
		InitCipher();
		int num = bufOff;
		byte[] array = new byte[bufBlock.Length];
		bufOff = 0;
		if (forEncryption)
		{
			Check.OutputLength(outBytes, outOff, num + macSize, "Output buffer too short");
			cipher.ProcessBlock(bufBlock, 0, array, 0);
			Array.Copy(array, 0, outBytes, outOff, num);
			mac.BlockUpdate(array, 0, num);
			CalculateMac();
			Array.Copy(macBlock, 0, outBytes, outOff + num, macSize);
			Reset(clearMac: false);
			return num + macSize;
		}
		if (num < macSize)
		{
			throw new InvalidCipherTextException("data too short");
		}
		Check.OutputLength(outBytes, outOff, num - macSize, "Output buffer too short");
		if (num > macSize)
		{
			mac.BlockUpdate(bufBlock, 0, num - macSize);
			cipher.ProcessBlock(bufBlock, 0, array, 0);
			Array.Copy(array, 0, outBytes, outOff, num - macSize);
		}
		CalculateMac();
		if (!VerifyMac(bufBlock, num - macSize))
		{
			throw new InvalidCipherTextException("mac check in EAX failed");
		}
		Reset(clearMac: false);
		return num - macSize;
	}

	public virtual byte[] GetMac()
	{
		byte[] array = new byte[macSize];
		Array.Copy(macBlock, 0, array, 0, macSize);
		return array;
	}

	public virtual int GetUpdateOutputSize(int len)
	{
		int num = len + bufOff;
		if (!forEncryption)
		{
			if (num < macSize)
			{
				return 0;
			}
			num -= macSize;
		}
		return num - num % blockSize;
	}

	public virtual int GetOutputSize(int len)
	{
		int num = len + bufOff;
		if (forEncryption)
		{
			return num + macSize;
		}
		if (num >= macSize)
		{
			return num - macSize;
		}
		return 0;
	}

	private int Process(byte b, byte[] outBytes, int outOff)
	{
		bufBlock[bufOff++] = b;
		if (bufOff == bufBlock.Length)
		{
			Check.OutputLength(outBytes, outOff, blockSize, "Output buffer is too short");
			int result;
			if (forEncryption)
			{
				result = cipher.ProcessBlock(bufBlock, 0, outBytes, outOff);
				mac.BlockUpdate(outBytes, outOff, blockSize);
			}
			else
			{
				mac.BlockUpdate(bufBlock, 0, blockSize);
				result = cipher.ProcessBlock(bufBlock, 0, outBytes, outOff);
			}
			bufOff = 0;
			if (!forEncryption)
			{
				Array.Copy(bufBlock, blockSize, bufBlock, 0, macSize);
				bufOff = macSize;
			}
			return result;
		}
		return 0;
	}

	private bool VerifyMac(byte[] mac, int off)
	{
		int num = 0;
		for (int i = 0; i < macSize; i++)
		{
			num |= macBlock[i] ^ mac[off + i];
		}
		return num == 0;
	}
}
