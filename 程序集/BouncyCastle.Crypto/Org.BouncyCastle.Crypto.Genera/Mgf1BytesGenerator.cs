using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Generators;

public class Mgf1BytesGenerator : IDerivationFunction
{
	private IDigest digest;

	private byte[] seed;

	private int hLen;

	public IDigest Digest => digest;

	public Mgf1BytesGenerator(IDigest digest)
	{
		this.digest = digest;
		hLen = digest.GetDigestSize();
	}

	public void Init(IDerivationParameters parameters)
	{
		if (!typeof(MgfParameters).IsInstanceOfType(parameters))
		{
			throw new ArgumentException("MGF parameters required for MGF1Generator");
		}
		MgfParameters mgfParameters = (MgfParameters)parameters;
		seed = mgfParameters.GetSeed();
	}

	private void ItoOSP(int i, byte[] sp)
	{
		sp[0] = (byte)((uint)i >> 24);
		sp[1] = (byte)((uint)i >> 16);
		sp[2] = (byte)((uint)i >> 8);
		sp[3] = (byte)i;
	}

	public int GenerateBytes(byte[] output, int outOff, int length)
	{
		if (output.Length - length < outOff)
		{
			throw new DataLengthException("output buffer too small");
		}
		byte[] array = new byte[hLen];
		byte[] array2 = new byte[4];
		int num = 0;
		digest.Reset();
		if (length > hLen)
		{
			do
			{
				ItoOSP(num, array2);
				digest.BlockUpdate(seed, 0, seed.Length);
				digest.BlockUpdate(array2, 0, array2.Length);
				digest.DoFinal(array, 0);
				Array.Copy(array, 0, output, outOff + num * hLen, hLen);
			}
			while (++num < length / hLen);
		}
		if (num * hLen < length)
		{
			ItoOSP(num, array2);
			digest.BlockUpdate(seed, 0, seed.Length);
			digest.BlockUpdate(array2, 0, array2.Length);
			digest.DoFinal(array, 0);
			Array.Copy(array, 0, output, outOff + num * hLen, length - num * hLen);
		}
		return length;
	}
}
