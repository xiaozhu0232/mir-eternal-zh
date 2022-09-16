using System;
using System.IO;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Pkcs;

public class Pkcs8EncryptedPrivateKeyInfo
{
	private EncryptedPrivateKeyInfo encryptedPrivateKeyInfo;

	private static EncryptedPrivateKeyInfo parseBytes(byte[] pkcs8Encoding)
	{
		try
		{
			return EncryptedPrivateKeyInfo.GetInstance(pkcs8Encoding);
		}
		catch (ArgumentException ex)
		{
			throw new PkcsIOException("malformed data: " + ex.Message, ex);
		}
		catch (Exception ex2)
		{
			throw new PkcsIOException("malformed data: " + ex2.Message, ex2);
		}
	}

	public Pkcs8EncryptedPrivateKeyInfo(EncryptedPrivateKeyInfo encryptedPrivateKeyInfo)
	{
		this.encryptedPrivateKeyInfo = encryptedPrivateKeyInfo;
	}

	public Pkcs8EncryptedPrivateKeyInfo(byte[] encryptedPrivateKeyInfo)
		: this(parseBytes(encryptedPrivateKeyInfo))
	{
	}

	public EncryptedPrivateKeyInfo ToAsn1Structure()
	{
		return encryptedPrivateKeyInfo;
	}

	public byte[] GetEncryptedData()
	{
		return encryptedPrivateKeyInfo.GetEncryptedData();
	}

	public byte[] GetEncoded()
	{
		return encryptedPrivateKeyInfo.GetEncoded();
	}

	public PrivateKeyInfo DecryptPrivateKeyInfo(IDecryptorBuilderProvider inputDecryptorProvider)
	{
		try
		{
			ICipherBuilder cipherBuilder = inputDecryptorProvider.CreateDecryptorBuilder(encryptedPrivateKeyInfo.EncryptionAlgorithm);
			ICipher cipher = cipherBuilder.BuildCipher(new MemoryInputStream(encryptedPrivateKeyInfo.GetEncryptedData()));
			Stream stream = cipher.Stream;
			byte[] obj = Streams.ReadAll(cipher.Stream);
			Platform.Dispose(stream);
			return PrivateKeyInfo.GetInstance(obj);
		}
		catch (Exception ex)
		{
			throw new PkcsException("unable to read encrypted data: " + ex.Message, ex);
		}
	}
}
