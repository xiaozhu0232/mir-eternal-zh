using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class Pkcs12ParametersGenerator : PbeParametersGenerator
{
	public const int KeyMaterial = 1;

	public const int IVMaterial = 2;

	public const int MacMaterial = 3;

	private readonly IDigest digest;

	private readonly int u;

	private readonly int v;

	public Pkcs12ParametersGenerator(IDigest digest)
	{
		this.digest = digest;
		u = digest.GetDigestSize();
		v = digest.GetByteLength();
	}

	private void Adjust(byte[] a, int aOff, byte[] b)
	{
		int num = (b[b.Length - 1] & 0xFF) + (a[aOff + b.Length - 1] & 0xFF) + 1;
		a[aOff + b.Length - 1] = (byte)num;
		num = (int)((uint)num >> 8);
		for (int num2 = b.Length - 2; num2 >= 0; num2--)
		{
			num += (b[num2] & 0xFF) + (a[aOff + num2] & 0xFF);
			a[aOff + num2] = (byte)num;
			num = (int)((uint)num >> 8);
		}
	}

	private byte[] GenerateDerivedKey(int idByte, int n)
	{
		byte[] array = new byte[v];
		byte[] array2 = new byte[n];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = (byte)idByte;
		}
		byte[] array3;
		if (mSalt != null && mSalt.Length != 0)
		{
			array3 = new byte[v * ((mSalt.Length + v - 1) / v)];
			for (int j = 0; j != array3.Length; j++)
			{
				array3[j] = mSalt[j % mSalt.Length];
			}
		}
		else
		{
			array3 = new byte[0];
		}
		byte[] array4;
		if (mPassword != null && mPassword.Length != 0)
		{
			array4 = new byte[v * ((mPassword.Length + v - 1) / v)];
			for (int k = 0; k != array4.Length; k++)
			{
				array4[k] = mPassword[k % mPassword.Length];
			}
		}
		else
		{
			array4 = new byte[0];
		}
		byte[] array5 = new byte[array3.Length + array4.Length];
		Array.Copy(array3, 0, array5, 0, array3.Length);
		Array.Copy(array4, 0, array5, array3.Length, array4.Length);
		byte[] array6 = new byte[v];
		int num = (n + u - 1) / u;
		byte[] array7 = new byte[u];
		for (int l = 1; l <= num; l++)
		{
			digest.BlockUpdate(array, 0, array.Length);
			digest.BlockUpdate(array5, 0, array5.Length);
			digest.DoFinal(array7, 0);
			for (int m = 1; m != mIterationCount; m++)
			{
				digest.BlockUpdate(array7, 0, array7.Length);
				digest.DoFinal(array7, 0);
			}
			for (int num2 = 0; num2 != array6.Length; num2++)
			{
				array6[num2] = array7[num2 % array7.Length];
			}
			for (int num3 = 0; num3 != array5.Length / v; num3++)
			{
				Adjust(array5, num3 * v, array6);
			}
			if (l == num)
			{
				Array.Copy(array7, 0, array2, (l - 1) * u, array2.Length - (l - 1) * u);
			}
			else
			{
				Array.Copy(array7, 0, array2, (l - 1) * u, array7.Length);
			}
		}
		return array2;
	}

	public override ICipherParameters GenerateDerivedParameters(int keySize)
	{
		keySize /= 8;
		byte[] key = GenerateDerivedKey(1, keySize);
		return new KeyParameter(key, 0, keySize);
	}

	public override ICipherParameters GenerateDerivedParameters(string algorithm, int keySize)
	{
		keySize /= 8;
		byte[] keyBytes = GenerateDerivedKey(1, keySize);
		return ParameterUtilities.CreateKeyParameter(algorithm, keyBytes, 0, keySize);
	}

	public override ICipherParameters GenerateDerivedParameters(int keySize, int ivSize)
	{
		keySize /= 8;
		ivSize /= 8;
		byte[] key = GenerateDerivedKey(1, keySize);
		byte[] iv = GenerateDerivedKey(2, ivSize);
		return new ParametersWithIV(new KeyParameter(key, 0, keySize), iv, 0, ivSize);
	}

	public override ICipherParameters GenerateDerivedParameters(string algorithm, int keySize, int ivSize)
	{
		keySize /= 8;
		ivSize /= 8;
		byte[] keyBytes = GenerateDerivedKey(1, keySize);
		KeyParameter parameters = ParameterUtilities.CreateKeyParameter(algorithm, keyBytes, 0, keySize);
		byte[] iv = GenerateDerivedKey(2, ivSize);
		return new ParametersWithIV(parameters, iv, 0, ivSize);
	}

	public override ICipherParameters GenerateDerivedMacParameters(int keySize)
	{
		keySize /= 8;
		byte[] key = GenerateDerivedKey(3, keySize);
		return new KeyParameter(key, 0, keySize);
	}
}
