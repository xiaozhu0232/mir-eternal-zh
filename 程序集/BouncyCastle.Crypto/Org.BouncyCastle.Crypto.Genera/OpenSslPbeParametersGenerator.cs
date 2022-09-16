using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class OpenSslPbeParametersGenerator : PbeParametersGenerator
{
	private readonly IDigest digest = new MD5Digest();

	public override void Init(byte[] password, byte[] salt, int iterationCount)
	{
		base.Init(password, salt, 1);
	}

	public virtual void Init(byte[] password, byte[] salt)
	{
		base.Init(password, salt, 1);
	}

	private byte[] GenerateDerivedKey(int bytesNeeded)
	{
		byte[] array = new byte[digest.GetDigestSize()];
		byte[] array2 = new byte[bytesNeeded];
		int num = 0;
		while (true)
		{
			digest.BlockUpdate(mPassword, 0, mPassword.Length);
			digest.BlockUpdate(mSalt, 0, mSalt.Length);
			digest.DoFinal(array, 0);
			int num2 = ((bytesNeeded > array.Length) ? array.Length : bytesNeeded);
			Array.Copy(array, 0, array2, num, num2);
			num += num2;
			bytesNeeded -= num2;
			if (bytesNeeded == 0)
			{
				break;
			}
			digest.Reset();
			digest.BlockUpdate(array, 0, array.Length);
		}
		return array2;
	}

	[Obsolete("Use version with 'algorithm' parameter")]
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

	[Obsolete("Use version with 'algorithm' parameter")]
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
