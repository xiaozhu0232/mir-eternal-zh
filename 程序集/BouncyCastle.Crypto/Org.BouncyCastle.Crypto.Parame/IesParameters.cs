namespace Org.BouncyCastle.Crypto.Parameters;

public class IesParameters : ICipherParameters
{
	private byte[] derivation;

	private byte[] encoding;

	private int macKeySize;

	public int MacKeySize => macKeySize;

	public IesParameters(byte[] derivation, byte[] encoding, int macKeySize)
	{
		this.derivation = derivation;
		this.encoding = encoding;
		this.macKeySize = macKeySize;
	}

	public byte[] GetDerivationV()
	{
		return derivation;
	}

	public byte[] GetEncodingV()
	{
		return encoding;
	}
}
