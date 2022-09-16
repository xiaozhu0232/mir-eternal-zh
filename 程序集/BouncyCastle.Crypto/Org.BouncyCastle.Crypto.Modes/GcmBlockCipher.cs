using System;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes;

public class GcmBlockCipher : IAeadBlockCipher, IAeadCipher
{
	private const int BlockSize = 16;

	private readonly IBlockCipher cipher;

	private readonly IGcmMultiplier multiplier;

	private IGcmExponentiator exp;

	private bool forEncryption;

	private bool initialised;

	private int macSize;

	private byte[] lastKey;

	private byte[] nonce;

	private byte[] initialAssociatedText;

	private byte[] H;

	private byte[] J0;

	private byte[] bufBlock;

	private byte[] macBlock;

	private byte[] S;

	private byte[] S_at;

	private byte[] S_atPre;

	private byte[] counter;

	private uint blocksRemaining;

	private int bufOff;

	private ulong totalLength;

	private byte[] atBlock;

	private int atBlockPos;

	private ulong atLength;

	private ulong atLengthPre;

	public virtual string AlgorithmName => cipher.AlgorithmName + "/GCM";

	public GcmBlockCipher(IBlockCipher c)
		: this(c, null)
	{
	}

	public GcmBlockCipher(IBlockCipher c, IGcmMultiplier m)
	{
		if (c.GetBlockSize() != 16)
		{
			throw new ArgumentException("cipher required with a block size of " + 16 + ".");
		}
		if (m == null)
		{
			m = new Tables4kGcmMultiplier();
		}
		cipher = c;
		multiplier = m;
	}

	public IBlockCipher GetUnderlyingCipher()
	{
		return cipher;
	}

	public virtual int GetBlockSize()
	{
		return 16;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		this.forEncryption = forEncryption;
		macBlock = null;
		initialised = true;
		byte[] array = null;
		KeyParameter keyParameter;
		if (parameters is AeadParameters)
		{
			AeadParameters aeadParameters = (AeadParameters)parameters;
			array = aeadParameters.GetNonce();
			initialAssociatedText = aeadParameters.GetAssociatedText();
			int num = aeadParameters.MacSize;
			if (num < 32 || num > 128 || num % 8 != 0)
			{
				throw new ArgumentException("Invalid value for MAC size: " + num);
			}
			macSize = num / 8;
			keyParameter = aeadParameters.Key;
		}
		else
		{
			if (!(parameters is ParametersWithIV))
			{
				throw new ArgumentException("invalid parameters passed to GCM");
			}
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			array = parametersWithIV.GetIV();
			initialAssociatedText = null;
			macSize = 16;
			keyParameter = (KeyParameter)parametersWithIV.Parameters;
		}
		int num2 = (forEncryption ? 16 : (16 + macSize));
		bufBlock = new byte[num2];
		if (array == null || array.Length < 1)
		{
			throw new ArgumentException("IV must be at least 1 byte");
		}
		if (forEncryption && nonce != null && Arrays.AreEqual(nonce, array))
		{
			if (keyParameter == null)
			{
				throw new ArgumentException("cannot reuse nonce for GCM encryption");
			}
			if (lastKey != null && Arrays.AreEqual(lastKey, keyParameter.GetKey()))
			{
				throw new ArgumentException("cannot reuse nonce for GCM encryption");
			}
		}
		nonce = array;
		if (keyParameter != null)
		{
			lastKey = keyParameter.GetKey();
		}
		if (keyParameter != null)
		{
			cipher.Init(forEncryption: true, keyParameter);
			H = new byte[16];
			cipher.ProcessBlock(H, 0, H, 0);
			multiplier.Init(H);
			exp = null;
		}
		else if (H == null)
		{
			throw new ArgumentException("Key must be specified in initial init");
		}
		J0 = new byte[16];
		if (nonce.Length == 12)
		{
			Array.Copy(nonce, 0, J0, 0, nonce.Length);
			J0[15] = 1;
		}
		else
		{
			gHASH(J0, nonce, nonce.Length);
			byte[] array2 = new byte[16];
			Pack.UInt64_To_BE((ulong)nonce.Length * 8uL, array2, 8);
			gHASHBlock(J0, array2);
		}
		S = new byte[16];
		S_at = new byte[16];
		S_atPre = new byte[16];
		atBlock = new byte[16];
		atBlockPos = 0;
		atLength = 0uL;
		atLengthPre = 0uL;
		counter = Arrays.Clone(J0);
		blocksRemaining = 4294967294u;
		bufOff = 0;
		totalLength = 0uL;
		if (initialAssociatedText != null)
		{
			ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
		}
	}

