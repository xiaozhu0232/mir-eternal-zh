using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Cms.Ecc;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms;

internal class KeyAgreeRecipientInfoGenerator : RecipientInfoGenerator
{
	private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

	private DerObjectIdentifier keyAgreementOID;

	private DerObjectIdentifier keyEncryptionOID;

	private IList recipientCerts;

	private AsymmetricCipherKeyPair senderKeyPair;

	internal DerObjectIdentifier KeyAgreementOID
	{
		set
		{
			keyAgreementOID = value;
		}
	}

	internal DerObjectIdentifier KeyEncryptionOID
	{
		set
		{
			keyEncryptionOID = value;
		}
	}

	internal ICollection RecipientCerts
	{
		set
		{
			recipientCerts = Platform.CreateArrayList(value);
		}
	}

	internal AsymmetricCipherKeyPair SenderKeyPair
	{
		set
		{
			senderKeyPair = value;
		}
	}

	internal KeyAgreeRecipientInfoGenerator()
	{
	}

	public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
	{
		byte[] key = contentEncryptionKey.GetKey();
		AsymmetricKeyParameter @public = senderKeyPair.Public;
		ICipherParameters cipherParameters = senderKeyPair.Private;
		OriginatorIdentifierOrKey originator;
		try
		{
			originator = new OriginatorIdentifierOrKey(CreateOriginatorPublicKey(@public));
		}
		catch (IOException ex)
		{
			throw new InvalidKeyException("cannot extract originator public key: " + ex);
		}
		Asn1OctetString ukm = null;
		if (keyAgreementOID.Id.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
		{
			try
			{
				IAsymmetricCipherKeyPairGenerator keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator(keyAgreementOID);
				keyPairGenerator.Init(((ECPublicKeyParameters)@public).CreateKeyGenerationParameters(random));
				AsymmetricCipherKeyPair asymmetricCipherKeyPair = keyPairGenerator.GenerateKeyPair();
				ukm = new DerOctetString(new MQVuserKeyingMaterial(CreateOriginatorPublicKey(asymmetricCipherKeyPair.Public), null));
				cipherParameters = new MqvPrivateParameters((ECPrivateKeyParameters)cipherParameters, (ECPrivateKeyParameters)asymmetricCipherKeyPair.Private, (ECPublicKeyParameters)asymmetricCipherKeyPair.Public);
			}
			catch (IOException ex2)
			{
				throw new InvalidKeyException("cannot extract MQV ephemeral public key: " + ex2);
			}
			catch (SecurityUtilityException ex3)
			{
				throw new InvalidKeyException("cannot determine MQV ephemeral key pair parameters from public key: " + ex3);
			}
		}
		DerSequence parameters = new DerSequence(keyEncryptionOID, DerNull.Instance);
		AlgorithmIdentifier keyEncryptionAlgorithm = new AlgorithmIdentifier(keyAgreementOID, parameters);
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (X509Certificate recipientCert in recipientCerts)
		{
			TbsCertificateStructure instance;
			try
			{
				instance = TbsCertificateStructure.GetInstance(Asn1Object.FromByteArray(recipientCert.GetTbsCertificate()));
			}
			catch (Exception)
			{
				throw new ArgumentException("can't extract TBS structure from certificate");
			}
			IssuerAndSerialNumber issuerSerial = new IssuerAndSerialNumber(instance.Issuer, instance.SerialNumber.Value);
			KeyAgreeRecipientIdentifier id = new KeyAgreeRecipientIdentifier(issuerSerial);
			ICipherParameters cipherParameters2 = recipientCert.GetPublicKey();
			if (keyAgreementOID.Id.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
			{
				cipherParameters2 = new MqvPublicParameters((ECPublicKeyParameters)cipherParameters2, (ECPublicKeyParameters)cipherParameters2);
			}
			IBasicAgreement basicAgreementWithKdf = AgreementUtilities.GetBasicAgreementWithKdf(keyAgreementOID, keyEncryptionOID.Id);
			basicAgreementWithKdf.Init(new ParametersWithRandom(cipherParameters, random));
			BigInteger s = basicAgreementWithKdf.CalculateAgreement(cipherParameters2);
			int qLength = GeneratorUtilities.GetDefaultKeySize(keyEncryptionOID) / 8;
			byte[] keyBytes = X9IntegerConverter.IntegerToBytes(s, qLength);
			KeyParameter parameters2 = ParameterUtilities.CreateKeyParameter(keyEncryptionOID, keyBytes);
			IWrapper wrapper = Helper.CreateWrapper(keyEncryptionOID.Id);
			wrapper.Init(forWrapping: true, new ParametersWithRandom(parameters2, random));
			byte[] str = wrapper.Wrap(key, 0, key.Length);
			Asn1OctetString encryptedKey = new DerOctetString(str);
			asn1EncodableVector.Add(new RecipientEncryptedKey(id, encryptedKey));
		}
		return new RecipientInfo(new KeyAgreeRecipientInfo(originator, ukm, keyEncryptionAlgorithm, new DerSequence(asn1EncodableVector)));
	}

	private static OriginatorPublicKey CreateOriginatorPublicKey(AsymmetricKeyParameter publicKey)
	{
		SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
		return new OriginatorPublicKey(new AlgorithmIdentifier(subjectPublicKeyInfo.AlgorithmID.Algorithm, DerNull.Instance), subjectPublicKeyInfo.PublicKeyData.GetBytes());
	}
}
