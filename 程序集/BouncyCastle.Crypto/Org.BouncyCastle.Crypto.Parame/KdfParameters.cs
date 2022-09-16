namespace Org.BouncyCastle.Crypto.Parameters;

public class KdfParameters : IDerivationParameters
{
	private byte[] iv;

	private byte[] shared;

	public KdfParameters(byte[] shared, byte[] iv)
	{
		this.shared = shared;
		this.iv = iv;
	}

	public byte[] GetSharedSecret()
	{
		return shared;
	}

	public byte[] GetIV()
	{
		return iv;
	}
}