	public virtual byte[] GetMac()
	{
		if (macBlock != null)
		{
			return Arrays.Clone(macBlock);
		}
		return new byte[macSize];
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
		return num - num % 16;
	}

	public virtual void ProcessAadByte(byte input)
	{
		CheckStatus();
		atBlock[atBlockPos] = input;
		if (++atBlockPos == 16)
		{
			gHASHBlock(S_at, atBlock);
			atBlockPos = 0;
			atLength += 16uL;
		}
	}

	public virtual void ProcessAadBytes(byte[] inBytes, int inOff, int len)
	{
		CheckStatus();
		for (int i = 0; i < len; i++)
		{
			atBlock[atBlockPos] = inBytes[inOff + i];
			if (++atBlockPos == 16)
			{
				gHASHBlock(S_at, atBlock);
				atBlockPos = 0;
				atLength += 16uL;
			}
		}
	}

	private void InitCipher()
	{
		if (atLength != 0)
		{
			Array.Copy(S_at, 0, S_atPre, 0, 16);
			atLengthPre = atLength;
		}
		if (atBlockPos > 0)
		{
			gHASHPartial(S_atPre, atBlock, 0, atBlockPos);
			atLengthPre += (uint)atBlockPos;
		}
		if (atLengthPre != 0)
		{
			Array.Copy(S_atPre, 0, S, 0, 16);
		}
	}

	public virtual int ProcessByte(byte input, byte[] output, int outOff)
	{
		CheckStatus();
		bufBlock[bufOff] = input;
		if (++bufOff == bufBlock.Length)
		{
			ProcessBlock(bufBlock, 0, output, outOff);
			if (forEncryption)
			{
				bufOff = 0;
			}
			else
			{
				Array.Copy(bufBlock, 16, bufBlock, 0, macSize);
				bufOff = macSize;
			}
			return 16;
		}
		return 0;
	}

	public virtual int ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
	{
		CheckStatus();
		Check.DataLength(input, inOff, len, "input buffer too short");
		int num = 0;
		if (forEncryption)
		{
			if (bufOff != 0)
			{
				while (len > 0)
				{
					len--;
					bufBlock[bufOff] = input[inOff++];
					if (++bufOff == 16)
					{
						ProcessBlock(bufBlock, 0, output, outOff);
						bufOff = 0;
						num += 16;
						break;
					}
				}
			}
			while (len >= 16)
			{
				ProcessBlock(input, inOff, output, outOff + num);
				inOff += 16;
				len -= 16;
				num += 16;
			}
			if (len > 0)
			{
				Array.Copy(input, inOff, bufBlock, 0, len);
				bufOff = len;
			}
		}
		else
		{
			for (int i = 0; i < len; i++)
			{
				bufBlock[bufOff] = input[inOff + i];
				if (++bufOff == bufBlock.Length)
				{
					ProcessBlock(bufBlock, 0, output, outOff + num);
					Array.Copy(bufBlock, 16, bufBlock, 0, macSize);
					bufOff = macSize;
					num += 16;
				}
			}
		}
		return num;
	}

	public int DoFinal(byte[] output, int outOff)
	{
		CheckStatus();
		if (totalLength == 0)
		{
			InitCipher();
		}
		int num = bufOff;
		if (forEncryption)
		{
			Check.OutputLength(output, outOff, num + macSize, "Output buffer too short");
		}
		else
		{
			if (num < macSize)
			{
				throw new InvalidCipherTextException("data too short");
			}
			num -= macSize;
			Check.OutputLength(output, outOff, num, "Output buffer too short");
		}
		if (num > 0)
		{
			ProcessPartial(bufBlock, 0, num, output, outOff);
		}
		atLength += (uint)atBlockPos;
		if (atLength > atLengthPre)
		{
			if (atBlockPos > 0)
			{
				gHASHPartial(S_at, atBlock, 0, atBlockPos);
			}
			if (atLengthPre != 0)
			{
				GcmUtilities.Xor(S_at, S_atPre);
			}
			long pow = (long)(totalLength * 8 + 127 >> 7);
			byte[] array = new byte[16];
			if (exp == null)
			{
				exp = new BasicGcmExponentiator();
				exp.Init(H);
			}
			exp.ExponentiateX(pow, array);
			GcmUtilities.Multiply(S_at, array);
			GcmUtilities.Xor(S, S_at);
		}
		byte[] array2 = new byte[16];
		Pack.UInt64_To_BE(atLength * 8, array2, 0);
		Pack.UInt64_To_BE(totalLength * 8, array2, 8);
		gHASHBlock(S, array2);
		byte[] array3 = new byte[16];
		cipher.ProcessBlock(J0, 0, array3, 0);
		GcmUtilities.Xor(array3, S);
		int num2 = num;
		macBlock = new byte[macSize];
		Array.Copy(array3, 0, macBlock, 0, macSize);
		if (forEncryption)
		{
			Array.Copy(macBlock, 0, output, outOff + bufOff, macSize);
			num2 += macSize;
		}
		else
		{
			byte[] array4 = new byte[macSize];
			Array.Copy(bufBlock, num, array4, 0, macSize);
			if (!Arrays.ConstantTimeAreEqual(macBlock, array4))
			{
				throw new InvalidCipherTextException("mac check in GCM failed");
			}
		}
		Reset(clearMac: false);
		return num2;
	}

