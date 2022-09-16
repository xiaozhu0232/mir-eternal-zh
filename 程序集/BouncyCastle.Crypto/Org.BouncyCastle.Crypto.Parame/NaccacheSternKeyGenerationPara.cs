using System;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters;

public class NaccacheSternKeyGenerationParameters : KeyGenerationParameters
{
	private readonly int certainty;

	private readonly int countSmallPrimes;

	public int Certainty => certainty;

	public int CountSmallPrimes => countSmallPrimes;

	[Obsolete("Remove: always false")]
	public bool IsDebug => false;

	public NaccacheSternKeyGenerationParameters(SecureRandom random, int strength, int certainty, int countSmallPrimes)
		: base(random, strength)
	{
		if (countSmallPrimes % 2 == 1)
		{
			throw new ArgumentException("countSmallPrimes must be a multiple of 2");
		}
		if (countSmallPrimes < 30)
		{
			throw new ArgumentException("countSmallPrimes must be >= 30 for security reasons");
		}
		this.certainty = certainty;
		this.countSmallPrimes = countSmallPrimes;
	}

	[Obsolete("Use version without 'debug' parameter")]
	public NaccacheSternKeyGenerationParameters(SecureRandom random, int strength, int certainty, int countSmallPrimes, bool debug)
		: this(random, strength, certainty, countSmallPrimes)
	{
	}
}
