using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class NaccacheSternKeyParameters : AsymmetricKeyParameter
{
	private readonly BigInteger g;

	private readonly BigInteger n;

	private readonly int lowerSigmaBound;

	public BigInteger G => g;

	public int LowerSigmaBound => lowerSigmaBound;

	public BigInteger Modulus => n;

	public NaccacheSternKeyParameters(bool privateKey, BigInteger g, BigInteger n, int lowerSigmaBound)
		: base(privateKey)
	{
		this.g = g;
		this.n = n;
		this.lowerSigmaBound = lowerSigmaBound;
	}
}
