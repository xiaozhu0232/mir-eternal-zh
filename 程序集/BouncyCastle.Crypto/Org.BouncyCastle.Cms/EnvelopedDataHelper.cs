using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms;

internal class EnvelopedDataHelper
{
	private static readonly IDictionary BaseCipherNames;

	private static readonly IDictionary MacAlgNames;

	static EnvelopedDataHelper()
	{
		BaseCipherNames = Platform.CreateHashtable();
		MacAlgNames = Platform.CreateHashtable();
		BaseCipherNames.Add(PkcsObjectIdentifiers.DesEde3Cbc, "DESEDE");
		BaseCipherNames.Add(NistObjectIdentifiers.IdAes128Cbc, "AES");
		BaseCipherNames.Add(NistObjectIdentifiers.IdAes192Cbc, "AES");
		BaseCipherNames.Add(NistObjectIdentifiers.IdAes256Cbc, "AES");
		MacAlgNames.Add(PkcsObjectIdentifiers.DesEde3Cbc, "DESEDEMac");
		MacAlgNames.Add(NistObjectIdentifiers.IdAes128Cbc, "AESMac");
		MacAlgNames.Add(NistObjectIdentifiers.IdAes192Cbc, "AESMac");
		MacAlgNames.Add(NistObjectIdentifiers.IdAes256Cbc, "AESMac");
		MacAlgNames.Add(PkcsObjectIdentifiers.RC2Cbc, "RC2Mac");
	}

	public static object CreateContentCipher(bool forEncryption, ICipherParameters encKey, AlgorithmIdentifier encryptionAlgID)
	{
		return CipherFactory.CreateContentCipher(forEncryption, encKey, encryptionAlgID);
	}

	public AlgorithmIdentifier GenerateEncryptionAlgID(DerObjectIdentifier encryptionOID, KeyParameter encKey, SecureRandom random)
	{
		return AlgorithmIdentifierFactory.GenerateEncryptionAlgID(encryptionOID, encKey.GetKey().Length * 8, random);
	}

	public CipherKeyGenerator CreateKeyGenerator(DerObjectIdentifier algorithm, SecureRandom random)
	{
		return CipherKeyGeneratorFactory.CreateKeyGenerator(algorithm, random);
	}
}
