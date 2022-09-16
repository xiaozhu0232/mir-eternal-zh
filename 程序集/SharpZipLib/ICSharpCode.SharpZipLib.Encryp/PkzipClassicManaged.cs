using System;
using System.Security.Cryptography;

namespace ICSharpCode.SharpZipLib.Encryption;

public sealed class PkzipClassicManaged : PkzipClassic
{
	private byte[] key;

	public override int BlockSize
	{
		get
		{
			return 8;
		}
		set
		{
			if (value != 8)
			{
				throw new CryptographicException();
			}
		}
	}

	public override KeySizes[] LegalKeySizes => new KeySizes[1]
	{
		new KeySizes(96, 96, 0)
	};

	public override KeySizes[] LegalBlockSizes => new KeySizes[1]
	{
		new KeySizes(8, 8, 0)
	};

	public override byte[] Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
		}
	}

	public override void GenerateIV()
	{
	}

	public override void GenerateKey()
	{
		key = new byte[12];
		Random random = new Random();
		random.NextBytes(key);
	}

	public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
	{
		return new PkzipClassicEncryptCryptoTransform(rgbKey);
	}

	public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
	{
		return new PkzipClassicDecryptCryptoTransform(rgbKey);
	}
}
