using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Cms;

public class EncryptedContentInfoParser
{
	private DerObjectIdentifier _contentType;

	private AlgorithmIdentifier _contentEncryptionAlgorithm;

	private Asn1TaggedObjectParser _encryptedContent;

	public DerObjectIdentifier ContentType => _contentType;

	public AlgorithmIdentifier ContentEncryptionAlgorithm => _contentEncryptionAlgorithm;

	public EncryptedContentInfoParser(Asn1SequenceParser seq)
	{
		_contentType = (DerObjectIdentifier)seq.ReadObject();
		_contentEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(seq.ReadObject().ToAsn1Object());
		_encryptedContent = (Asn1TaggedObjectParser)seq.ReadObject();
	}

	public IAsn1Convertible GetEncryptedContent(int tag)
	{
		return _encryptedContent.GetObjectParser(tag, isExplicit: false);
	}
}
