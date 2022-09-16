using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class SignedDataParser
{
	private Asn1SequenceParser _seq;

	private DerInteger _version;

	private object _nextObject;

	private bool _certsCalled;

	private bool _crlsCalled;

	public DerInteger Version => _version;

	public static SignedDataParser GetInstance(object o)
	{
		if (o is Asn1Sequence)
		{
			return new SignedDataParser(((Asn1Sequence)o).Parser);
		}
		if (o is Asn1SequenceParser)
		{
			return new SignedDataParser((Asn1SequenceParser)o);
		}
		throw new IOException("unknown object encountered: " + Platform.GetTypeName(o));
	}

	public SignedDataParser(Asn1SequenceParser seq)
	{
		_seq = seq;
		_version = (DerInteger)seq.ReadObject();
	}

	public Asn1SetParser GetDigestAlgorithms()
	{
		return (Asn1SetParser)_seq.ReadObject();
	}

	public ContentInfoParser GetEncapContentInfo()
	{
		return new ContentInfoParser((Asn1SequenceParser)_seq.ReadObject());
	}

	public Asn1SetParser GetCertificates()
	{
		_certsCalled = true;
		_nextObject = _seq.ReadObject();
		if (_nextObject is Asn1TaggedObjectParser && ((Asn1TaggedObjectParser)_nextObject).TagNo == 0)
		{
			Asn1SetParser result = (Asn1SetParser)((Asn1TaggedObjectParser)_nextObject).GetObjectParser(17, isExplicit: false);
			_nextObject = null;
			return result;
		}
		return null;
	}

	public Asn1SetParser GetCrls()
	{
		if (!_certsCalled)
		{
			throw new IOException("GetCerts() has not been called.");
		}
		_crlsCalled = true;
		if (_nextObject == null)
		{
			_nextObject = _seq.ReadObject();
		}
		if (_nextObject is Asn1TaggedObjectParser && ((Asn1TaggedObjectParser)_nextObject).TagNo == 1)
		{
			Asn1SetParser result = (Asn1SetParser)((Asn1TaggedObjectParser)_nextObject).GetObjectParser(17, isExplicit: false);
			_nextObject = null;
			return result;
		}
		return null;
	}

	public Asn1SetParser GetSignerInfos()
	{
		if (!_certsCalled || !_crlsCalled)
		{
			throw new IOException("GetCerts() and/or GetCrls() has not been called.");
		}
		if (_nextObject == null)
		{
			_nextObject = _seq.ReadObject();
		}
		return (Asn1SetParser)_nextObject;
	}
}
