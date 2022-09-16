using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms;

public class CmsEnvelopedGenerator
{
	public const string IdeaCbc = "1.3.6.1.4.1.188.7.1.1.2";

	public const string Cast5Cbc = "1.2.840.113533.7.66.10";

	internal static readonly short[] rc2Table = new short[256]
	{
		189, 86, 234, 242, 162, 241, 172, 42, 176, 147,
		209, 156, 27, 51, 253, 208, 48, 4, 182, 220,
		125, 223, 50, 75, 247, 203, 69, 155, 49, 187,
		33, 90, 65, 159, 225, 217, 74, 77, 158, 218,
		160, 104, 44, 195, 39, 95, 128, 54, 62, 238,
		251, 149, 26, 254, 206, 168, 52, 169, 19, 240,
		166, 63, 216, 12, 120, 36, 175, 35, 82, 193,
		103, 23, 245, 102, 144, 231, 232, 7, 184, 96,
		72, 230, 30, 83, 243, 146, 164, 114, 140, 8,
		21, 110, 134, 0, 132, 250, 244, 127, 138, 66,
		25, 246, 219, 205, 20, 141, 80, 18, 186, 60,
		6, 78, 236, 179, 53, 17, 161, 136, 142, 43,
		148, 153, 183, 113, 116, 211, 228, 191, 58, 222,
		150, 14, 188, 10, 237, 119, 252, 55, 107, 3,
		121, 137, 98, 198, 215, 192, 210, 124, 106, 139,
		34, 163, 91, 5, 93, 2, 117, 213, 97, 227,
		24, 143, 85, 81, 173, 31, 11, 94, 133, 229,
		194, 87, 99, 202, 61, 108, 180, 197, 204, 112,
		178, 145, 89, 13, 71, 32, 200, 79, 88, 224,
		1, 226, 22, 56, 196, 111, 59, 15, 101, 70,
		190, 126, 45, 123, 130, 249, 64, 181, 29, 115,
		248, 235, 38, 199, 135, 151, 37, 84, 177, 40,
		170, 152, 157, 165, 100, 109, 122, 212, 16, 129,
		68, 239, 73, 214, 174, 46, 221, 118, 92, 47,
		167, 28, 201, 9, 105, 154, 131, 207, 41, 57,
		185, 233, 76, 255, 67, 171
	};

	public static readonly string DesEde3Cbc = PkcsObjectIdentifiers.DesEde3Cbc.Id;

	public static readonly string RC2Cbc = PkcsObjectIdentifiers.RC2Cbc.Id;

	public static readonly string Aes128Cbc = NistObjectIdentifiers.IdAes128Cbc.Id;

	public static readonly string Aes192Cbc = NistObjectIdentifiers.IdAes192Cbc.Id;

	public static readonly string Aes256Cbc = NistObjectIdentifiers.IdAes256Cbc.Id;

	public static readonly string Camellia128Cbc = NttObjectIdentifiers.IdCamellia128Cbc.Id;

	public static readonly string Camellia192Cbc = NttObjectIdentifiers.IdCamellia192Cbc.Id;

	public static readonly string Camellia256Cbc = NttObjectIdentifiers.IdCamellia256Cbc.Id;

	public static readonly string SeedCbc = KisaObjectIdentifiers.IdSeedCbc.Id;

	public static readonly string DesEde3Wrap = PkcsObjectIdentifiers.IdAlgCms3DesWrap.Id;

	public static readonly string Aes128Wrap = NistObjectIdentifiers.IdAes128Wrap.Id;

	public static readonly string Aes192Wrap = NistObjectIdentifiers.IdAes192Wrap.Id;

	public static readonly string Aes256Wrap = NistObjectIdentifiers.IdAes256Wrap.Id;

	public static readonly string Camellia128Wrap = NttObjectIdentifiers.IdCamellia128Wrap.Id;

	public static readonly string Camellia192Wrap = NttObjectIdentifiers.IdCamellia192Wrap.Id;

	public static readonly string Camellia256Wrap = NttObjectIdentifiers.IdCamellia256Wrap.Id;

	public static readonly string SeedWrap = KisaObjectIdentifiers.IdNpkiAppCmsSeedWrap.Id;

	public static readonly string ECDHSha1Kdf = X9ObjectIdentifiers.DHSinglePassStdDHSha1KdfScheme.Id;

	public static readonly string ECMqvSha1Kdf = X9ObjectIdentifiers.MqvSinglePassSha1KdfScheme.Id;

	internal readonly IList recipientInfoGenerators = Platform.CreateArrayList();

	internal readonly SecureRandom rand;

	internal CmsAttributeTableGenerator unprotectedAttributeGenerator = null;

	public CmsAttributeTableGenerator UnprotectedAttributeGenerator
	{
		get
		{
			return unprotectedAttributeGenerator;
		}
		set
		{
			unprotectedAttributeGenerator = value;
		}
	}

	public CmsEnvelopedGenerator()
		: this(new SecureRandom())
	{
	}

	public CmsEnvelopedGenerator(SecureRandom rand)
	{
		this.rand = rand;
	}

	public void AddKeyTransRecipient(X509Certificate cert)
	{
		KeyTransRecipientInfoGenerator keyTransRecipientInfoGenerator = new KeyTransRecipientInfoGenerator();
		keyTransRecipientInfoGenerator.RecipientCert = cert;
		recipientInfoGenerators.Add(keyTransRecipientInfoGenerator);
	}

