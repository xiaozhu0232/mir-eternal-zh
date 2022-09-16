using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Cms;

public class CmsCompressedDataGenerator
{
	public const string ZLib = "1.2.840.113549.1.9.16.3.8";

	public CmsCompressedData Generate(CmsProcessable content, string compressionOid)
	{
		AlgorithmIdentifier compressionAlgorithm;
		Asn1OctetString content2;
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			ZOutputStream zOutputStream = new ZOutputStream(memoryStream, -1);
			content.Write(zOutputStream);
			Platform.Dispose(zOutputStream);
			compressionAlgorithm = new AlgorithmIdentifier(new DerObjectIdentifier(compressionOid));
			content2 = new BerOctetString(memoryStream.ToArray());
		}
		catch (IOException e)
		{
			throw new CmsException("exception encoding data.", e);
		}
		ContentInfo encapContentInfo = new ContentInfo(CmsObjectIdentifiers.Data, content2);
		ContentInfo contentInfo = new ContentInfo(CmsObjectIdentifiers.CompressedData, new CompressedData(compressionAlgorithm, encapContentInfo));
		return new CmsCompressedData(contentInfo);
	}
}
