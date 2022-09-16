using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms;

public class KeyTransRecipientInfoGenerator : RecipientInfoGenerator
{
	private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

	private TbsCertificateStructure recipientTbsCert;

	private AsymmetricKeyParameter recipientPublicKey;

	private Asn1OctetString subjectKeyIdentifier;

	private SubjectPublicKeyInfo info;

	private IssuerAndSerialNumber issuerAndSerialNumber;

	private SecureRandom random;

	internal X509Certificate RecipientCert
	{
		set
		{
			recipientTbsCert = CmsUtilities.GetTbsCertificateStructure(value);
			recipientPublicKey = value.GetPublicKey();
			info = recipientTbsCert.SubjectPublicKeyInfo;
		}
	}

	internal AsymmetricKeyParameter RecipientPublicKey
	{
		set
		{
			recipientPublicKey = value;
			try
			{
				info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(recipientPublicKey);
			}
			catch (IOException)
			{
				throw new ArgumentException("can't extract key algorithm from this key");
			}
		}
	}

	internal Asn1OctetString SubjectKeyIdentifier
	{
		set
		{
			subjectKeyIdentifier = value;
		}
	}

	protected virtual AlgorithmIdentifier AlgorithmDetails => info.AlgorithmID;

	internal KeyTransRecipientInfoGenerator()
	{
	}

	protected KeyTransRecipientInfoGenerator(IssuerAndSerialNumber issuerAndSerialNumber)
	{
		this.issuerAndSerialNumber = issuerAndSerialNumber;
	}

	protected KeyTransRecipientInfoGenerator(byte[] subjectKeyIdentifier)
	{
		this.subjectKeyIdentifier = new DerOctetString(subjectKeyIdentifier);
	}

	public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
	{
		AlgorithmIdentifier algorithmDetails = AlgorithmDetails;
		this.random = random;
		byte[] str = GenerateWrappedKey(contentEncryptionKey);
		RecipientIdentifier rid;
		if (recipientTbsCert != null)
		{
			IssuerAndSerialNumber id = new IssuerAndSerialNumber(recipientTbsCert.Issuer, recipientTbsCert.SerialNumber.Value);
			rid = new RecipientIdentifier(id);
		}
		else
		{
			rid = new RecipientIdentifier(subjectKeyIdentifier);
		}
		return new RecipientInfo(new KeyTransRecipientInfo(rid, algorithmDetails, new DerOctetString(str)));
	}

	protected virtual byte[] GenerateWrappedKey(KeyParameter contentEncryptionKey)
	{
		byte[] key = contentEncryptionKey.GetKey();
		AlgorithmIdentifier algorithmID = info.AlgorithmID;
		IWrapper wrapper = Helper.CreateWrapper(algorithmID.Algorithm.Id);
		wrapper.Init(forWrapping: true, new ParametersWithRandom(recipientPublicKey, random));
		return wrapper.Wrap(key, 0, key.Length);
	}
}
