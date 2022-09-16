using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Prng.Drbg;

internal class DrbgUtilities
{
	private static readonly IDictionary maxSecurityStrengths;

	static DrbgUtilities()
	{
		maxSecurityStrengths = Platform.CreateHashtable();
		maxSecurityStrengths.Add("SHA-1", 128);
		maxSecurityStrengths.Add("SHA-224", 192);
		maxSecurityStrengths.Add("SHA-256", 256);
		maxSecurityStrengths.Add("SHA-384", 256);
		maxSecurityStrengths.Add("SHA-512", 256);
		maxSecurityStrengths.Add("SHA-512/224", 192);
		maxSecurityStrengths.Add("SHA-512/256", 256);
	}

	internal static int GetMaxSecurityStrength(IDigest d)
	{
		return (int)maxSecurityStrengths[d.AlgorithmName];
	}

	internal static int GetMaxSecurityStrength(IMac m)
	{
		string algorithmName = m.AlgorithmName;
		return (int)maxSecurityStrengths[algorithmName.Substring(0, algorithmName.IndexOf("/"))];
	}

	internal static byte[] HashDF(IDigest digest, byte[] seedMaterial, int seedLength)
	{
		byte[] array = new byte[(seedLength + 7) / 8];
		int num = array.Length / digest.GetDigestSize();
		int num2 = 1;
		byte[] array2 = new byte[digest.GetDigestSize()];
		for (int i = 0; i <= num; i++)
		{
			digest.Update((byte)num2);
			digest.Update((byte)(seedLength >> 24));
			digest.Update((byte)(seedLength >> 16));
			digest.Update((byte)(seedLength >> 8));
			digest.Update((byte)seedLength);
			digest.BlockUpdate(seedMaterial, 0, seedMaterial.Length);
			digest.DoFinal(array2, 0);
			int length = ((array.Length - i * array2.Length > array2.Length) ? array2.Length : (array.Length - i * array2.Length));
			Array.Copy(array2, 0, array, i * array2.Length, length);
			num2++;
		}
		if (seedLength % 8 != 0)
		{
			int num3 = 8 - seedLength % 8;
			uint num4 = 0u;
			for (int j = 0; j != array.Length; j++)
			{
				uint num5 = array[j];
				array[j] = (byte)((num5 >> num3) | (num4 << 8 - num3));
				num4 = num5;
			}
		}
		return array;
	}

	internal static bool IsTooLarge(byte[] bytes, int maxBytes)
	{
		if (bytes != null)
		{
			return bytes.Length > maxBytes;
		}
		return false;
	}
}
