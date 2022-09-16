using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Agreement;

public class DHBasicAgreement : IBasicAgreement
{
	private DHPrivateKeyParameters key;

	private DHParameters dhParams;

	public virtual void Init(ICipherParameters parameters)
	{
		if (parameters is ParametersWithRandom)
		{
			parameters = ((ParametersWithRandom)parameters).Parameters;
		}
		if (!(parameters is DHPrivateKeyParameters))
		{
			throw new ArgumentException("DHEngine expects DHPrivateKeyParameters");
		}
		key = (DHPrivateKeyParameters)parameters;
		dhParams = key.Parameters;
	}

	public virtual int GetFieldSize()
	{
		return (key.Parameters.P.BitLength + 7) / 8;
	}

	public virtual BigInteger CalculateAgreement(ICipherParameters pubKey)
	{
		if (key == null)
		{
			throw new InvalidOperationException("Agreement algorithm not initialised");
		}
		DHPublicKeyParameters dHPublicKeyParameters = (DHPublicKeyParameters)pubKey;
		if (!dHPublicKeyParameters.Parameters.Equals(dhParams))
		{
			throw new ArgumentException("Diffie-Hellman public key has wrong parameters.");
		}
		BigInteger p = dhParams.P;
		BigInteger y = dHPublicKeyParameters.Y;
		if (y == null || y.CompareTo(BigInteger.One) <= 0 || y.CompareTo(p.Subtract(BigInteger.One)) >= 0)
		{
			throw new ArgumentException("Diffie-Hellman public key is weak");
		}
		BigInteger bigInteger = y.ModPow(key.X, p);
		if (bigInteger.Equals(BigInteger.One))
		{
			throw new InvalidOperationException("Shared key can't be 1");
		}
		return bigInteger;
	}
}
