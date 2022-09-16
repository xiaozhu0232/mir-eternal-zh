namespace Org.BouncyCastle.Crypto.Generators;

public class Kdf1BytesGenerator : BaseKdfBytesGenerator
{
	public Kdf1BytesGenerator(IDigest digest)
		: base(0, digest)
	{
	}
}
