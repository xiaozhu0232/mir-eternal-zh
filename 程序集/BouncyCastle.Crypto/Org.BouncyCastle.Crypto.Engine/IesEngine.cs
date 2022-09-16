using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class IesEngine
{
	private readonly IBasicAgreement agree;

	private readonly IDerivationFunction kdf;

	private readonly IMac mac;

	private readonly BufferedBlockCipher cipher;

	private readonly byte[] macBuf;

	private bool forEncryption;

	private ICipherParameters privParam;

	private ICipherParameters pubParam;

	private IesParameters param;

	public IesEngine(IBasicAgreement agree, IDerivationFunction kdf, IMac mac)
	{
		this.agree = agree;
		this.kdf = kdf;
		this.mac = mac;
		macBuf = new byte[mac.GetMacSize()];
	}

	public IesEngine(IBasicAgreement agree, IDerivationFunction kdf, IMac mac, BufferedBlockCipher cipher)
	{
		this.agree = agree;
		this.kdf = kdf;
		this.mac = mac;
		macBuf = new byte[mac.GetMacSize()];
		this.cipher = cipher;
	}

	public virtual void Init(bool forEncryption, ICipherParameters privParameters, ICipherParameters pubParameters, ICipherParameters iesParameters)
	{
		this.forEncryption = forEncryption;
		privParam = privParameters;
		pubParam = pubParameters;
		param = (IesParameters)iesParameters;
	}

	private byte[] DecryptBlock(byte[] in_enc, int inOff, int inLen, byte[] z)
	{
		byte[] array = null;
		KeyParameter keyParameter = null;
		KdfParameters kdfParameters = new KdfParameters(z, param.GetDerivationV());
		int macKeySize = param.MacKeySize;
		kdf.Init(kdfParameters);
		if (inLen < mac.GetMacSize())
		{
			throw new InvalidCipherTextException("Length of input must be greater than the MAC");
		}
		inLen -= mac.GetMacSize();
		if (cipher == null)
		{
			byte[] array2 = GenerateKdfBytes(kdfParameters, inLen + macKeySize / 8);
			array = new byte[inLen];
			for (int i = 0; i != inLen; i++)
			{
				array[i] = (byte)(in_enc[inOff + i] ^ array2[i]);
			}
			keyParameter = new KeyParameter(array2, inLen, macKeySize / 8);
		}
		else
		{
			int cipherKeySize = ((IesWithCipherParameters)param).CipherKeySize;
			byte[] key = GenerateKdfBytes(kdfParameters, cipherKeySize / 8 + macKeySize / 8);
			cipher.Init(forEncryption: false, new KeyParameter(key, 0, cipherKeySize / 8));
			array = cipher.DoFinal(in_enc, inOff, inLen);
			keyParameter = new KeyParameter(key, cipherKeySize / 8, macKeySize / 8);
		}
		byte[] encodingV = param.GetEncodingV();
		mac.Init(keyParameter);
		mac.BlockUpdate(in_enc, inOff, inLen);
		mac.BlockUpdate(encodingV, 0, encodingV.Length);
		mac.DoFinal(macBuf, 0);
		inOff += inLen;
		byte[] a = Arrays.CopyOfRange(in_enc, inOff, inOff + macBuf.Length);
		if (!Arrays.ConstantTimeAreEqual(a, macBuf))
		{
			throw new InvalidCipherTextException("Invalid MAC.");
		}
		return array;
	}

	private byte[] EncryptBlock(byte[] input, int inOff, int inLen, byte[] z)
	{
		byte[] array = null;
		KeyParameter keyParameter = null;
		KdfParameters kParam = new KdfParameters(z, param.GetDerivationV());
		int num = 0;
		int macKeySize = param.MacKeySize;
		if (cipher == null)
		{
			byte[] array2 = GenerateKdfBytes(kParam, inLen + macKeySize / 8);
			array = new byte[inLen + mac.GetMacSize()];
			num = inLen;
			for (int i = 0; i != inLen; i++)
			{
				array[i] = (byte)(input[inOff + i] ^ array2[i]);
			}
			keyParameter = new KeyParameter(array2, inLen, macKeySize / 8);
		}
		else
		{
			int cipherKeySize = ((IesWithCipherParameters)param).CipherKeySize;
			byte[] key = GenerateKdfBytes(kParam, cipherKeySize / 8 + macKeySize / 8);
			cipher.Init(forEncryption: true, new KeyParameter(key, 0, cipherKeySize / 8));
			num = cipher.GetOutputSize(inLen);
			byte[] array3 = new byte[num];
			int num2 = cipher.ProcessBytes(input, inOff, inLen, array3, 0);
			num2 += cipher.DoFinal(array3, num2);
			array = new byte[num2 + mac.GetMacSize()];
			num = num2;
			Array.Copy(array3, 0, array, 0, num2);
			keyParameter = new KeyParameter(key, cipherKeySize / 8, macKeySize / 8);
		}
		byte[] encodingV = param.GetEncodingV();
		mac.Init(keyParameter);
		mac.BlockUpdate(array, 0, num);
		mac.BlockUpdate(encodingV, 0, encodingV.Length);
		mac.DoFinal(array, num);
		return array;
	}

	private byte[] GenerateKdfBytes(KdfParameters kParam, int length)
	{
		byte[] array = new byte[length];
		kdf.Init(kParam);
		kdf.GenerateBytes(array, 0, array.Length);
		return array;
	}

	public virtual byte[] ProcessBlock(byte[] input, int inOff, int inLen)
	{
		agree.Init(privParam);
		BigInteger n = agree.CalculateAgreement(pubParam);
		byte[] array = BigIntegers.AsUnsignedByteArray(agree.GetFieldSize(), n);
		try
		{
			return forEncryption ? EncryptBlock(input, inOff, inLen, array) : DecryptBlock(input, inOff, inLen, array);
		}
		finally
		{
			Array.Clear(array, 0, array.Length);
		}
	}
}
