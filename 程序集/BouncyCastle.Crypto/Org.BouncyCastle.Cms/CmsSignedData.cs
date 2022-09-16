using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Cms;

public class CmsSignedData
{
	private static readonly CmsSignedHelper Helper = CmsSignedHelper.Instance;

	private readonly CmsProcessable signedContent;

	private SignedData signedData;

	private ContentInfo contentInfo;

	private SignerInformationStore signerInfoStore;

	private IX509Store attrCertStore;

	private IX509Store certificateStore;

	private IX509Store crlStore;

	private IDictionary hashes;

	public int Version => signedData.Version.IntValueExact;

	[Obsolete("Use 'SignedContentType' property instead.")]
	public string SignedContentTypeOid => signedData.EncapContentInfo.ContentType.Id;

	public DerObjectIdentifier SignedContentType => signedData.EncapContentInfo.ContentType;

	public CmsProcessable SignedContent => signedContent;

	public ContentInfo ContentInfo => contentInfo;

	private CmsSignedData(CmsSignedData c)
	{
		signedData = c.signedData;
		contentInfo = c.contentInfo;
		signedContent = c.signedContent;
		signerInfoStore = c.signerInfoStore;
	}

	public CmsSignedData(byte[] sigBlock)
		: this(CmsUtilities.ReadContentInfo(new MemoryStream(sigBlock, writable: false)))
	{
	}

	public CmsSignedData(CmsProcessable signedContent, byte[] sigBlock)
		: this(signedContent, CmsUtilities.ReadContentInfo(new MemoryStream(sigBlock, writable: false)))
	{
	}

	public CmsSignedData(IDictionary hashes, byte[] sigBlock)
		: this(hashes, CmsUtilities.ReadContentInfo(sigBlock))
	{
	}

	public CmsSignedData(CmsProcessable signedContent, Stream sigData)
		: this(signedContent, CmsUtilities.ReadContentInfo(sigData))
	{
	}

	public CmsSignedData(Stream sigData)
		: this(CmsUtilities.ReadContentInfo(sigData))
	{
	}

	public CmsSignedData(CmsProcessable signedContent, ContentInfo sigData)
	{
		this.signedContent = signedContent;
		contentInfo = sigData;
		signedData = SignedData.GetInstance(contentInfo.Content);
	}

	public CmsSignedData(IDictionary hashes, ContentInfo sigData)
	{
		this.hashes = hashes;
		contentInfo = sigData;
		signedData = SignedData.GetInstance(contentInfo.Content);
	}

	public CmsSignedData(ContentInfo sigData)
	{
		contentInfo = sigData;
		signedData = SignedData.GetInstance(contentInfo.Content);
		if (signedData.EncapContentInfo.Content != null)
		{
			signedContent = new CmsProcessableByteArray(((Asn1OctetString)signedData.EncapContentInfo.Content).GetOctets());
		}
	}

	internal IX509Store GetCertificates()
	{
		return Helper.GetCertificates(signedData.Certificates);
	}

	public SignerInformationStore GetSignerInfos()
	{
		if (signerInfoStore == null)
		{
			IList list = Platform.CreateArrayList();
			Asn1Set signerInfos = signedData.SignerInfos;
			foreach (object item in signerInfos)
			{
				SignerInfo instance = SignerInfo.GetInstance(item);
				DerObjectIdentifier contentType = signedData.EncapContentInfo.ContentType;
				if (hashes == null)
				{
					list.Add(new SignerInformation(instance, contentType, signedContent, null));
					continue;
				}
				byte[] digest = (byte[])hashes[instance.DigestAlgorithm.Algorithm.Id];
				list.Add(new SignerInformation(instance, contentType, null, new BaseDigestCalculator(digest)));
			}
			signerInfoStore = new SignerInformationStore(list);
		}
		return signerInfoStore;
	}

	public IX509Store GetAttributeCertificates(string type)
	{
		if (attrCertStore == null)
		{
			attrCertStore = Helper.CreateAttributeStore(type, signedData.Certificates);
		}
		return attrCertStore;
	}

	public IX509Store GetCertificates(string type)
	{
		if (certificateStore == null)
		{
			certificateStore = Helper.CreateCertificateStore(type, signedData.Certificates);
		}
		return certificateStore;
	}

	public IX509Store GetCrls(string type)
	{
		if (crlStore == null)
		{
			crlStore = Helper.CreateCrlStore(type, signedData.CRLs);
		}
		return crlStore;
	}

	public byte[] GetEncoded()
	{
		return contentInfo.GetEncoded();
	}

	public byte[] GetEncoded(string encoding)
	{
		return contentInfo.GetEncoded(encoding);
	}

	public static CmsSignedData ReplaceSigners(CmsSignedData signedData, SignerInformationStore signerInformationStore)
	{
		CmsSignedData cmsSignedData = new CmsSignedData(signedData);
		cmsSignedData.signerInfoStore = signerInformationStore;
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
		foreach (SignerInformation signer in signerInformationStore.GetSigners())
		{
			asn1EncodableVector.Add(Helper.FixAlgID(signer.DigestAlgorithmID));
			asn1EncodableVector2.Add(signer.ToSignerInfo());
		}
		Asn1Set asn1Set = new DerSet(asn1EncodableVector);
		Asn1Set element = new DerSet(asn1EncodableVector2);
		Asn1Sequence asn1Sequence = (Asn1Sequence)signedData.signedData.ToAsn1Object();
		asn1EncodableVector2 = new Asn1EncodableVector(asn1Sequence[0], asn1Set);
		for (int i = 2; i != asn1Sequence.Count - 1; i++)
		{
			asn1EncodableVector2.Add(asn1Sequence[i]);
		}
		asn1EncodableVector2.Add(element);
		cmsSignedData.signedData = SignedData.GetInstance(new BerSequence(asn1EncodableVector2));
		cmsSignedData.contentInfo = new ContentInfo(cmsSignedData.contentInfo.ContentType, cmsSignedData.signedData);
		return cmsSignedData;
	}

	public static CmsSignedData ReplaceCertificatesAndCrls(CmsSignedData signedData, IX509Store x509Certs, IX509Store x509Crls, IX509Store x509AttrCerts)
	{
		if (x509AttrCerts != null)
		{
			throw Platform.CreateNotImplementedException("Currently can't replace attribute certificates");
		}
		CmsSignedData cmsSignedData = new CmsSignedData(signedData);
		Asn1Set certificates = null;
		try
		{
			Asn1Set asn1Set = CmsUtilities.CreateBerSetFromList(CmsUtilities.GetCertificatesFromStore(x509Certs));
			if (asn1Set.Count != 0)
			{
				certificates = asn1Set;
			}
		}
		catch (X509StoreException e)
		{
			throw new CmsException("error getting certificates from store", e);
		}
		Asn1Set crls = null;
		try
		{
			Asn1Set asn1Set2 = CmsUtilities.CreateBerSetFromList(CmsUtilities.GetCrlsFromStore(x509Crls));
			if (asn1Set2.Count != 0)
			{
				crls = asn1Set2;
			}
		}
		catch (X509StoreException e2)
		{
			throw new CmsException("error getting CRLs from store", e2);
		}
		SignedData signedData2 = signedData.signedData;
		cmsSignedData.signedData = new SignedData(signedData2.DigestAlgorithms, signedData2.EncapContentInfo, certificates, crls, signedData2.SignerInfos);
		cmsSignedData.contentInfo = new ContentInfo(cmsSignedData.contentInfo.ContentType, cmsSignedData.signedData);
		return cmsSignedData;
	}
}
