using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms;

public class KeyTransRecipientInformation : RecipientInformation
{
	private KeyTransRecipientInfo info;

	internal KeyTransRecipientInformation(KeyTransRecipientInfo info, CmsSecureReadable secureReadable)
		: base(info.KeyEncryptionAlgorithm, secureReadable)
	{
		this.info = info;
		rid = new RecipientID();
		RecipientIdentifier recipientIdentifier = info.RecipientIdentifier;
		try
		{
			if (recipientIdentifier.IsTagged)
			{
				Asn1OctetString instance = Asn1OctetString.GetInstance(recipientIdentifier.ID);
				rid.SubjectKeyIdentifier = instance.GetOctets();
			}
			else
			{
				Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber instance2 = Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber.GetInstance(recipientIdentifier.ID);
				rid.Issuer = instance2.Name;
				rid.SerialNumber = instance2.SerialNumber.Value;
			}
		}
		catch (IOException)
		{
			throw new ArgumentException("invalid rid in KeyTransRecipientInformation");
		}
	}

	private string GetExchangeEncryptionAlgorithmName(AlgorithmIdentifier algo)
	{
		DerObjectIdentifier algorithm = algo.Algorithm;
		if (PkcsObjectIdentifiers.RsaEncryption.Equals(algorithm))
		{
			return "RSA//PKCS1Padding";
		}
		if (PkcsObjectIdentifiers.IdRsaesOaep.Equals(algorithm))
		{
			RsaesOaepParameters instance = RsaesOaepParameters.GetInstance(algo.Parameters);
			return "RSA//OAEPWITH" + DigestUtilities.GetAlgorithmName(instance.HashAlgorithm.Algorithm) + "ANDMGF1Padding";
		}
		return algorithm.Id;
	}

	internal KeyParameter UnwrapKey(ICipherParameters key)
	{
		byte[] octets = info.EncryptedKey.GetOctets();
		string exchangeEncryptionAlgorithmName = GetExchangeEncryptionAlgorithmName(keyEncAlg);
		try
		{
			IWrapper wrapper = WrapperUtilities.GetWrapper(exchangeEncryptionAlgorithmName);
			wrapper.Init(forWrapping: false, key);
			return ParameterUtilities.CreateKeyParameter(GetContentAlgorithmName(), wrapper.Unwrap(octets, 0, octets.Length));
		}
		catch (SecurityUtilityException e)
		{
			throw new CmsException("couldn't create cipher.", e);
		}
		catch (InvalidKeyException e2)
		{
			throw new CmsException("key invalid in message.", e2);
		}
		catch (DataLengthException e3)
		{
			throw new CmsException("illegal blocksize in message.", e3);
		}
		catch (InvalidCipherTextException e4)
		{
			throw new CmsException("bad padding in message.", e4);
		}
	}

	public override CmsTypedStream GetContentStream(ICipherParameters key)
	{
		KeyParameter sKey = UnwrapKey(key);
		return GetContentFromSessionKey(sKey);
	}
}
