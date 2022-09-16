namespace Org.BouncyCastle.Asn1;

public class BerSequenceParser : Asn1SequenceParser, IAsn1Convertible
{
	private readonly Asn1StreamParser _parser;

	internal BerSequenceParser(Asn1StreamParser parser)
	{
		_parser = parser;
	}

	public IAsn1Convertible ReadObject()
	{
		return _parser.ReadObject();
	}

	public Asn1Object ToAsn1Object()
	{
		return new BerSequence(_parser.ReadVector());
	}
}