	public void AddKeyTransRecipient(AsymmetricKeyParameter pubKey, byte[] subKeyId)
	{
		KeyTransRecipientInfoGenerator keyTransRecipientInfoGenerator = new KeyTransRecipientInfoGenerator();
		keyTransRecipientInfoGenerator.RecipientPublicKey = pubKey;
		keyTransRecipientInfoGenerator.SubjectKeyIdentifier = new DerOctetString(subKeyId);
		recipientInfoGenerators.Add(keyTransRecipientInfoGenerator);
	}

	public void AddKekRecipient(string keyAlgorithm, KeyParameter key, byte[] keyIdentifier)
	{
		AddKekRecipient(keyAlgorithm, key, new KekIdentifier(keyIdentifier, null, null));
	}

	public void AddKekRecipient(string keyAlgorithm, KeyParameter key, KekIdentifier kekIdentifier)
	{
		KekRecipientInfoGenerator kekRecipientInfoGenerator = new KekRecipientInfoGenerator();
		kekRecipientInfoGenerator.KekIdentifier = kekIdentifier;
		kekRecipientInfoGenerator.KeyEncryptionKeyOID = keyAlgorithm;
		kekRecipientInfoGenerator.KeyEncryptionKey = key;
		recipientInfoGenerators.Add(kekRecipientInfoGenerator);
	}

	public void AddPasswordRecipient(CmsPbeKey pbeKey, string kekAlgorithmOid)
	{
		Pbkdf2Params parameters = new Pbkdf2Params(pbeKey.Salt, pbeKey.IterationCount);
		PasswordRecipientInfoGenerator passwordRecipientInfoGenerator = new PasswordRecipientInfoGenerator();
		passwordRecipientInfoGenerator.KeyDerivationAlgorithm = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdPbkdf2, parameters);
		passwordRecipientInfoGenerator.KeyEncryptionKeyOID = kekAlgorithmOid;
		passwordRecipientInfoGenerator.KeyEncryptionKey = pbeKey.GetEncoded(kekAlgorithmOid);
		recipientInfoGenerators.Add(passwordRecipientInfoGenerator);
	}

	public void AddKeyAgreementRecipient(string agreementAlgorithm, AsymmetricKeyParameter senderPrivateKey, AsymmetricKeyParameter senderPublicKey, X509Certificate recipientCert, string cekWrapAlgorithm)
	{
		IList list = Platform.CreateArrayList(1);
		list.Add(recipientCert);
		AddKeyAgreementRecipients(agreementAlgorithm, senderPrivateKey, senderPublicKey, list, cekWrapAlgorithm);
	}

	public void AddKeyAgreementRecipients(string agreementAlgorithm, AsymmetricKeyParameter senderPrivateKey, AsymmetricKeyParameter senderPublicKey, ICollection recipientCerts, string cekWrapAlgorithm)
	{
		if (!senderPrivateKey.IsPrivate)
		{
			throw new ArgumentException("Expected private key", "senderPrivateKey");
		}
		if (senderPublicKey.IsPrivate)
		{
			throw new ArgumentException("Expected public key", "senderPublicKey");
		}
		KeyAgreeRecipientInfoGenerator keyAgreeRecipientInfoGenerator = new KeyAgreeRecipientInfoGenerator();
		keyAgreeRecipientInfoGenerator.KeyAgreementOID = new DerObjectIdentifier(agreementAlgorithm);
		keyAgreeRecipientInfoGenerator.KeyEncryptionOID = new DerObjectIdentifier(cekWrapAlgorithm);
		keyAgreeRecipientInfoGenerator.RecipientCerts = recipientCerts;
		keyAgreeRecipientInfoGenerator.SenderKeyPair = new AsymmetricCipherKeyPair(senderPublicKey, senderPrivateKey);
		recipientInfoGenerators.Add(keyAgreeRecipientInfoGenerator);
	}

	public void AddRecipientInfoGenerator(RecipientInfoGenerator recipientInfoGenerator)
	{
		recipientInfoGenerators.Add(recipientInfoGenerator);
	}

	protected internal virtual AlgorithmIdentifier GetAlgorithmIdentifier(string encryptionOid, KeyParameter encKey, Asn1Encodable asn1Params, out ICipherParameters cipherParameters)
	{
		Asn1Object asn1Object;
		if (asn1Params != null)
		{
			asn1Object = asn1Params.ToAsn1Object();
			cipherParameters = ParameterUtilities.GetCipherParameters(encryptionOid, encKey, asn1Object);
		}
		else
		{
			asn1Object = DerNull.Instance;
			cipherParameters = encKey;
		}
		return new AlgorithmIdentifier(new DerObjectIdentifier(encryptionOid), asn1Object);
	}

	protected internal virtual Asn1Encodable GenerateAsn1Parameters(string encryptionOid, byte[] encKeyBytes)
	{
		Asn1Encodable result = null;
		try
		{
			if (encryptionOid.Equals(RC2Cbc))
			{
				byte[] array = new byte[8];
				rand.NextBytes(array);
				int num = encKeyBytes.Length * 8;
				int parameterVersion = ((num >= 256) ? num : rc2Table[num]);
				result = new RC2CbcParameter(parameterVersion, array);
				return result;
			}
			result = ParameterUtilities.GenerateParameters(encryptionOid, rand);
			return result;
		}
		catch (SecurityUtilityException)
		{
			return result;
		}
	}
}
