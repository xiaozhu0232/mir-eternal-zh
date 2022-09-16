using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DHValidationParameters
{
	private readonly byte[] seed;

	private readonly int counter;

	public int Counter => counter;

	public DHValidationParameters(byte[] seed, int counter)
	{
		if (seed == null)
		{
			throw new ArgumentNullException("seed");
		}
		this.seed = (byte[])seed.Clone();
		this.counter = counter;
	}

	public byte[] GetSeed()
	{
		return (byte[])seed.Clone();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is DHValidationParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(DHValidationParameters other)
	{
		if (counter == other.counter)
		{
			return Arrays.AreEqual(seed, other.seed);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return counter.GetHashCode() ^ Arrays.GetHashCode(seed);
	}
}
