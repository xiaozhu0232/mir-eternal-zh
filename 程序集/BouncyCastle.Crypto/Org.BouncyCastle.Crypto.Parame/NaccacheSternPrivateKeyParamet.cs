using System;
using System.Collections;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class NaccacheSternPrivateKeyParameters : NaccacheSternKeyParameters
{
	private readonly BigInteger phiN;

	private readonly IList smallPrimes;

	public BigInteger PhiN => phiN;

	[Obsolete("Use 'SmallPrimesList' instead")]
	public ArrayList SmallPrimes => new ArrayList(smallPrimes);

	public IList SmallPrimesList => smallPrimes;

	[Obsolete]
	public NaccacheSternPrivateKeyParameters(BigInteger g, BigInteger n, int lowerSigmaBound, ArrayList smallPrimes, BigInteger phiN)
		: base(privateKey: true, g, n, lowerSigmaBound)
	{
		this.smallPrimes = smallPrimes;
		this.phiN = phiN;
	}

	public NaccacheSternPrivateKeyParameters(BigInteger g, BigInteger n, int lowerSigmaBound, IList smallPrimes, BigInteger phiN)
		: base(privateKey: true, g, n, lowerSigmaBound)
	{
		this.smallPrimes = smallPrimes;
		this.phiN = phiN;
	}
}
