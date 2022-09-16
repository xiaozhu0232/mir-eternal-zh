using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Cms;

namespace Org.BouncyCastle.Crmf;

public class PkiArchiveControl : IControl
{
	public static readonly int encryptedPrivKey = 0;

	public static readonly int keyGenParameters = 1;

	public static readonly int archiveRemGenPrivKey = 2;

	private static readonly DerObjectIdentifier type = CrmfObjectIdentifiers.id_regCtrl_pkiArchiveOptions;

	private readonly PkiArchiveOptions pkiArchiveOptions;

	public DerObjectIdentifier Type => type;

	public Asn1Encodable Value => pkiArchiveOptions;

	public int ArchiveType => pkiArchiveOptions.Type;

	public bool EnvelopedData
	{
		get
		{
			EncryptedKey instance = EncryptedKey.GetInstance(pkiArchiveOptions.Value);
			return !instance.IsEncryptedValue;
		}
	}

	public PkiArchiveControl(PkiArchiveOptions pkiArchiveOptions)
	{
		this.pkiArchiveOptions = pkiArchiveOptions;
	}

	public CmsEnvelopedData GetEnvelopedData()
	{
		try
		{
			EncryptedKey instance = EncryptedKey.GetInstance(pkiArchiveOptions.Value);
			EnvelopedData instance2 = Org.BouncyCastle.Asn1.Cms.EnvelopedData.GetInstance(instance.Value);
			return new CmsEnvelopedData(new ContentInfo(CmsObjectIdentifiers.EnvelopedData, instance2));
		}
		catch (CmsException ex)
		{
			throw new CrmfException("CMS parsing error: " + ex.Message, ex);
		}
		catch (Exception ex2)
		{
			throw new CrmfException("CRMF parsing error: " + ex2.Message, ex2);
		}
	}
}
