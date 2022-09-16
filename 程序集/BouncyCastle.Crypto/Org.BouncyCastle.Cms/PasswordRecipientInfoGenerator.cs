using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms;

internal class PasswordRecipientInfoGenerator : RecipientInfoGenerator
{
	private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

	private AlgorithmIdentifier keyDerivationAlgorithm;

	private KeyParameter keyEncryptionKey;

	private string keyEncryptionKeyOID;

	internal AlgorithmIdentifier KeyDerivationAlgorithm
	{
		set
		{
			keyDerivationAlgorithm = value;
		}
	}

	internal KeyParameter KeyEncryptionKey
	{
		set
		{
			keyEncryptionKey = value;
		}
	}

	internal string KeyEncryptionKeyOID
	{
		set
		{
			keyEncryptionKeyOID = value;
		}
	}

	internal PasswordRecipientInfoGenerator()
	{
	}

	public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
	{
		byte[] key = contentEncryptionKey.GetKey();
		string rfc3211WrapperName = Helper.GetRfc3211WrapperName(keyEncryptionKeyOID);
		IWrapper wrapper = Helper.CreateWrapper(rfc3211WrapperName);
		int num = (Platform.StartsWith(rfc3211WrapperName, "DESEDE") ? 8 : 16);
		byte[] array = new byte[num];
		random.NextBytes(array);
		ICipherParameters parameters = new ParametersWithIV(keyEncryptionKey, array);
		wrapper.Init(forWrapping: true, new ParametersWithRandom(parameters, random));
		Asn1OctetString encryptedKey = new DerOctetString(wrapper.Wrap(key, 0, key.Length));
		DerSequence parameters2 = new DerSequence(new DerObjectIdentifier(keyEncryptionKeyOID), new DerOctetString(array));
		AlgorithmIdentifier keyEncryptionAlgorithm = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdAlgPwriKek, parameters2);
		return new RecipientInfo(new PasswordRecipientInfo(keyDerivationAlgorithm, keyEncryptionAlgorithm, encryptedKey));
	}
}
