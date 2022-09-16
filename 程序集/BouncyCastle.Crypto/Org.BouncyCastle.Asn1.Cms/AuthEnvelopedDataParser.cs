namespace Org.BouncyCastle.Asn1.Cms;

public class AuthEnvelopedDataParser
{
	private Asn1SequenceParser seq;

	private DerInteger version;

	private IAsn1Convertible nextObject;

	private bool originatorInfoCalled;

	public DerInteger Version => version;

	public AuthEnvelopedDataParser(Asn1SequenceParser seq)
	{
		this.seq = seq;
		version = (DerInteger)seq.ReadObject();
	}

	public OriginatorInfo GetOriginatorInfo()
	{
		originatorInfoCalled = true;
		if (nextObject == null)
		{
			nextObject = seq.ReadObject();
		}
		if (nextObject is Asn1TaggedObjectParser && ((Asn1TaggedObjectParser)nextObject).TagNo == 0)
		{
			Asn1SequenceParser asn1SequenceParser = (Asn1SequenceParser)((Asn1TaggedObjectParser)nextObject).GetObjectParser(16, isExplicit: false);
			nextObject = null;
			return OriginatorInfo.GetInstance(asn1SequenceParser.ToAsn1Object());
		}
		return null;
	}

	public Asn1SetParser GetRecipientInfos()
	{
		if (!originatorInfoCalled)
		{
			GetOriginatorInfo();
		}
		if (nextObject == null)
		{
			nextObject = seq.ReadObject();
		}
		Asn1SetParser result = (Asn1SetParser)nextObject;
		nextObject = null;
		return result;
	}

	public EncryptedContentInfoParser GetAuthEncryptedContentInfo()
	{
		if (nextObject == null)
		{
			nextObject = seq.ReadObject();
		}
		if (nextObject != null)
		{
			Asn1SequenceParser asn1SequenceParser = (Asn1SequenceParser)nextObject;
			nextObject = null;
			return new EncryptedContentInfoParser(asn1SequenceParser);
		}
		return null;
	}

	public Asn1SetParser GetAuthAttrs()
	{
		if (nextObject == null)
		{
			nextObject = seq.ReadObject();
		}
		if (nextObject is Asn1TaggedObjectParser)
		{
			IAsn1Convertible asn1Convertible = nextObject;
			nextObject = null;
			return (Asn1SetParser)((Asn1TaggedObjectParser)asn1Convertible).GetObjectParser(17, isExplicit: false);
		}
		return null;
	}

	public Asn1OctetString GetMac()
	{
		if (nextObject == null)
		{
			nextObject = seq.ReadObject();
		}
		IAsn1Convertible asn1Convertible = nextObject;
		nextObject = null;
		return Asn1OctetString.GetInstance(asn1Convertible.ToAsn1Object());
	}

	public Asn1SetParser GetUnauthAttrs()
	{
		if (nextObject == null)
		{
			nextObject = seq.ReadObject();
		}
		if (nextObject != null)
		{
			IAsn1Convertible asn1Convertible = nextObject;
			nextObject = null;
			return (Asn1SetParser)((Asn1TaggedObjectParser)asn1Convertible).GetObjectParser(17, isExplicit: false);
		}
		return null;
	}
}
