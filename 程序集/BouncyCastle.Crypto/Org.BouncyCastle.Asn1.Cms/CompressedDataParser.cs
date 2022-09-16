using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Cms;

public class CompressedDataParser
{
	private DerInteger _version;

	private AlgorithmIdentifier _compressionAlgorithm;

	private ContentInfoParser _encapContentInfo;

	public DerInteger Version => _version;

	public AlgorithmIdentifier CompressionAlgorithmIdentifier => _compressionAlgorithm;

	public CompressedDataParser(Asn1SequenceParser seq)
	{
		_version = (DerInteger)seq.ReadObject();
		_compressionAlgorithm = AlgorithmIdentifier.GetInstance(seq.ReadObject().ToAsn1Object());
		_encapContentInfo = new ContentInfoParser((Asn1SequenceParser)seq.ReadObject());
	}

	public ContentInfoParser GetEncapContentInfo()
	{
		return _encapContentInfo;
	}
}
