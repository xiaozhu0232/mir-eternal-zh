using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Cms;

internal class CmsEnvelopedHelper
{
	internal class CmsAuthenticatedSecureReadable : CmsSecureReadable
	{
		private AlgorithmIdentifier algorithm;

		private IMac mac;

		private CmsReadable readable;

		public AlgorithmIdentifier Algorithm => algorithm;

		public object CryptoObject => mac;

		internal CmsAuthenticatedSecureReadable(AlgorithmIdentifier algorithm, CmsReadable readable)
		{
			this.algorithm = algorithm;
			this.readable = readable;
		}

		public CmsReadable GetReadable(KeyParameter sKey)
		{
			string id = algorithm.Algorithm.Id;
			try
			{
				mac = MacUtilities.GetMac(id);
				mac.Init(sKey);
			}
			catch (SecurityUtilityException e)
			{
				throw new CmsException("couldn't create cipher.", e);
			}
			catch (InvalidKeyException e2)
			{
				throw new CmsException("key invalid in message.", e2);
			}
			catch (IOException e3)
			{
				throw new CmsException("error decoding algorithm parameters.", e3);
			}
			try
			{
				return new CmsProcessableInputStream(new TeeInputStream(readable.GetInputStream(), new MacSink(mac)));
			}
			catch (IOException e4)
			{
				throw new CmsException("error reading content.", e4);
			}
		}
	}

	internal class CmsEnvelopedSecureReadable : CmsSecureReadable
	{
		private AlgorithmIdentifier algorithm;

		private IBufferedCipher cipher;

		private CmsReadable readable;

		public AlgorithmIdentifier Algorithm => algorithm;

		public object CryptoObject => cipher;

		internal CmsEnvelopedSecureReadable(AlgorithmIdentifier algorithm, CmsReadable readable)
		{
			this.algorithm = algorithm;
			this.readable = readable;
		}

		public CmsReadable GetReadable(KeyParameter sKey)
		{
			try
			{
				cipher = CipherUtilities.GetCipher(algorithm.Algorithm);
				Asn1Object asn1Object = algorithm.Parameters?.ToAsn1Object();
				ICipherParameters cipherParameters = sKey;
				if (asn1Object != null && !(asn1Object is Asn1Null))
				{
					cipherParameters = ParameterUtilities.GetCipherParameters(algorithm.Algorithm, cipherParameters, asn1Object);
				}
				else
				{
					string id = algorithm.Algorithm.Id;
					if (id.Equals(CmsEnvelopedGenerator.DesEde3Cbc) || id.Equals("1.3.6.1.4.1.188.7.1.1.2") || id.Equals("1.2.840.113533.7.66.10"))
					{
						cipherParameters = new ParametersWithIV(cipherParameters, new byte[8]);
					}
				}
				cipher.Init(forEncryption: false, cipherParameters);
			}
			catch (SecurityUtilityException e)
			{
				throw new CmsException("couldn't create cipher.", e);
			}
			catch (InvalidKeyException e2)
			{
				throw new CmsException("key invalid in message.", e2);
			}
			catch (IOException e3)
			{
				throw new CmsException("error decoding algorithm parameters.", e3);
			}
			try
			{
				return new CmsProcessableInputStream(new CipherStream(readable.GetInputStream(), cipher, null));
			}
			catch (IOException e4)
			{
				throw new CmsException("error reading content.", e4);
			}
		}
	}

	internal static readonly CmsEnvelopedHelper Instance;

	private static readonly IDictionary KeySizes;

	private static readonly IDictionary BaseCipherNames;

	static CmsEnvelopedHelper()
	{
		Instance = new CmsEnvelopedHelper();
		KeySizes = Platform.CreateHashtable();
		BaseCipherNames = Platform.CreateHashtable();
		KeySizes.Add(CmsEnvelopedGenerator.DesEde3Cbc, 192);
		KeySizes.Add(CmsEnvelopedGenerator.Aes128Cbc, 128);
		KeySizes.Add(CmsEnvelopedGenerator.Aes192Cbc, 192);
		KeySizes.Add(CmsEnvelopedGenerator.Aes256Cbc, 256);
		BaseCipherNames.Add(CmsEnvelopedGenerator.DesEde3Cbc, "DESEDE");
		BaseCipherNames.Add(CmsEnvelopedGenerator.Aes128Cbc, "AES");
		BaseCipherNames.Add(CmsEnvelopedGenerator.Aes192Cbc, "AES");
		BaseCipherNames.Add(CmsEnvelopedGenerator.Aes256Cbc, "AES");
	}

	private string GetAsymmetricEncryptionAlgName(string encryptionAlgOid)
	{
		if (PkcsObjectIdentifiers.RsaEncryption.Id.Equals(encryptionAlgOid))
		{
			return "RSA/ECB/PKCS1Padding";
		}
		return encryptionAlgOid;
	}

	internal IBufferedCipher CreateAsymmetricCipher(string encryptionOid)
	{
		string asymmetricEncryptionAlgName = GetAsymmetricEncryptionAlgName(encryptionOid);
		if (!asymmetricEncryptionAlgName.Equals(encryptionOid))
		{
			try
			{
				return CipherUtilities.GetCipher(asymmetricEncryptionAlgName);
			}
			catch (SecurityUtilityException)
			{
			}
		}
		return CipherUtilities.GetCipher(encryptionOid);
	}

	internal IWrapper CreateWrapper(string encryptionOid)
	{
		try
		{
			return WrapperUtilities.GetWrapper(encryptionOid);
		}
		catch (SecurityUtilityException)
		{
			return WrapperUtilities.GetWrapper(GetAsymmetricEncryptionAlgName(encryptionOid));
		}
	}

	internal string GetRfc3211WrapperName(string oid)
	{
		if (oid == null)
		{
			throw new ArgumentNullException("oid");
		}
		string text = (string)BaseCipherNames[oid];
		if (text == null)
		{
			throw new ArgumentException("no name for " + oid, "oid");
		}
		return text + "RFC3211Wrap";
	}

	internal int GetKeySize(string oid)
	{
		if (!KeySizes.Contains(oid))
		{
			throw new ArgumentException("no keysize for " + oid, "oid");
		}
		return (int)KeySizes[oid];
	}

	internal static RecipientInformationStore BuildRecipientInformationStore(Asn1Set recipientInfos, CmsSecureReadable secureReadable)
	{
		IList list = Platform.CreateArrayList();
		for (int i = 0; i != recipientInfos.Count; i++)
		{
			RecipientInfo instance = RecipientInfo.GetInstance(recipientInfos[i]);
			ReadRecipientInfo(list, instance, secureReadable);
		}
		return new RecipientInformationStore(list);
	}

	private static void ReadRecipientInfo(IList infos, RecipientInfo info, CmsSecureReadable secureReadable)
	{
		Asn1Encodable info2 = info.Info;
		if (info2 is KeyTransRecipientInfo)
		{
			infos.Add(new KeyTransRecipientInformation((KeyTransRecipientInfo)info2, secureReadable));
		}
		else if (info2 is KekRecipientInfo)
		{
			infos.Add(new KekRecipientInformation((KekRecipientInfo)info2, secureReadable));
		}
		else if (info2 is KeyAgreeRecipientInfo)
		{
			KeyAgreeRecipientInformation.ReadRecipientInfo(infos, (KeyAgreeRecipientInfo)info2, secureReadable);
		}
		else if (info2 is PasswordRecipientInfo)
		{
			infos.Add(new PasswordRecipientInformation((PasswordRecipientInfo)info2, secureReadable));
		}
	}
}
