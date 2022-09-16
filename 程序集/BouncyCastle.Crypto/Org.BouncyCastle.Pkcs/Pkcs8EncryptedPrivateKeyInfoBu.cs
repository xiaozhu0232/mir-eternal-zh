using System;
using System.IO;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Pkcs;

public class Pkcs8EncryptedPrivateKeyInfoBuilder
{
	private PrivateKeyInfo privateKeyInfo;

	public Pkcs8EncryptedPrivateKeyInfoBuilder(byte[] privateKeyInfo)
		: this(PrivateKeyInfo.GetInstance(privateKeyInfo))
	{
	}

	public Pkcs8EncryptedPrivateKeyInfoBuilder(PrivateKeyInfo privateKeyInfo)
	{
		this.privateKeyInfo = privateKeyInfo;
	}

	public Pkcs8EncryptedPrivateKeyInfo Build(ICipherBuilder encryptor)
	{
		try
		{
			MemoryStream memoryStream = new MemoryOutputStream();
			ICipher cipher = encryptor.BuildCipher(memoryStream);
			byte[] encoded = privateKeyInfo.GetEncoded();
			Stream stream = cipher.Stream;
			stream.Write(encoded, 0, encoded.Length);
			Platform.Dispose(stream);
			return new Pkcs8EncryptedPrivateKeyInfo(new EncryptedPrivateKeyInfo((AlgorithmIdentifier)encryptor.AlgorithmDetails, memoryStream.ToArray()));
		}
		catch (IOException)
		{
			throw new InvalidOperationException("cannot encode privateKeyInfo");
		}
	}
}
