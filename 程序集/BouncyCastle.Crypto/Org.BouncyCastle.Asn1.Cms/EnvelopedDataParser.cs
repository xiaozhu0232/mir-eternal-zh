namespace Org.BouncyCastle.Asn1.Cms;

public class EnvelopedDataParser
{
	private Asn1SequenceParser _seq;

	private DerInteger _version;

	private IAsn1Convertible _nextObject;

	private bool _originatorInfoCalled;

	public DerInteger Version => _version;

	public EnvelopedDataParser(Asn1SequenceParser seq)
	{
		_seq = seq;
		_version = (DerInteger)seq.ReadObject();
	}

	public OriginatorInfo GetOriginatorInfo()
	{
		_originatorInfoCalled = true;
		if (_nextObject == null)
		{
			_nextObject = _seq.ReadObject();
		}
		if (_nextObject is Asn1TaggedObjectParser && ((Asn1TaggedObjectParser)_nextObject).TagNo == 0)
		{
			Asn1SequenceParser asn1SequenceParser = (Asn1SequenceParser)((Asn1TaggedObjectParser)_nextObject).GetObjectParser(16, isExplicit: false);
			_nextObject = null;
			return OriginatorInfo.GetInstance(asn1SequenceParser.ToAsn1Object());
		}
		return null;
	}

	public Asn1SetParser GetRecipientInfos()
	{
		if (!_originatorInfoCalled)
		{
			GetOriginatorInfo();
		}
		if (_nextObject == null)
		{
			_nextObject = _seq.ReadObject();
		}
		Asn1SetParser result = (Asn1SetParser)_nextObject;
		_nextObject = null;
		return result;
	}

	public EncryptedContentInfoParser GetEncryptedContentInfo()
	{
		if (_nextObject == null)
		{
			_nextObject = _seq.ReadObject();
		}
		if (_nextObject != null)
		{
			Asn1SequenceParser seq = (Asn1SequenceParser)_nextObject;
			_nextObject = null;
			return new EncryptedContentInfoParser(seq);
		}
		return null;
	}

	public Asn1SetParser GetUnprotectedAttrs()
	{
		if (_nextObject == null)
		{
			_nextObject = _seq.ReadObject();
		}
		if (_nextObject != null)
		{
			IAsn1Convertible nextObject = _nextObject;
			_nextObject = null;
			return (Asn1SetParser)((Asn1TaggedObjectParser)nextObject).GetObjectParser(17, isExplicit: false);
		}
		return null;
	}
}
