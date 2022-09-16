using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class Pkcs5S1ParametersGenerator : PbeParametersGenerator
{
	private readonly IDigest digest;

	public Pkcs5S1ParametersGenerator(IDigest digest)
	{
		this.digest = digest;
	}

	private byte[] GenerateDerivedKey()
	{
		byte[] array = new byte[digest.GetDigestSize()];
		digest.BlockUpdate(mPassword, 0, mPassword.Length);
		digest.BlockUpdate(mSalt, 0, mSalt.Length);
		digest.DoFinal(array, 0);
		for (int i = 1; i < mIterationCount; i++)
		{
			digest.BlockUpdate(array, 0, array.Length);
			digest.DoFinal(array, 0);
		}
		return array;
	}

	public override ICipherParameters GenerateDerivedParameters(int keySize)
	{
		return GenerateDerivedMacParameters(keySize);
	}

	public override ICipherParameters GenerateDerivedParameters(string algorithm, int keySize)
	{
		keySize /= 8;
		if (keySize > digest.GetDigestSize())
		{
			throw new ArgumentException("Can't Generate a derived key " + keySize + " bytes long.");
		}
		byte[] keyBytes = GenerateDerivedKey();
		return ParameterUtilities.CreateKeyParameter(algorithm, keyBytes, 0, keySize);
	}

	public override ICipherParameters GenerateDerivedParameters(int keySize, int ivSize)
	{
		keySize /= 8;
		ivSize /= 8;
		if (keySize + ivSize > digest.GetDigestSize())
		{
			throw new ArgumentException("Can't Generate a derived key " + (keySize + ivSize) + " bytes long.");
		}
		byte[] array = GenerateDerivedKey();
		return new ParametersWithIV(new KeyParameter(array, 0, keySize), array, keySize, ivSize);
	}

	public override ICipherParameters GenerateDerivedParameters(string algorithm, int keySize, int ivSize)
	{
		keySize /= 8;
		ivSize /= 8;
		if (keySize + ivSize > digest.GetDigestSize())
		{
			throw new ArgumentException("Can't Generate a derived key " + (keySize + ivSize) + " bytes long.");
		}
		byte[] array = GenerateDerivedKey();
		KeyParameter parameters = ParameterUtilities.CreateKeyParameter(algorithm, array, 0, keySize);
		return new ParametersWithIV(parameters, array, keySize, ivSize);
	}

	public override ICipherParameters GenerateDerivedMacParameters(int keySize)
	{
		keySize /= 8;
		if (keySize > digest.GetDigestSize())
		{
			throw new ArgumentException("Can't Generate a derived key " + keySize + " bytes long.");
		}
		byte[] key = GenerateDerivedKey();
		return new KeyParameter(key, 0, keySize);
	}
}
