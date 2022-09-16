using System;
using System.IO;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;

namespace Org.BouncyCastle.Crmf;

public class PkiArchiveControlBuilder
{
	private CmsEnvelopedDataGenerator envGen;

	private CmsProcessableByteArray keyContent;

	public PkiArchiveControlBuilder(PrivateKeyInfo privateKeyInfo, GeneralName generalName)
	{
		EncKeyWithID encKeyWithID = new EncKeyWithID(privateKeyInfo, generalName);
		try
		{
			keyContent = new CmsProcessableByteArray(CrmfObjectIdentifiers.id_ct_encKeyWithID, encKeyWithID.GetEncoded());
		}
		catch (IOException innerException)
		{
			throw new InvalidOperationException("unable to encode key and general name info", innerException);
		}
		envGen = new CmsEnvelopedDataGenerator();
	}

	public PkiArchiveControlBuilder AddRecipientGenerator(RecipientInfoGenerator recipientGen)
	{
		envGen.AddRecipientInfoGenerator(recipientGen);
		return this;
	}

	public PkiArchiveControl Build(ICipherBuilderWithKey contentEncryptor)
	{
		CmsEnvelopedData cmsEnvelopedData = envGen.Generate(keyContent, contentEncryptor);
		EnvelopedData instance = EnvelopedData.GetInstance(cmsEnvelopedData.ContentInfo.Content);
		return new PkiArchiveControl(new PkiArchiveOptions(new EncryptedKey(instance)));
	}
}
