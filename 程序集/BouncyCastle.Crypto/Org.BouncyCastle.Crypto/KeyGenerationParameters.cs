using System;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto;

public class KeyGenerationParameters
{
	private SecureRandom random;

	private int strength;

	public SecureRandom Random => random;

	public int Strength => strength;

	public KeyGenerationParameters(SecureRandom random, int strength)
	{
		if (random == null)
		{
			throw new ArgumentNullException("random");
		}
		if (strength < 1)
		{
			throw new ArgumentException("strength must be a positive value", "strength");
		}
		this.random = random;
		this.strength = strength;
	}
}
