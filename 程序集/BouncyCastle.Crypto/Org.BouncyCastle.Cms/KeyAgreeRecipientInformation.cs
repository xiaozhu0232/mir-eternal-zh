using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Cms.Ecc;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms;

public class KeyAgreeRecipientInformation : RecipientInformation
{
	private KeyAgreeRecipientInfo info;

	private Asn1OctetString encryptedKey;

	internal static void ReadRecipientInfo(IList infos, KeyAgreeRecipientInfo info, CmsSecureReadable secureReadable)
	{
		try
		{
			foreach (Asn1Encodable recipientEncryptedKey in info.RecipientEncryptedKeys)
			{
				RecipientEncryptedKey instance = RecipientEncryptedKey.GetInstance(recipientEncryptedKey.ToAsn1Object());
				RecipientID recipientID = new RecipientID();
				KeyAgreeRecipientIdentifier identifier = instance.Identifier;
				Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber issuerAndSerialNumber = identifier.IssuerAndSerialNumber;
				if (issuerAndSerialNumber != null)
				{
					recipientID.Issuer = issuerAndSerialNumber.Name;
					recipientID.SerialNumber = issuerAndSerialNumber.SerialNumber.Value;
				}
				else
				{
					RecipientKeyIdentifier rKeyID = identifier.RKeyID;
					recipientID.SubjectKeyIdentifier = rKeyID.SubjectKeyIdentifier.GetOctets();
				}
				infos.Add(new KeyAgreeRecipientInformation(info, recipientID, instance.EncryptedKey, secureReadable));
			}
		}
		catch (IOException innerException)
		{
			throw new ArgumentException("invalid rid in KeyAgreeRecipientInformation", innerException);
		}
	}

	internal KeyAgreeRecipientInformation(KeyAgreeRecipientInfo info, RecipientID rid, Asn1OctetString encryptedKey, CmsSecureReadable secureReadable)
		: base(info.KeyEncryptionAlgorithm, secureReadable)
	{
		this.info = info;
		base.rid = rid;
		this.encryptedKey = encryptedKey;
	}

	private AsymmetricKeyParameter GetSenderPublicKey(AsymmetricKeyParameter receiverPrivateKey, OriginatorIdentifierOrKey originator)
	{
		OriginatorPublicKey originatorPublicKey = originator.OriginatorPublicKey;
		if (originatorPublicKey != null)
		{
			return GetPublicKeyFromOriginatorPublicKey(receiverPrivateKey, originatorPublicKey);
		}
		OriginatorID originatorID = new OriginatorID();
		Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber issuerAndSerialNumber = originator.IssuerAndSerialNumber;
		if (issuerAndSerialNumber != null)
		{
			originatorID.Issuer = issuerAndSerialNumber.Name;
			originatorID.SerialNumber = issuerAndSerialNumber.SerialNumber.Value;
		}
		else
		{
			SubjectKeyIdentifier subjectKeyIdentifier = originator.SubjectKeyIdentifier;
			originatorID.SubjectKeyIdentifier = subjectKeyIdentifier.GetKeyIdentifier();
		}
		return GetPublicKeyFromOriginatorID(originatorID);
	}

	private AsymmetricKeyParameter GetPublicKeyFromOriginatorPublicKey(AsymmetricKeyParameter receiverPrivateKey, OriginatorPublicKey originatorPublicKey)
	{
		PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(receiverPrivateKey);
		SubjectPublicKeyInfo keyInfo = new SubjectPublicKeyInfo(privateKeyInfo.PrivateKeyAlgorithm, originatorPublicKey.PublicKey.GetBytes());
		return PublicKeyFactory.CreateKey(keyInfo);
	}

	private AsymmetricKeyParameter GetPublicKeyFromOriginatorID(OriginatorID origID)
	{
		throw new CmsException("No support for 'originator' as IssuerAndSerialNumber or SubjectKeyIdentifier");
	}

	private KeyParameter CalculateAgreedWrapKey(string wrapAlg, AsymmetricKeyParameter senderPublicKey, AsymmetricKeyParameter receiverPrivateKey)
	{
		DerObjectIdentifier algorithm = keyEncAlg.Algorithm;
		ICipherParameters cipherParameters = senderPublicKey;
		ICipherParameters cipherParameters2 = receiverPrivateKey;
		if (algorithm.Id.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
		{
			byte[] octets = info.UserKeyingMaterial.GetOctets();
			MQVuserKeyingMaterial instance = MQVuserKeyingMaterial.GetInstance(Asn1Object.FromByteArray(octets));
			AsymmetricKeyParameter publicKeyFromOriginatorPublicKey = GetPublicKeyFromOriginatorPublicKey(receiverPrivateKey, instance.EphemeralPublicKey);
			cipherParameters = new MqvPublicParameters((ECPublicKeyParameters)cipherParameters, (ECPublicKeyParameters)publicKeyFromOriginatorPublicKey);
			cipherParameters2 = new MqvPrivateParameters((ECPrivateKeyParameters)cipherParameters2, (ECPrivateKeyParameters)cipherParameters2);
		}
		IBasicAgreement basicAgreementWithKdf = AgreementUtilities.GetBasicAgreementWithKdf(algorithm, wrapAlg);
		basicAgreementWithKdf.Init(cipherParameters2);
		BigInteger s = basicAgreementWithKdf.CalculateAgreement(cipherParameters);
		int qLength = GeneratorUtilities.GetDefaultKeySize(wrapAlg) / 8;
		byte[] keyBytes = X9IntegerConverter.IntegerToBytes(s, qLength);
		return ParameterUtilities.CreateKeyParameter(wrapAlg, keyBytes);
	}

	private KeyParameter UnwrapSessionKey(string wrapAlg, KeyParameter agreedKey)
	{
		byte[] octets = encryptedKey.GetOctets();
		IWrapper wrapper = WrapperUtilities.GetWrapper(wrapAlg);
		wrapper.Init(forWrapping: false, agreedKey);
		byte[] keyBytes = wrapper.Unwrap(octets, 0, octets.Length);
		return ParameterUtilities.CreateKeyParameter(GetContentAlgorithmName(), keyBytes);
	}

	internal KeyParameter GetSessionKey(AsymmetricKeyParameter receiverPrivateKey)
	{
		try
		{
			string id = DerObjectIdentifier.GetInstance(Asn1Sequence.GetInstance(keyEncAlg.Parameters)[0]).Id;
			AsymmetricKeyParameter senderPublicKey = GetSenderPublicKey(receiverPrivateKey, info.Originator);
			KeyParameter agreedKey = CalculateAgreedWrapKey(id, senderPublicKey, receiverPrivateKey);
			return UnwrapSessionKey(id, agreedKey);
		}
		catch (SecurityUtilityException e)
		{
			throw new CmsException("couldn't create cipher.", e);
		}
		catch (InvalidKeyException e2)
		{
			throw new CmsException("key invalid in message.", e2);
		}
		catch (Exception e3)
		{
			throw new CmsException("originator key invalid.", e3);
		}
	}

	public override CmsTypedStream GetContentStream(ICipherParameters key)
	{
		if (!(key is AsymmetricKeyParameter))
		{
			throw new ArgumentException("KeyAgreement requires asymmetric key", "key");
		}
		AsymmetricKeyParameter asymmetricKeyParameter = (AsymmetricKeyParameter)key;
		if (!asymmetricKeyParameter.IsPrivate)
		{
			throw new ArgumentException("Expected private key", "key");
		}
		KeyParameter sessionKey = GetSessionKey(asymmetricKeyParameter);
		return GetContentFromSessionKey(sessionKey);
	}
}
