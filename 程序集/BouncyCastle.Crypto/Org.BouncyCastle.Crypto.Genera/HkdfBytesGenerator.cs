using System;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Generators;

public class HkdfBytesGenerator : IDerivationFunction
{
	private HMac hMacHash;

	private int hashLen;

	private byte[] info;

	private byte[] currentT;

	private int generatedBytes;

	public virtual IDigest Digest => hMacHash.GetUnderlyingDigest();

	public HkdfBytesGenerator(IDigest hash)
	{
		hMacHash = new HMac(hash);
		hashLen = hash.GetDigestSize();
	}

	public virtual void Init(IDerivationParameters parameters)
	{
		if (!(parameters is HkdfParameters))
		{
			throw new ArgumentException("HKDF parameters required for HkdfBytesGenerator", "parameters");
		}
		HkdfParameters hkdfParameters = (HkdfParameters)parameters;
		if (hkdfParameters.SkipExtract)
		{
			hMacHash.Init(new KeyParameter(hkdfParameters.GetIkm()));
		}
		else
		{
			hMacHash.Init(Extract(hkdfParameters.GetSalt(), hkdfParameters.GetIkm()));
		}
		info = hkdfParameters.GetInfo();
		generatedBytes = 0;
		currentT = new byte[hashLen];
	}

	private KeyParameter Extract(byte[] salt, byte[] ikm)
	{
		if (salt == null)
		{
			hMacHash.Init(new KeyParameter(new byte[hashLen]));
		}
		else
		{
			hMacHash.Init(new KeyParameter(salt));
		}
		hMacHash.BlockUpdate(ikm, 0, ikm.Length);
		byte[] array = new byte[hashLen];
		hMacHash.DoFinal(array, 0);
		return new KeyParameter(array);
	}

	private void ExpandNext()
	{
		int num = generatedBytes / hashLen + 1;
		if (num >= 256)
		{
			throw new DataLengthException("HKDF cannot generate more than 255 blocks of HashLen size");
		}
		if (generatedBytes != 0)
		{
			hMacHash.BlockUpdate(currentT, 0, hashLen);
		}
		hMacHash.BlockUpdate(info, 0, info.Length);
		hMacHash.Update((byte)num);
		hMacHash.DoFinal(currentT, 0);
	}

	public virtual int GenerateBytes(byte[] output, int outOff, int len)
	{
		if (generatedBytes + len > 255 * hashLen)
		{
			throw new DataLengthException("HKDF may only be used for 255 * HashLen bytes of output");
		}
		if (generatedBytes % hashLen == 0)
		{
			ExpandNext();
		}
		int num = len;
		int sourceIndex = generatedBytes % hashLen;
		int val = hashLen - generatedBytes % hashLen;
		int num2 = System.Math.Min(val, num);
		Array.Copy(currentT, sourceIndex, output, outOff, num2);
		generatedBytes += num2;
		num -= num2;
		outOff += num2;
		while (num > 0)
		{
			ExpandNext();
			num2 = System.Math.Min(hashLen, num);
			Array.Copy(currentT, 0, output, outOff, num2);
			generatedBytes += num2;
			num -= num2;
			outOff += num2;
		}
		return len;
	}
}
