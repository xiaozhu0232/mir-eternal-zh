namespace Org.BouncyCastle.Crypto.Parameters;

public class Iso18033KdfParameters : IDerivationParameters
{
	private byte[] seed;

	public Iso18033KdfParameters(byte[] seed)
	{
		this.seed = seed;
	}

	public byte[] GetSeed()
	{
		return seed;
	}
}
