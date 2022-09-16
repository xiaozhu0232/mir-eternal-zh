using System;

namespace Org.BouncyCastle.Crypto.Generators;

public class Poly1305KeyGenerator : CipherKeyGenerator
{
	private const byte R_MASK_LOW_2 = 252;

	private const byte R_MASK_HIGH_4 = 15;

	protected override void engineInit(KeyGenerationParameters param)
	{
		random = param.Random;
		strength = 32;
	}

	protected override byte[] engineGenerateKey()
	{
		byte[] array = base.engineGenerateKey();
		Clamp(array);
		return array;
	}

	public static void Clamp(byte[] key)
	{
		if (key.Length != 32)
		{
			throw new ArgumentException("Poly1305 key must be 256 bits.");
		}
		byte[] array;
		(array = key)[3] = (byte)(array[3] & 0xFu);
		(array = key)[7] = (byte)(array[7] & 0xFu);
		(array = key)[11] = (byte)(array[11] & 0xFu);
		(array = key)[15] = (byte)(array[15] & 0xFu);
		(array = key)[4] = (byte)(array[4] & 0xFCu);
		(array = key)[8] = (byte)(array[8] & 0xFCu);
		(array = key)[12] = (byte)(array[12] & 0xFCu);
	}

	public static void CheckKey(byte[] key)
	{
		if (key.Length != 32)
		{
			throw new ArgumentException("Poly1305 key must be 256 bits.");
		}
		CheckMask(key[3], 15);
		CheckMask(key[7], 15);
		CheckMask(key[11], 15);
		CheckMask(key[15], 15);
		CheckMask(key[4], 252);
		CheckMask(key[8], 252);
		CheckMask(key[12], 252);
	}

	private static void CheckMask(byte b, byte mask)
	{
		if ((b & ~mask) != 0)
		{
			throw new ArgumentException("Invalid format for r portion of Poly1305 key.");
		}
	}
}
