using System.IO;

namespace Org.BouncyCastle.Asn1;

public class BerSequenceGenerator : BerGenerator
{
	public BerSequenceGenerator(Stream outStream)
		: base(outStream)
	{
		WriteBerHeader(48);
	}

	public BerSequenceGenerator(Stream outStream, int tagNo, bool isExplicit)
		: base(outStream, tagNo, isExplicit)
	{
		WriteBerHeader(48);
	}
}
