using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Cms;

public class CmsSignedDataParser : CmsContentInfoParser
{
	private static readonly CmsSignedHelper Helper = CmsSignedHelper.Instance;

	private SignedDataParser _signedData;

	private DerObjectIdentifier _signedContentType;

	private CmsTypedStream _signedContent;

	private IDictionary _digests;

	private ISet _digestOids;

	private SignerInformationStore _signerInfoStore;

	private Asn1Set _certSet;

	private Asn1Set _crlSet;

	private bool _isCertCrlParsed;

	private IX509Store _attributeStore;

	private IX509Store _certificateStore;

	private IX509Store _crlStore;

	public int Version => _signedData.Version.IntValueExact;

	public ISet DigestOids => new HashSet(_digestOids);

	public DerObjectIdentifier SignedContentType => _signedContentType;

	public CmsSignedDataParser(byte[] sigBlock)
		: this(new MemoryStream(sigBlock, writable: false))
	{
	}

	public CmsSignedDataParser(CmsTypedStream signedContent, byte[] sigBlock)
		: this(signedContent, new MemoryStream(sigBlock, writable: false))
	{
	}

	public CmsSignedDataParser(Stream sigData)
		: this(null, sigData)
	{
	}

	public CmsSignedDataParser(CmsTypedStream signedContent, Stream sigData)
		: base(sigData)
	{
		try
		{
			_signedContent = signedContent;
			_signedData = SignedDataParser.GetInstance(contentInfo.GetContent(16));
			_digests = Platform.CreateHashtable();
			_digestOids = new HashSet();
			Asn1SetParser digestAlgorithms = _signedData.GetDigestAlgorithms();
			IAsn1Convertible asn1Convertible;
			while ((asn1Convertible = digestAlgorithms.ReadObject()) != null)
			{
				AlgorithmIdentifier instance = AlgorithmIdentifier.GetInstance(asn1Convertible.ToAsn1Object());
				try
				{
					string id = instance.Algorithm.Id;
					string digestAlgName = Helper.GetDigestAlgName(id);
					if (!_digests.Contains(digestAlgName))
					{
						_digests[digestAlgName] = Helper.GetDigestInstance(digestAlgName);
						_digestOids.Add(id);
					}
				}
				catch (SecurityUtilityException)
				{
				}
			}
			ContentInfoParser encapContentInfo = _signedData.GetEncapContentInfo();
			Asn1OctetStringParser asn1OctetStringParser = (Asn1OctetStringParser)encapContentInfo.GetContent(4);
			if (asn1OctetStringParser != null)
			{
				CmsTypedStream cmsTypedStream = new CmsTypedStream(encapContentInfo.ContentType.Id, asn1OctetStringParser.GetOctetStream());
				if (_signedContent == null)
				{
					_signedContent = cmsTypedStream;
				}
				else
				{
					cmsTypedStream.Drain();
				}
			}
			_signedContentType = ((_signedContent == null) ? encapContentInfo.ContentType : new DerObjectIdentifier(_signedContent.ContentType));
		}
		catch (IOException ex2)
		{
			throw new CmsException("io exception: " + ex2.Message, ex2);
		}
	}

	public SignerInformationStore GetSignerInfos()
	{
		if (_signerInfoStore == null)
		{
			PopulateCertCrlSets();
			IList list = Platform.CreateArrayList();
			IDictionary dictionary = Platform.CreateHashtable();
			foreach (object key in _digests.Keys)
			{
				dictionary[key] = DigestUtilities.DoFinal((IDigest)_digests[key]);
			}
			try
			{
				Asn1SetParser signerInfos = _signedData.GetSignerInfos();
				IAsn1Convertible asn1Convertible;
				while ((asn1Convertible = signerInfos.ReadObject()) != null)
				{
					SignerInfo instance = SignerInfo.GetInstance(asn1Convertible.ToAsn1Object());
					string digestAlgName = Helper.GetDigestAlgName(instance.DigestAlgorithm.Algorithm.Id);
					byte[] digest = (byte[])dictionary[digestAlgName];
					list.Add(new SignerInformation(instance, _signedContentType, null, new BaseDigestCalculator(digest)));
				}
			}
			catch (IOException ex)
			{
				throw new CmsException("io exception: " + ex.Message, ex);
			}
			_signerInfoStore = new SignerInformationStore(list);
		}
		return _signerInfoStore;
	}

