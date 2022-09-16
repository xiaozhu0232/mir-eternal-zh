using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Generators;

public class BaseKdfBytesGenerator : IDerivationFunction
{
	private int counterStart;

	private IDigest digest;

	private byte[] shared;

	private byte[] iv;

	public virtual IDigest Digest => digest;

	public BaseKdfBytesGenerator(int counterStart, IDigest digest)
	{
		this.counterStart = counterStart;
		this.digest = digest;
	}

	public virtual void Init(IDerivationParameters parameters)
	{
		if (parameters is KdfParameters)
		{
			KdfParameters kdfParameters = (KdfParameters)parameters;
			shared = kdfParameters.GetSharedSecret();
			iv = kdfParameters.GetIV();
			return;
		}
		if (parameters is Iso18033KdfParameters)
		{
			Iso18033KdfParameters iso18033KdfParameters = (Iso18033KdfParameters)parameters;
			shared = iso18033KdfParameters.GetSeed();
			iv = null;
			return;
		}
		throw new ArgumentException("KDF parameters required for KDF Generator");
	}

	public virtual int GenerateBytes(byte[] output, int outOff, int length)
	{
		if (output.Length - length < outOff)
		{
			throw new DataLengthException("output buffer too small");
		}
		long num = length;
		int digestSize = digest.GetDigestSize();
		if (num > 8589934591L)
		{
			throw new ArgumentException("Output length too large");
		}
		int num2 = (int)((num + digestSize - 1) / digestSize);
		byte[] array = new byte[digest.GetDigestSize()];
		byte[] array2 = new byte[4];
		Pack.UInt32_To_BE((uint)counterStart, array2, 0);
		uint num3 = (uint)counterStart & 0xFFFFFF00u;
		for (int i = 0; i < num2; i++)
		{
			digest.BlockUpdate(shared, 0, shared.Length);
			digest.BlockUpdate(array2, 0, 4);
			if (iv != null)
			{
				digest.BlockUpdate(iv, 0, iv.Length);
			}
			digest.DoFinal(array, 0);
			if (length > digestSize)
			{
				Array.Copy(array, 0, output, outOff, digestSize);
				outOff += digestSize;
				length -= digestSize;
			}
			else
			{
				Array.Copy(array, 0, output, outOff, length);
			}
			byte[] array3;
			byte b;
			(array3 = array2)[3] = (b = (byte)(array3[3] + 1));
			if (b == 0)
			{
				num3 += 256;
				Pack.UInt32_To_BE(num3, array2, 0);
			}
		}
		digest.Reset();
		return (int)num;
	}
}
