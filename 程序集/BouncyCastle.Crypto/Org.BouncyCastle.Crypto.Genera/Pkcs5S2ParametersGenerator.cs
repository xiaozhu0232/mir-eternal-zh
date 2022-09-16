using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class Pkcs5S2ParametersGenerator : PbeParametersGenerator
{
	private readonly IMac hMac;

	private readonly byte[] state;

	public Pkcs5S2ParametersGenerator()
		: this(new Sha1Digest())
	{
	}

	public Pkcs5S2ParametersGenerator(IDigest digest)
	{
		hMac = new HMac(digest);
		state = new byte[hMac.GetMacSize()];
	}

	private void F(byte[] S, int c, byte[] iBuf, byte[] outBytes, int outOff)
	{
		if (c == 0)
		{
			throw new ArgumentException("iteration count must be at least 1.");
		}
		if (S != null)
		{
			hMac.BlockUpdate(S, 0, S.Length);
		}
		hMac.BlockUpdate(iBuf, 0, iBuf.Length);
		hMac.DoFinal(state, 0);
		Array.Copy(state, 0, outBytes, outOff, state.Length);
		for (int i = 1; i < c; i++)
		{
			hMac.BlockUpdate(state, 0, state.Length);
			hMac.DoFinal(state, 0);
			for (int j = 0; j < state.Length; j++)
			{
				byte[] array;
				byte[] array2 = (array = outBytes);
				int num = outOff + j;
				nint num2 = num;
				array2[num] = (byte)(array[num2] ^ state[j]);
			}
		}
	}

	private byte[] GenerateDerivedKey(int dkLen)
	{
		int macSize = hMac.GetMacSize();
		int num = (dkLen + macSize - 1) / macSize;
		byte[] array = new byte[4];
		byte[] array2 = new byte[num * macSize];
		int num2 = 0;
		ICipherParameters parameters = new KeyParameter(mPassword);
		hMac.Init(parameters);
		for (int i = 1; i <= num; i++)
		{
			int num3 = 3;
			while (true)
			{
				byte[] array3;
				byte[] array4 = (array3 = array);
				int num4 = num3;
				nint num5 = num4;
				byte b;
				array4[num4] = (b = (byte)(array3[num5] + 1));
				if (b != 0)
				{
					break;
				}
				num3--;
			}
			F(mSalt, mIterationCount, array, array2, num2);
			num2 += macSize;
		}
		return array2;
	}

	public override ICipherParameters GenerateDerivedParameters(int keySize)
	{
		return GenerateDerivedMacParameters(keySize);
	}

	public override ICipherParameters GenerateDerivedParameters(string algorithm, int keySize)
	{
		keySize /= 8;
		byte[] keyBytes = GenerateDerivedKey(keySize);
		return ParameterUtilities.CreateKeyParameter(algorithm, keyBytes, 0, keySize);
	}

	public override ICipherParameters GenerateDerivedParameters(int keySize, int ivSize)
	{
		keySize /= 8;
		ivSize /= 8;
		byte[] array = GenerateDerivedKey(keySize + ivSize);
		return new ParametersWithIV(new KeyParameter(array, 0, keySize), array, keySize, ivSize);
	}

	public override ICipherParameters GenerateDerivedParameters(string algorithm, int keySize, int ivSize)
	{
		keySize /= 8;
		ivSize /= 8;
		byte[] array = GenerateDerivedKey(keySize + ivSize);
		KeyParameter parameters = ParameterUtilities.CreateKeyParameter(algorithm, array, 0, keySize);
		return new ParametersWithIV(parameters, array, keySize, ivSize);
	}

	public override ICipherParameters GenerateDerivedMacParameters(int keySize)
	{
		keySize /= 8;
		byte[] key = GenerateDerivedKey(keySize);
		return new KeyParameter(key, 0, keySize);
	}
}