	public IX509Store GetAttributeCertificates(string type)
	{
		if (_attributeStore == null)
		{
			PopulateCertCrlSets();
			_attributeStore = Helper.CreateAttributeStore(type, _certSet);
		}
		return _attributeStore;
	}

	public IX509Store GetCertificates(string type)
	{
		if (_certificateStore == null)
		{
			PopulateCertCrlSets();
			_certificateStore = Helper.CreateCertificateStore(type, _certSet);
		}
		return _certificateStore;
	}

	public IX509Store GetCrls(string type)
	{
		if (_crlStore == null)
		{
			PopulateCertCrlSets();
			_crlStore = Helper.CreateCrlStore(type, _crlSet);
		}
		return _crlStore;
	}

	private void PopulateCertCrlSets()
	{
		if (_isCertCrlParsed)
		{
			return;
		}
		_isCertCrlParsed = true;
		try
		{
			_certSet = GetAsn1Set(_signedData.GetCertificates());
			_crlSet = GetAsn1Set(_signedData.GetCrls());
		}
		catch (IOException e)
		{
			throw new CmsException("problem parsing cert/crl sets", e);
		}
	}

	public CmsTypedStream GetSignedContent()
	{
		if (_signedContent == null)
		{
			return null;
		}
		Stream stream = _signedContent.ContentStream;
		foreach (IDigest value in _digests.Values)
		{
			stream = new DigestStream(stream, value, null);
		}
		return new CmsTypedStream(_signedContent.ContentType, stream);
	}

	public static Stream ReplaceSigners(Stream original, SignerInformationStore signerInformationStore, Stream outStr)
	{
		CmsSignedDataStreamGenerator cmsSignedDataStreamGenerator = new CmsSignedDataStreamGenerator();
		CmsSignedDataParser cmsSignedDataParser = new CmsSignedDataParser(original);
		cmsSignedDataStreamGenerator.AddSigners(signerInformationStore);
		CmsTypedStream signedContent = cmsSignedDataParser.GetSignedContent();
		bool flag = signedContent != null;
		Stream stream = cmsSignedDataStreamGenerator.Open(outStr, cmsSignedDataParser.SignedContentType.Id, flag);
		if (flag)
		{
			Streams.PipeAll(signedContent.ContentStream, stream);
		}
		cmsSignedDataStreamGenerator.AddAttributeCertificates(cmsSignedDataParser.GetAttributeCertificates("Collection"));
		cmsSignedDataStreamGenerator.AddCertificates(cmsSignedDataParser.GetCertificates("Collection"));
		cmsSignedDataStreamGenerator.AddCrls(cmsSignedDataParser.GetCrls("Collection"));
		Platform.Dispose(stream);
		return outStr;
	}

	public static Stream ReplaceCertificatesAndCrls(Stream original, IX509Store x509Certs, IX509Store x509Crls, IX509Store x509AttrCerts, Stream outStr)
	{
		CmsSignedDataStreamGenerator cmsSignedDataStreamGenerator = new CmsSignedDataStreamGenerator();
		CmsSignedDataParser cmsSignedDataParser = new CmsSignedDataParser(original);
		cmsSignedDataStreamGenerator.AddDigests(cmsSignedDataParser.DigestOids);
		CmsTypedStream signedContent = cmsSignedDataParser.GetSignedContent();
		bool flag = signedContent != null;
		Stream stream = cmsSignedDataStreamGenerator.Open(outStr, cmsSignedDataParser.SignedContentType.Id, flag);
		if (flag)
		{
			Streams.PipeAll(signedContent.ContentStream, stream);
		}
		if (x509AttrCerts != null)
		{
			cmsSignedDataStreamGenerator.AddAttributeCertificates(x509AttrCerts);
		}
		if (x509Certs != null)
		{
			cmsSignedDataStreamGenerator.AddCertificates(x509Certs);
		}
		if (x509Crls != null)
		{
			cmsSignedDataStreamGenerator.AddCrls(x509Crls);
		}
		cmsSignedDataStreamGenerator.AddSigners(cmsSignedDataParser.GetSignerInfos());
		Platform.Dispose(stream);
		return outStr;
	}

	private static Asn1Set GetAsn1Set(Asn1SetParser asn1SetParser)
	{
		if (asn1SetParser != null)
		{
			return Asn1Set.GetInstance(asn1SetParser.ToAsn1Object());
		}
		return null;
	}
}
