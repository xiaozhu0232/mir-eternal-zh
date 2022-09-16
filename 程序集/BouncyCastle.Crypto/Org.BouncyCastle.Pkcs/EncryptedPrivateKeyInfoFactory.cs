using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Pkcs;

public sealed class EncryptedPrivateKeyInfoFactory
{
	private EncryptedPrivateKeyInfoFactory()
	{
	}

	public static EncryptedPrivateKeyInfo CreateEncryptedPrivateKeyInfo(DerObjectIdentifier algorithm, char[] passPhrase, byte[] salt, int iterationCount, AsymmetricKeyParameter key)
	{
		return CreateEncryptedPrivateKeyInfo(algorithm.Id, passPhrase, salt, iterationCount, PrivateKeyInfoFactory.CreatePrivateKeyInfo(key));
	}

	public static EncryptedPrivateKeyInfo CreateEncryptedPrivateKeyInfo(string algorithm, char[] passPhrase, byte[] salt, int iterationCount, AsymmetricKeyParameter key)
	{
		return CreateEncryptedPrivateKeyInfo(algorithm, passPhrase, salt, iterationCount, PrivateKeyInfoFactory.CreatePrivateKeyInfo(key));
	}

	public static EncryptedPrivateKeyInfo CreateEncryptedPrivateKeyInfo(string algorithm, char[] passPhrase, byte[] salt, int iterationCount, PrivateKeyInfo keyInfo)
	{
		if (!(PbeUtilities.CreateEngine(algorithm) is IBufferedCipher bufferedCipher))
		{
			throw new Exception("Unknown encryption algorithm: " + algorithm);
		}
		Asn1Encodable asn1Encodable = PbeUtilities.GenerateAlgorithmParameters(algorithm, salt, iterationCount);
		ICipherParameters parameters = PbeUtilities.GenerateCipherParameters(algorithm, passPhrase, asn1Encodable);
		bufferedCipher.Init(forEncryption: true, parameters);
		byte[] encoding = bufferedCipher.DoFinal(keyInfo.GetEncoded());
		DerObjectIdentifier objectIdentifier = PbeUtilities.GetObjectIdentifier(algorithm);
		AlgorithmIdentifier algId = new AlgorithmIdentifier(objectIdentifier, asn1Encodable);
		return new EncryptedPrivateKeyInfo(algId, encoding);
	}

	public static EncryptedPrivateKeyInfo CreateEncryptedPrivateKeyInfo(DerObjectIdentifier cipherAlgorithm, DerObjectIdentifier prfAlgorithm, char[] passPhrase, byte[] salt, int iterationCount, SecureRandom random, AsymmetricKeyParameter key)
	{
		return CreateEncryptedPrivateKeyInfo(cipherAlgorithm, prfAlgorithm, passPhrase, salt, iterationCount, random, PrivateKeyInfoFactory.CreatePrivateKeyInfo(key));
	}

	public static EncryptedPrivateKeyInfo CreateEncryptedPrivateKeyInfo(DerObjectIdentifier cipherAlgorithm, DerObjectIdentifier prfAlgorithm, char[] passPhrase, byte[] salt, int iterationCount, SecureRandom random, PrivateKeyInfo keyInfo)
	{
		IBufferedCipher cipher = CipherUtilities.GetCipher(cipherAlgorithm);
		if (cipher == null)
		{
			throw new Exception("Unknown encryption algorithm: " + cipherAlgorithm);
		}
		Asn1Encodable asn1Encodable = PbeUtilities.GenerateAlgorithmParameters(cipherAlgorithm, prfAlgorithm, salt, iterationCount, random);
		ICipherParameters parameters = PbeUtilities.GenerateCipherParameters(PkcsObjectIdentifiers.IdPbeS2, passPhrase, asn1Encodable);
		cipher.Init(forEncryption: true, parameters);
		byte[] encoding = cipher.DoFinal(keyInfo.GetEncoded());
		AlgorithmIdentifier algId = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdPbeS2, asn1Encodable);
		return new EncryptedPrivateKeyInfo(algId, encoding);
	}
}
