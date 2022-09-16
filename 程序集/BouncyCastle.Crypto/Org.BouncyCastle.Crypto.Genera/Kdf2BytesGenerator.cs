namespace Org.BouncyCastle.Crypto.Generators;

public class Kdf2BytesGenerator : BaseKdfBytesGenerator
{
	public Kdf2BytesGenerator(IDigest digest)
		: base(1, digest)
	{
	}
}
