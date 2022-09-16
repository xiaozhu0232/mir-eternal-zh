using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Cms;

public class CmsCompressedDataParser : CmsContentInfoParser
{
	public CmsCompressedDataParser(byte[] compressedData)
		: this(new MemoryStream(compressedData, writable: false))
	{
	}

	public CmsCompressedDataParser(Stream compressedData)
		: base(compressedData)
	{
	}

	public CmsTypedStream GetContent()
	{
		try
		{
			CompressedDataParser compressedDataParser = new CompressedDataParser((Asn1SequenceParser)contentInfo.GetContent(16));
			ContentInfoParser encapContentInfo = compressedDataParser.GetEncapContentInfo();
			Asn1OctetStringParser asn1OctetStringParser = (Asn1OctetStringParser)encapContentInfo.GetContent(4);
			return new CmsTypedStream(encapContentInfo.ContentType.ToString(), new ZInputStream(asn1OctetStringParser.GetOctetStream()));
		}
		catch (IOException e)
		{
			throw new CmsException("IOException reading compressed content.", e);
		}
	}
}
