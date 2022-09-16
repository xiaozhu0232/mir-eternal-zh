using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Signers;

public class RandomDsaKCalculator : IDsaKCalculator
{
	private BigInteger q;

	private SecureRandom random;

	public virtual bool IsDeterministic => false;

	public virtual void Init(BigInteger n, SecureRandom random)
	{
		q = n;
		this.random = random;
	}

	public virtual void Init(BigInteger n, BigInteger d, byte[] message)
	{
		throw new InvalidOperationException("Operation not supported");
	}

	public virtual BigInteger NextK()
	{
		int bitLength = q.BitLength;
		BigInteger bigInteger;
		do
		{
			bigInteger = new BigInteger(bitLength, random);
		}
		while (bigInteger.SignValue < 1 || bigInteger.CompareTo(q) >= 0);
		return bigInteger;
	}
}
