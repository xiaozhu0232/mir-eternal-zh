using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class DsaValidationParameters
{
	private readonly byte[] seed;

	private readonly int counter;

	private readonly int usageIndex;

	public virtual int Counter => counter;

	public virtual int UsageIndex => usageIndex;

	public DsaValidationParameters(byte[] seed, int counter)
		: this(seed, counter, -1)
	{
	}

	public DsaValidationParameters(byte[] seed, int counter, int usageIndex)
	{
		if (seed == null)
		{
			throw new ArgumentNullException("seed");
		}
		this.seed = (byte[])seed.Clone();
		this.counter = counter;
		this.usageIndex = usageIndex;
	}

	public virtual byte[] GetSeed()
	{
		return (byte[])seed.Clone();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is DsaValidationParameters other))
		{
			return false;
		}
		return Equals(other);
	}

	protected virtual bool Equals(DsaValidationParameters other)
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
