using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpSecretKey
{
	private readonly SecretKeyPacket secret;

	private readonly PgpPublicKey pub;

	public bool IsSigningKey
	{
		get
		{
			switch (pub.Algorithm)
			{
			case PublicKeyAlgorithmTag.RsaGeneral:
			case PublicKeyAlgorithmTag.RsaSign:
			case PublicKeyAlgorithmTag.Dsa:
			case PublicKeyAlgorithmTag.ECDsa:
			case PublicKeyAlgorithmTag.ElGamalGeneral:
			case PublicKeyAlgorithmTag.EdDsa:
				return true;
			default:
				return false;
			}
		}
	}

	public bool IsMasterKey => pub.IsMasterKey;

	public bool IsPrivateKeyEmpty
	{
		get
		{
			byte[] secretKeyData = secret.GetSecretKeyData();
			if (secretKeyData != null)
			{
				return secretKeyData.Length < 1;
			}
			return true;
		}
	}

	public SymmetricKeyAlgorithmTag KeyEncryptionAlgorithm => secret.EncAlgorithm;

	public long KeyId => pub.KeyId;

	public int S2kUsage => secret.S2kUsage;

	public S2k S2k => secret.S2k;

	public PgpPublicKey PublicKey => pub;

	public IEnumerable UserIds => pub.GetUserIds();

	public IEnumerable UserAttributes => pub.GetUserAttributes();

	internal PgpSecretKey(SecretKeyPacket secret, PgpPublicKey pub)
	{
		this.secret = secret;
		this.pub = pub;
	}

	internal PgpSecretKey(PgpPrivateKey privKey, PgpPublicKey pubKey, SymmetricKeyAlgorithmTag encAlgorithm, byte[] rawPassPhrase, bool clearPassPhrase, bool useSha1, SecureRandom rand, bool isMasterKey)
	{
		pub = pubKey;
		BcpgObject bcpgObject;
		switch (pubKey.Algorithm)
		{
		case PublicKeyAlgorithmTag.RsaGeneral:
		case PublicKeyAlgorithmTag.RsaEncrypt:
		case PublicKeyAlgorithmTag.RsaSign:
		{
			RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters = (RsaPrivateCrtKeyParameters)privKey.Key;
			bcpgObject = new RsaSecretBcpgKey(rsaPrivateCrtKeyParameters.Exponent, rsaPrivateCrtKeyParameters.P, rsaPrivateCrtKeyParameters.Q);
			break;
		}
		case PublicKeyAlgorithmTag.Dsa:
		{
			DsaPrivateKeyParameters dsaPrivateKeyParameters = (DsaPrivateKeyParameters)privKey.Key;
			bcpgObject = new DsaSecretBcpgKey(dsaPrivateKeyParameters.X);
			break;
		}
		case PublicKeyAlgorithmTag.EC:
		case PublicKeyAlgorithmTag.ECDsa:
		{
			ECPrivateKeyParameters eCPrivateKeyParameters = (ECPrivateKeyParameters)privKey.Key;
			bcpgObject = new ECSecretBcpgKey(eCPrivateKeyParameters.D);
			break;
		}
		case PublicKeyAlgorithmTag.ElGamalEncrypt:
		case PublicKeyAlgorithmTag.ElGamalGeneral:
		{
			ElGamalPrivateKeyParameters elGamalPrivateKeyParameters = (ElGamalPrivateKeyParameters)privKey.Key;
			bcpgObject = new ElGamalSecretBcpgKey(elGamalPrivateKeyParameters.X);
			break;
		}
		default:
			throw new PgpException("unknown key class");
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
			bcpgOutputStream.WriteObject(bcpgObject);
			byte[] array = memoryStream.ToArray();
			byte[] b = Checksum(useSha1, array, array.Length);
			array = Arrays.Concatenate(array, b);
			if (encAlgorithm == SymmetricKeyAlgorithmTag.Null)
			{
				if (isMasterKey)
				{
					secret = new SecretKeyPacket(pub.publicPk, encAlgorithm, null, null, array);
				}
				else
				{
					secret = new SecretSubkeyPacket(pub.publicPk, encAlgorithm, null, null, array);
				}
				return;
			}
			S2k s2k;
			byte[] iv;
			byte[] secKeyData = ((pub.Version < 4) ? EncryptKeyDataV3(array, encAlgorithm, rawPassPhrase, clearPassPhrase, rand, out s2k, out iv) : EncryptKeyDataV4(array, encAlgorithm, HashAlgorithmTag.Sha1, rawPassPhrase, clearPassPhrase, rand, out s2k, out iv));
			int s2kUsage = (useSha1 ? 254 : 255);
			if (isMasterKey)
			{
				secret = new SecretKeyPacket(pub.publicPk, encAlgorithm, s2kUsage, s2k, iv, secKeyData);
			}
			else
			{
				secret = new SecretSubkeyPacket(pub.publicPk, encAlgorithm, s2kUsage, s2k, iv, secKeyData);
			}
		}
		catch (PgpException ex)
		{
			throw ex;
		}
		catch (Exception exception)
		{
			throw new PgpException("Exception encrypting key", exception);
		}
	}

	[Obsolete("Use the constructor taking an explicit 'useSha1' parameter instead")]
	public PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, char[] passPhrase, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, keyPair, id, encAlgorithm, passPhrase, useSha1: false, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, keyPair, id, encAlgorithm, utf8PassPhrase: false, passPhrase, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, bool utf8PassPhrase, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, keyPair, id, encAlgorithm, PgpUtilities.EncodePassPhrase(passPhrase, utf8PassPhrase), clearPassPhrase: true, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, byte[] rawPassPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, keyPair, id, encAlgorithm, rawPassPhrase, clearPassPhrase: false, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	internal PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, byte[] rawPassPhrase, bool clearPassPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(keyPair.PrivateKey, CertifiedPublicKey(certificationLevel, keyPair, id, hashedPackets, unhashedPackets), encAlgorithm, rawPassPhrase, clearPassPhrase, useSha1, rand, isMasterKey: true)
	{
	}

	public PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, HashAlgorithmTag hashAlgorithm, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, keyPair, id, encAlgorithm, hashAlgorithm, utf8PassPhrase: false, passPhrase, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, HashAlgorithmTag hashAlgorithm, bool utf8PassPhrase, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, keyPair, id, encAlgorithm, hashAlgorithm, PgpUtilities.EncodePassPhrase(passPhrase, utf8PassPhrase), clearPassPhrase: true, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, HashAlgorithmTag hashAlgorithm, byte[] rawPassPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, keyPair, id, encAlgorithm, hashAlgorithm, rawPassPhrase, clearPassPhrase: false, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	internal PgpSecretKey(int certificationLevel, PgpKeyPair keyPair, string id, SymmetricKeyAlgorithmTag encAlgorithm, HashAlgorithmTag hashAlgorithm, byte[] rawPassPhrase, bool clearPassPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(keyPair.PrivateKey, CertifiedPublicKey(certificationLevel, keyPair, id, hashedPackets, unhashedPackets, hashAlgorithm), encAlgorithm, rawPassPhrase, clearPassPhrase, useSha1, rand, isMasterKey: true)
	{
	}

	private static PgpPublicKey CertifiedPublicKey(int certificationLevel, PgpKeyPair keyPair, string id, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets)
	{
		PgpSignatureGenerator pgpSignatureGenerator;
		try
		{
			pgpSignatureGenerator = new PgpSignatureGenerator(keyPair.PublicKey.Algorithm, HashAlgorithmTag.Sha1);
		}
		catch (Exception ex)
		{
			throw new PgpException("Creating signature generator: " + ex.Message, ex);
		}
		pgpSignatureGenerator.InitSign(certificationLevel, keyPair.PrivateKey);
		pgpSignatureGenerator.SetHashedSubpackets(hashedPackets);
		pgpSignatureGenerator.SetUnhashedSubpackets(unhashedPackets);
		try
		{
			PgpSignature certification = pgpSignatureGenerator.GenerateCertification(id, keyPair.PublicKey);
			return PgpPublicKey.AddCertification(keyPair.PublicKey, id, certification);
		}
		catch (Exception ex2)
		{
			throw new PgpException("Exception doing certification: " + ex2.Message, ex2);
		}
	}

	private static PgpPublicKey CertifiedPublicKey(int certificationLevel, PgpKeyPair keyPair, string id, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, HashAlgorithmTag hashAlgorithm)
	{
		PgpSignatureGenerator pgpSignatureGenerator;
		try
		{
			pgpSignatureGenerator = new PgpSignatureGenerator(keyPair.PublicKey.Algorithm, hashAlgorithm);
		}
		catch (Exception ex)
		{
			throw new PgpException("Creating signature generator: " + ex.Message, ex);
		}
		pgpSignatureGenerator.InitSign(certificationLevel, keyPair.PrivateKey);
		pgpSignatureGenerator.SetHashedSubpackets(hashedPackets);
		pgpSignatureGenerator.SetUnhashedSubpackets(unhashedPackets);
		try
		{
			PgpSignature certification = pgpSignatureGenerator.GenerateCertification(id, keyPair.PublicKey);
			return PgpPublicKey.AddCertification(keyPair.PublicKey, id, certification);
		}
		catch (Exception ex2)
		{
			throw new PgpException("Exception doing certification: " + ex2.Message, ex2);
		}
	}

	public PgpSecretKey(int certificationLevel, PublicKeyAlgorithmTag algorithm, AsymmetricKeyParameter pubKey, AsymmetricKeyParameter privKey, DateTime time, string id, SymmetricKeyAlgorithmTag encAlgorithm, char[] passPhrase, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, new PgpKeyPair(algorithm, pubKey, privKey, time), id, encAlgorithm, passPhrase, useSha1: false, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpSecretKey(int certificationLevel, PublicKeyAlgorithmTag algorithm, AsymmetricKeyParameter pubKey, AsymmetricKeyParameter privKey, DateTime time, string id, SymmetricKeyAlgorithmTag encAlgorithm, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, new PgpKeyPair(algorithm, pubKey, privKey, time), id, encAlgorithm, passPhrase, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	private byte[] ExtractKeyData(byte[] rawPassPhrase, bool clearPassPhrase)
	{
		SymmetricKeyAlgorithmTag encAlgorithm = secret.EncAlgorithm;
		byte[] secretKeyData = secret.GetSecretKeyData();
		if (encAlgorithm == SymmetricKeyAlgorithmTag.Null)
		{
			return secretKeyData;
		}
		try
		{
			KeyParameter key = PgpUtilities.DoMakeKeyFromPassPhrase(secret.EncAlgorithm, secret.S2k, rawPassPhrase, clearPassPhrase);
			byte[] iV = secret.GetIV();
			byte[] array;
			if (secret.PublicKeyPacket.Version >= 4)
			{
				array = RecoverKeyData(encAlgorithm, "/CFB/NoPadding", key, iV, secretKeyData, 0, secretKeyData.Length);
				bool flag = secret.S2kUsage == 254;
				byte[] array2 = Checksum(flag, array, flag ? (array.Length - 20) : (array.Length - 2));
				for (int i = 0; i != array2.Length; i++)
				{
					if (array2[i] != array[array.Length - array2.Length + i])
					{
						throw new PgpException("Checksum mismatch at " + i + " of " + array2.Length);
					}
				}
			}
			else
			{
				array = new byte[secretKeyData.Length];
				iV = Arrays.Clone(iV);
				int num = 0;
				for (int j = 0; j != 4; j++)
				{
					int num2 = ((((secretKeyData[num] & 0xFF) << 8) | (secretKeyData[num + 1] & 0xFF)) + 7) / 8;
					array[num] = secretKeyData[num];
					array[num + 1] = secretKeyData[num + 1];
					num += 2;
					if (num2 > secretKeyData.Length - num)
					{
						throw new PgpException("out of range encLen found in encData");
					}
					byte[] sourceArray = RecoverKeyData(encAlgorithm, "/CFB/NoPadding", key, iV, secretKeyData, num, num2);
					Array.Copy(sourceArray, 0, array, num, num2);
					num += num2;
					if (j != 3)
					{
						Array.Copy(secretKeyData, num - iV.Length, iV, 0, iV.Length);
					}
				}
				array[num] = secretKeyData[num];
				array[num + 1] = secretKeyData[num + 1];
				int num3 = ((secretKeyData[num] << 8) & 0xFF00) | (secretKeyData[num + 1] & 0xFF);
				int num4 = 0;
				for (int k = 0; k < num; k++)
				{
					num4 += array[k] & 0xFF;
				}
				num4 &= 0xFFFF;
				if (num4 != num3)
				{
					throw new PgpException("Checksum mismatch: passphrase wrong, expected " + num3.ToString("X") + " found " + num4.ToString("X"));
				}
			}
			return array;
		}
		catch (PgpException ex)
		{
			throw ex;
		}
		catch (Exception exception)
		{
			throw new PgpException("Exception decrypting key", exception);
		}
	}

	private static byte[] RecoverKeyData(SymmetricKeyAlgorithmTag encAlgorithm, string modeAndPadding, KeyParameter key, byte[] iv, byte[] keyData, int keyOff, int keyLen)
	{
		IBufferedCipher cipher;
		try
		{
			string symmetricCipherName = PgpUtilities.GetSymmetricCipherName(encAlgorithm);
			cipher = CipherUtilities.GetCipher(symmetricCipherName + modeAndPadding);
		}
		catch (Exception exception)
		{
			throw new PgpException("Exception creating cipher", exception);
		}
		cipher.Init(forEncryption: false, new ParametersWithIV(key, iv));
		return cipher.DoFinal(keyData, keyOff, keyLen);
	}

	public PgpPrivateKey ExtractPrivateKey(char[] passPhrase)
	{
		return DoExtractPrivateKey(PgpUtilities.EncodePassPhrase(passPhrase, utf8: false), clearPassPhrase: true);
	}

	public PgpPrivateKey ExtractPrivateKeyUtf8(char[] passPhrase)
	{
		return DoExtractPrivateKey(PgpUtilities.EncodePassPhrase(passPhrase, utf8: true), clearPassPhrase: true);
	}

	public PgpPrivateKey ExtractPrivateKeyRaw(byte[] rawPassPhrase)
	{
		return DoExtractPrivateKey(rawPassPhrase, clearPassPhrase: false);
	}

	internal PgpPrivateKey DoExtractPrivateKey(byte[] rawPassPhrase, bool clearPassPhrase)
	{
		if (IsPrivateKeyEmpty)
		{
			return null;
		}
		PublicKeyPacket publicKeyPacket = secret.PublicKeyPacket;
		try
		{
			byte[] buffer = ExtractKeyData(rawPassPhrase, clearPassPhrase);
			BcpgInputStream bcpgIn = BcpgInputStream.Wrap(new MemoryStream(buffer, writable: false));
			AsymmetricKeyParameter privateKey;
			switch (publicKeyPacket.Algorithm)
			{
			case PublicKeyAlgorithmTag.RsaGeneral:
			case PublicKeyAlgorithmTag.RsaEncrypt:
			case PublicKeyAlgorithmTag.RsaSign:
			{
				RsaPublicBcpgKey rsaPublicBcpgKey = (RsaPublicBcpgKey)publicKeyPacket.Key;
				RsaSecretBcpgKey rsaSecretBcpgKey = new RsaSecretBcpgKey(bcpgIn);
				RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters = new RsaPrivateCrtKeyParameters(rsaSecretBcpgKey.Modulus, rsaPublicBcpgKey.PublicExponent, rsaSecretBcpgKey.PrivateExponent, rsaSecretBcpgKey.PrimeP, rsaSecretBcpgKey.PrimeQ, rsaSecretBcpgKey.PrimeExponentP, rsaSecretBcpgKey.PrimeExponentQ, rsaSecretBcpgKey.CrtCoefficient);
				privateKey = rsaPrivateCrtKeyParameters;
				break;
			}
			case PublicKeyAlgorithmTag.Dsa:
			{
				DsaPublicBcpgKey dsaPublicBcpgKey = (DsaPublicBcpgKey)publicKeyPacket.Key;
				DsaSecretBcpgKey dsaSecretBcpgKey = new DsaSecretBcpgKey(bcpgIn);
				DsaParameters parameters2 = new DsaParameters(dsaPublicBcpgKey.P, dsaPublicBcpgKey.Q, dsaPublicBcpgKey.G);
				privateKey = new DsaPrivateKeyParameters(dsaSecretBcpgKey.X, parameters2);
				break;
			}
			case PublicKeyAlgorithmTag.EC:
				privateKey = GetECKey("ECDH", bcpgIn);
				break;
			case PublicKeyAlgorithmTag.ECDsa:
				privateKey = GetECKey("ECDSA", bcpgIn);
				break;
			case PublicKeyAlgorithmTag.ElGamalEncrypt:
			case PublicKeyAlgorithmTag.ElGamalGeneral:
			{
				ElGamalPublicBcpgKey elGamalPublicBcpgKey = (ElGamalPublicBcpgKey)publicKeyPacket.Key;
				ElGamalSecretBcpgKey elGamalSecretBcpgKey = new ElGamalSecretBcpgKey(bcpgIn);
				ElGamalParameters parameters = new ElGamalParameters(elGamalPublicBcpgKey.P, elGamalPublicBcpgKey.G);
				privateKey = new ElGamalPrivateKeyParameters(elGamalSecretBcpgKey.X, parameters);
				break;
			}
			default:
				throw new PgpException("unknown public key algorithm encountered");
			}
			return new PgpPrivateKey(KeyId, publicKeyPacket, privateKey);
		}
		catch (PgpException ex)
		{
			throw ex;
		}
		catch (Exception exception)
		{
			throw new PgpException("Exception constructing key", exception);
		}
	}

	private ECPrivateKeyParameters GetECKey(string algorithm, BcpgInputStream bcpgIn)
	{
		ECPublicBcpgKey eCPublicBcpgKey = (ECPublicBcpgKey)secret.PublicKeyPacket.Key;
		ECSecretBcpgKey eCSecretBcpgKey = new ECSecretBcpgKey(bcpgIn);
		return new ECPrivateKeyParameters(algorithm, eCSecretBcpgKey.X, eCPublicBcpgKey.CurveOid);
	}

	private static byte[] Checksum(bool useSha1, byte[] bytes, int length)
	{
		if (useSha1)
		{
			try
			{
				IDigest digest = DigestUtilities.GetDigest("SHA1");
				digest.BlockUpdate(bytes, 0, length);
				return DigestUtilities.DoFinal(digest);
			}
			catch (Exception exception)
			{
				throw new PgpException("Can't find SHA-1", exception);
			}
		}
		int num = 0;
		for (int i = 0; i != length; i++)
		{
			num += bytes[i];
		}
		return new byte[2]
		{
			(byte)(num >> 8),
			(byte)num
		};
	}

	public byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public void Encode(Stream outStr)
	{
		BcpgOutputStream bcpgOutputStream = BcpgOutputStream.Wrap(outStr);
		bcpgOutputStream.WritePacket(secret);
		if (pub.trustPk != null)
		{
			bcpgOutputStream.WritePacket(pub.trustPk);
		}
		if (pub.subSigs == null)
		{
			foreach (PgpSignature keySig in pub.keySigs)
			{
				keySig.Encode(bcpgOutputStream);
			}
			for (int i = 0; i != pub.ids.Count; i++)
			{
				object obj = pub.ids[i];
				if (obj is string)
				{
					string id = (string)obj;
					bcpgOutputStream.WritePacket(new UserIdPacket(id));
				}
				else
				{
					PgpUserAttributeSubpacketVector pgpUserAttributeSubpacketVector = (PgpUserAttributeSubpacketVector)obj;
					bcpgOutputStream.WritePacket(new UserAttributePacket(pgpUserAttributeSubpacketVector.ToSubpacketArray()));
				}
				if (pub.idTrusts[i] != null)
				{
					bcpgOutputStream.WritePacket((ContainedPacket)pub.idTrusts[i]);
				}
				foreach (PgpSignature item in (IList)pub.idSigs[i])
				{
					item.Encode(bcpgOutputStream);
				}
			}
			return;
		}
		foreach (PgpSignature subSig in pub.subSigs)
		{
			subSig.Encode(bcpgOutputStream);
		}
	}

	public static PgpSecretKey CopyWithNewPassword(PgpSecretKey key, char[] oldPassPhrase, char[] newPassPhrase, SymmetricKeyAlgorithmTag newEncAlgorithm, SecureRandom rand)
	{
		return DoCopyWithNewPassword(key, PgpUtilities.EncodePassPhrase(oldPassPhrase, utf8: false), PgpUtilities.EncodePassPhrase(newPassPhrase, utf8: false), clearPassPhrase: true, newEncAlgorithm, rand);
	}

	public static PgpSecretKey CopyWithNewPasswordUtf8(PgpSecretKey key, char[] oldPassPhrase, char[] newPassPhrase, SymmetricKeyAlgorithmTag newEncAlgorithm, SecureRandom rand)
	{
		return DoCopyWithNewPassword(key, PgpUtilities.EncodePassPhrase(oldPassPhrase, utf8: true), PgpUtilities.EncodePassPhrase(newPassPhrase, utf8: true), clearPassPhrase: true, newEncAlgorithm, rand);
	}

	public static PgpSecretKey CopyWithNewPasswordRaw(PgpSecretKey key, byte[] rawOldPassPhrase, byte[] rawNewPassPhrase, SymmetricKeyAlgorithmTag newEncAlgorithm, SecureRandom rand)
	{
		return DoCopyWithNewPassword(key, rawOldPassPhrase, rawNewPassPhrase, clearPassPhrase: false, newEncAlgorithm, rand);
	}

	internal static PgpSecretKey DoCopyWithNewPassword(PgpSecretKey key, byte[] rawOldPassPhrase, byte[] rawNewPassPhrase, bool clearPassPhrase, SymmetricKeyAlgorithmTag newEncAlgorithm, SecureRandom rand)
	{
		if (key.IsPrivateKeyEmpty)
		{
			throw new PgpException("no private key in this SecretKey - public key present only.");
		}
		byte[] array = key.ExtractKeyData(rawOldPassPhrase, clearPassPhrase);
		int num = key.secret.S2kUsage;
		byte[] iv = null;
		S2k s2k = null;
		PublicKeyPacket publicKeyPacket = key.secret.PublicKeyPacket;
		byte[] array2;
		if (newEncAlgorithm == SymmetricKeyAlgorithmTag.Null)
		{
			num = 0;
			if (key.secret.S2kUsage == 254)
			{
				array2 = new byte[array.Length - 18];
				Array.Copy(array, 0, array2, 0, array2.Length - 2);
				byte[] array3 = Checksum(useSha1: false, array2, array2.Length - 2);
				array2[array2.Length - 2] = array3[0];
				array2[array2.Length - 1] = array3[1];
			}
			else
			{
				array2 = array;
			}
		}
		else
		{
			if (num == 0)
			{
				num = 255;
			}
			try
			{
				array2 = ((publicKeyPacket.Version < 4) ? EncryptKeyDataV3(array, newEncAlgorithm, rawNewPassPhrase, clearPassPhrase, rand, out s2k, out iv) : EncryptKeyDataV4(array, newEncAlgorithm, HashAlgorithmTag.Sha1, rawNewPassPhrase, clearPassPhrase, rand, out s2k, out iv));
			}
			catch (PgpException ex)
			{
				throw ex;
			}
			catch (Exception exception)
			{
				throw new PgpException("Exception encrypting key", exception);
			}
		}
		SecretKeyPacket secretKeyPacket = ((!(key.secret is SecretSubkeyPacket)) ? new SecretKeyPacket(publicKeyPacket, newEncAlgorithm, num, s2k, iv, array2) : new SecretSubkeyPacket(publicKeyPacket, newEncAlgorithm, num, s2k, iv, array2));
		return new PgpSecretKey(secretKeyPacket, key.pub);
	}

	public static PgpSecretKey ReplacePublicKey(PgpSecretKey secretKey, PgpPublicKey publicKey)
	{
		if (publicKey.KeyId != secretKey.KeyId)
		{
			throw new ArgumentException("KeyId's do not match");
		}
		return new PgpSecretKey(secretKey.secret, publicKey);
	}

	private static byte[] EncryptKeyDataV3(byte[] rawKeyData, SymmetricKeyAlgorithmTag encAlgorithm, byte[] rawPassPhrase, bool clearPassPhrase, SecureRandom random, out S2k s2k, out byte[] iv)
	{
		s2k = null;
		iv = null;
		KeyParameter key = PgpUtilities.DoMakeKeyFromPassPhrase(encAlgorithm, s2k, rawPassPhrase, clearPassPhrase);
		byte[] array = new byte[rawKeyData.Length];
		int num = 0;
		for (int i = 0; i != 4; i++)
		{
			int num2 = ((((rawKeyData[num] & 0xFF) << 8) | (rawKeyData[num + 1] & 0xFF)) + 7) / 8;
			array[num] = rawKeyData[num];
			array[num + 1] = rawKeyData[num + 1];
			if (num2 > rawKeyData.Length - (num + 2))
			{
				throw new PgpException("out of range encLen found in rawKeyData");
			}
			byte[] array2;
			if (i == 0)
			{
				array2 = EncryptData(encAlgorithm, key, rawKeyData, num + 2, num2, random, ref iv);
			}
			else
			{
				byte[] iv2 = Arrays.CopyOfRange(array, num - iv.Length, num);
				array2 = EncryptData(encAlgorithm, key, rawKeyData, num + 2, num2, random, ref iv2);
			}
			Array.Copy(array2, 0, array, num + 2, array2.Length);
			num += 2 + num2;
		}
		array[num] = rawKeyData[num];
		array[num + 1] = rawKeyData[num + 1];
		return array;
	}

	private static byte[] EncryptKeyDataV4(byte[] rawKeyData, SymmetricKeyAlgorithmTag encAlgorithm, HashAlgorithmTag hashAlgorithm, byte[] rawPassPhrase, bool clearPassPhrase, SecureRandom random, out S2k s2k, out byte[] iv)
	{
		s2k = PgpUtilities.GenerateS2k(hashAlgorithm, 96, random);
		KeyParameter key = PgpUtilities.DoMakeKeyFromPassPhrase(encAlgorithm, s2k, rawPassPhrase, clearPassPhrase);
		iv = null;
		return EncryptData(encAlgorithm, key, rawKeyData, 0, rawKeyData.Length, random, ref iv);
	}

	private static byte[] EncryptData(SymmetricKeyAlgorithmTag encAlgorithm, KeyParameter key, byte[] data, int dataOff, int dataLen, SecureRandom random, ref byte[] iv)
	{
		IBufferedCipher cipher;
		try
		{
			string symmetricCipherName = PgpUtilities.GetSymmetricCipherName(encAlgorithm);
			cipher = CipherUtilities.GetCipher(symmetricCipherName + "/CFB/NoPadding");
		}
		catch (Exception exception)
		{
			throw new PgpException("Exception creating cipher", exception);
		}
		if (iv == null)
		{
			iv = PgpUtilities.GenerateIV(cipher.GetBlockSize(), random);
		}
		cipher.Init(forEncryption: true, new ParametersWithRandom(new ParametersWithIV(key, iv), random));
		return cipher.DoFinal(data, dataOff, dataLen);
	}

	public static PgpSecretKey ParseSecretKeyFromSExpr(Stream inputStream, char[] passPhrase, PgpPublicKey pubKey)
	{
		return DoParseSecretKeyFromSExpr(inputStream, PgpUtilities.EncodePassPhrase(passPhrase, utf8: false), clearPassPhrase: true, pubKey);
	}

	public static PgpSecretKey ParseSecretKeyFromSExprUtf8(Stream inputStream, char[] passPhrase, PgpPublicKey pubKey)
	{
		return DoParseSecretKeyFromSExpr(inputStream, PgpUtilities.EncodePassPhrase(passPhrase, utf8: true), clearPassPhrase: true, pubKey);
	}

	public static PgpSecretKey ParseSecretKeyFromSExprRaw(Stream inputStream, byte[] rawPassPhrase, PgpPublicKey pubKey)
	{
		return DoParseSecretKeyFromSExpr(inputStream, rawPassPhrase, clearPassPhrase: false, pubKey);
	}

	internal static PgpSecretKey DoParseSecretKeyFromSExpr(Stream inputStream, byte[] rawPassPhrase, bool clearPassPhrase, PgpPublicKey pubKey)
	{
		SXprUtilities.SkipOpenParenthesis(inputStream);
		string text = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
		if (text.Equals("protected-private-key"))
		{
			SXprUtilities.SkipOpenParenthesis(inputStream);
			string text2 = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
			if (text2.Equals("ecc"))
			{
				SXprUtilities.SkipOpenParenthesis(inputStream);
				SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
				string curveName = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
				SXprUtilities.SkipCloseParenthesis(inputStream);
				SXprUtilities.SkipOpenParenthesis(inputStream);
				text = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
				if (text.Equals("q"))
				{
					SXprUtilities.ReadBytes(inputStream, inputStream.ReadByte());
					SXprUtilities.SkipCloseParenthesis(inputStream);
					byte[] dValue = GetDValue(inputStream, rawPassPhrase, clearPassPhrase, curveName);
					return new PgpSecretKey(new SecretKeyPacket(pubKey.PublicKeyPacket, SymmetricKeyAlgorithmTag.Null, null, null, new ECSecretBcpgKey(new BigInteger(1, dValue)).GetEncoded()), pubKey);
				}
				throw new PgpException("no q value found");
			}
			throw new PgpException("no curve details found");
		}
		throw new PgpException("unknown key type found");
	}

	public static PgpSecretKey ParseSecretKeyFromSExpr(Stream inputStream, char[] passPhrase)
	{
		return DoParseSecretKeyFromSExpr(inputStream, PgpUtilities.EncodePassPhrase(passPhrase, utf8: false), clearPassPhrase: true);
	}

	public static PgpSecretKey ParseSecretKeyFromSExprUtf8(Stream inputStream, char[] passPhrase)
	{
		return DoParseSecretKeyFromSExpr(inputStream, PgpUtilities.EncodePassPhrase(passPhrase, utf8: true), clearPassPhrase: true);
	}

	public static PgpSecretKey ParseSecretKeyFromSExprRaw(Stream inputStream, byte[] rawPassPhrase)
	{
		return DoParseSecretKeyFromSExpr(inputStream, rawPassPhrase, clearPassPhrase: false);
	}

	internal static PgpSecretKey DoParseSecretKeyFromSExpr(Stream inputStream, byte[] rawPassPhrase, bool clearPassPhrase)
	{
		SXprUtilities.SkipOpenParenthesis(inputStream);
		string text = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
		if (text.Equals("protected-private-key"))
		{
			SXprUtilities.SkipOpenParenthesis(inputStream);
			string text2 = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
			if (text2.Equals("ecc"))
			{
				SXprUtilities.SkipOpenParenthesis(inputStream);
				SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
				string text3 = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
				if (Platform.StartsWith(text3, "NIST "))
				{
					text3 = text3.Substring("NIST ".Length);
				}
				SXprUtilities.SkipCloseParenthesis(inputStream);
				SXprUtilities.SkipOpenParenthesis(inputStream);
				text = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
				if (text.Equals("q"))
				{
					byte[] bytes = SXprUtilities.ReadBytes(inputStream, inputStream.ReadByte());
					PublicKeyPacket publicKeyPacket = new PublicKeyPacket(PublicKeyAlgorithmTag.ECDsa, DateTime.UtcNow, new ECDsaPublicBcpgKey(ECNamedCurveTable.GetOid(text3), new BigInteger(1, bytes)));
					SXprUtilities.SkipCloseParenthesis(inputStream);
					byte[] dValue = GetDValue(inputStream, rawPassPhrase, clearPassPhrase, text3);
					return new PgpSecretKey(new SecretKeyPacket(publicKeyPacket, SymmetricKeyAlgorithmTag.Null, null, null, new ECSecretBcpgKey(new BigInteger(1, dValue)).GetEncoded()), new PgpPublicKey(publicKeyPacket));
				}
				throw new PgpException("no q value found");
			}
			throw new PgpException("no curve details found");
		}
		throw new PgpException("unknown key type found");
	}

	private static byte[] GetDValue(Stream inputStream, byte[] rawPassPhrase, bool clearPassPhrase, string curveName)
	{
		SXprUtilities.SkipOpenParenthesis(inputStream);
		string text = SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
		if (text.Equals("protected"))
		{
			SXprUtilities.ReadString(inputStream, inputStream.ReadByte());
			SXprUtilities.SkipOpenParenthesis(inputStream);
			S2k s2k = SXprUtilities.ParseS2k(inputStream);
			byte[] iv = SXprUtilities.ReadBytes(inputStream, inputStream.ReadByte());
			SXprUtilities.SkipCloseParenthesis(inputStream);
			byte[] array = SXprUtilities.ReadBytes(inputStream, inputStream.ReadByte());
			KeyParameter key = PgpUtilities.DoMakeKeyFromPassPhrase(SymmetricKeyAlgorithmTag.Aes128, s2k, rawPassPhrase, clearPassPhrase);
			byte[] buffer = RecoverKeyData(SymmetricKeyAlgorithmTag.Aes128, "/CBC/NoPadding", key, iv, array, 0, array.Length);
			Stream stream = new MemoryStream(buffer, writable: false);
			SXprUtilities.SkipOpenParenthesis(stream);
			SXprUtilities.SkipOpenParenthesis(stream);
			SXprUtilities.SkipOpenParenthesis(stream);
			SXprUtilities.ReadString(stream, stream.ReadByte());
			return SXprUtilities.ReadBytes(stream, stream.ReadByte());
		}
		throw new PgpException("protected block not found");
	}
}
