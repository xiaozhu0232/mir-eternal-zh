using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Cms;

public class CmsCompressedData
{
	internal ContentInfo contentInfo;

	public ContentInfo ContentInfo => contentInfo;

	public CmsCompressedData(byte[] compressedData)
		: this(CmsUtilities.ReadContentInfo(compressedData))
	{
	}

	public CmsCompressedData(Stream compressedDataStream)
		: this(CmsUtilities.ReadContentInfo(compressedDataStream))
	{
	}

	public CmsCompressedData(ContentInfo contentInfo)
	{
		this.contentInfo = contentInfo;
	}

	public byte[] GetContent()
	{
		CompressedData instance = CompressedData.GetInstance(contentInfo.Content);
		ContentInfo encapContentInfo = instance.EncapContentInfo;
		Asn1OctetString asn1OctetString = (Asn1OctetString)encapContentInfo.Content;
		ZInputStream zInputStream = new ZInputStream(asn1OctetString.GetOctetStream());
		try
		{
			return CmsUtilities.StreamToByteArray(zInputStream);
		}
		catch (IOException e)
		{
			throw new CmsException("exception reading compressed stream.", e);
		}
		finally
		{
			Platform.Dispose(zInputStream);
		}
	}

	public byte[] GetContent(int limit)
	{
		CompressedData instance = CompressedData.GetInstance(contentInfo.Content);
		ContentInfo encapContentInfo = instance.EncapContentInfo;
		Asn1OctetString asn1OctetString = (Asn1OctetString)encapContentInfo.Content;
		ZInputStream inStream = new ZInputStream(new MemoryStream(asn1OctetString.GetOctets(), writable: false));
		try
		{
			return CmsUtilities.StreamToByteArray(inStream, limit);
		}
		catch (IOException e)
		{
			throw new CmsException("exception reading compressed stream.", e);
		}
	}

	public byte[] GetEncoded()
	{
		return contentInfo.GetEncoded();
	}
}