	public virtual void Reset()
	{
		Reset(clearMac: true);
	}

	private void Reset(bool clearMac)
	{
		cipher.Reset();
		S = new byte[16];
		S_at = new byte[16];
		S_atPre = new byte[16];
		atBlock = new byte[16];
		atBlockPos = 0;
		atLength = 0uL;
		atLengthPre = 0uL;
		counter = Arrays.Clone(J0);
		blocksRemaining = 4294967294u;
		bufOff = 0;
		totalLength = 0uL;
		if (bufBlock != null)
		{
			Arrays.Fill(bufBlock, 0);
		}
		if (clearMac)
		{
			macBlock = null;
		}
		if (forEncryption)
		{
			initialised = false;
		}
		else if (initialAssociatedText != null)
		{
			ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
		}
	}

	private void ProcessBlock(byte[] buf, int bufOff, byte[] output, int outOff)
	{
		Check.OutputLength(output, outOff, 16, "Output buffer too short");
		if (totalLength == 0)
		{
			InitCipher();
		}
		byte[] array = new byte[16];
		GetNextCtrBlock(array);
		if (forEncryption)
		{
			GcmUtilities.Xor(array, buf, bufOff);
			gHASHBlock(S, array);
			Array.Copy(array, 0, output, outOff, 16);
		}
		else
		{
			gHASHBlock(S, buf, bufOff);
			GcmUtilities.Xor(array, 0, buf, bufOff, output, outOff);
		}
		totalLength += 16uL;
	}

	private void ProcessPartial(byte[] buf, int off, int len, byte[] output, int outOff)
	{
		byte[] array = new byte[16];
		GetNextCtrBlock(array);
		if (forEncryption)
		{
			GcmUtilities.Xor(buf, off, array, 0, len);
			gHASHPartial(S, buf, off, len);
		}
		else
		{
			gHASHPartial(S, buf, off, len);
			GcmUtilities.Xor(buf, off, array, 0, len);
		}
		Array.Copy(buf, off, output, outOff, len);
		totalLength += (uint)len;
	}

	private void gHASH(byte[] Y, byte[] b, int len)
	{
		for (int i = 0; i < len; i += 16)
		{
			int len2 = System.Math.Min(len - i, 16);
			gHASHPartial(Y, b, i, len2);
		}
	}

	private void gHASHBlock(byte[] Y, byte[] b)
	{
		GcmUtilities.Xor(Y, b);
		multiplier.MultiplyH(Y);
	}

	private void gHASHBlock(byte[] Y, byte[] b, int off)
	{
		GcmUtilities.Xor(Y, b, off);
		multiplier.MultiplyH(Y);
	}

	private void gHASHPartial(byte[] Y, byte[] b, int off, int len)
	{
		GcmUtilities.Xor(Y, b, off, len);
		multiplier.MultiplyH(Y);
	}

	private void GetNextCtrBlock(byte[] block)
	{
		if (blocksRemaining == 0)
		{
			throw new InvalidOperationException("Attempt to process too many blocks");
		}
		blocksRemaining--;
		uint num = 1u;
		num += counter[15];
		counter[15] = (byte)num;
		num >>= 8;
		num += counter[14];
		counter[14] = (byte)num;
		num >>= 8;
		num += counter[13];
		counter[13] = (byte)num;
		num >>= 8;
		num += counter[12];
		counter[12] = (byte)num;
		cipher.ProcessBlock(counter, 0, block, 0);
	}

	private void CheckStatus()
	{
		if (!initialised)
		{
			if (forEncryption)
			{
				throw new InvalidOperationException("GCM cipher cannot be reused for encryption");
			}
			throw new InvalidOperationException("GCM cipher needs to be initialised");
		}
	}
}
