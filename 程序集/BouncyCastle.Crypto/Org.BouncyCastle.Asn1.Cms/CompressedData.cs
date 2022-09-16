using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class CompressedData : Asn1Encodable
{
	private DerInteger version;

	private AlgorithmIdentifier compressionAlgorithm;

	private ContentInfo encapContentInfo;

	public DerInteger Version => version;

	public AlgorithmIdentifier CompressionAlgorithmIdentifier => compressionAlgorithm;

	public ContentInfo EncapContentInfo => encapContentInfo;

	public CompressedData(AlgorithmIdentifier compressionAlgorithm, ContentInfo encapContentInfo)
	{
		version = new DerInteger(0);
		this.compressionAlgorithm = compressionAlgorithm;
		this.encapContentInfo = encapContentInfo;
	}

	public CompressedData(Asn1Sequence seq)
	{
		version = (DerInteger)seq[0];
		compressionAlgorithm = AlgorithmIdentifier.GetInstance(seq[1]);
		encapContentInfo = ContentInfo.GetInstance(seq[2]);
	}

	public static CompressedData GetInstance(Asn1TaggedObject ato, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(ato, explicitly));
	}

	public static CompressedData GetInstance(object obj)
	{
		if (obj == null || obj is CompressedData)
		{
			return (CompressedData)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CompressedData((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid CompressedData: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		return new BerSequence(version, compressionAlgorithm, encapContentInfo);
	}
}
