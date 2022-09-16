using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters;

public class RsaKeyParameters : AsymmetricKeyParameter
{
	private static readonly BigInteger SmallPrimesProduct = new BigInteger("8138e8a0fcf3a4e84a771d40fd305d7f4aa59306d7251de54d98af8fe95729a1f73d893fa424cd2edc8636a6c3285e022b0e3866a565ae8108eed8591cd4fe8d2ce86165a978d719ebf647f362d33fca29cd179fb42401cbaf3df0c614056f9c8f3cfd51e474afb6bc6974f78db8aba8e9e517fded658591ab7502bd41849462f", 16);

	private readonly BigInteger modulus;

	private readonly BigInteger exponent;

	public BigInteger Modulus => modulus;

	public BigInteger Exponent => exponent;

	private static BigInteger Validate(BigInteger modulus)
	{
		if ((modulus.IntValue & 1) == 0)
		{
			throw new ArgumentException("RSA modulus is even", "modulus");
		}
		if (!modulus.Gcd(SmallPrimesProduct).Equals(BigInteger.One))
		{
			throw new ArgumentException("RSA modulus has a small prime factor");
		}
		return modulus;
	}

	public RsaKeyParameters(bool isPrivate, BigInteger modulus, BigInteger exponent)
		: base(isPrivate)
	{
		if (modulus == null)
		{
			throw new ArgumentNullException("modulus");
		}
		if (exponent == null)
		{
			throw new ArgumentNullException("exponent");
		}
		if (modulus.SignValue <= 0)
		{
			throw new ArgumentException("Not a valid RSA modulus", "modulus");
		}
		if (exponent.SignValue <= 0)
		{
			throw new ArgumentException("Not a valid RSA exponent", "exponent");
		}
		if (!isPrivate && (exponent.IntValue & 1) == 0)
		{
			throw new ArgumentException("RSA publicExponent is even", "exponent");
		}
		this.modulus = Validate(modulus);
		this.exponent = exponent;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RsaKeyParameters rsaKeyParameters))
		{
			return false;
		}
		if (rsaKeyParameters.IsPrivate == base.IsPrivate && rsaKeyParameters.Modulus.Equals(modulus))
		{
			return rsaKeyParameters.Exponent.Equals(exponent);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return modulus.GetHashCode() ^ exponent.GetHashCode() ^ base.IsPrivate.GetHashCode();
	}
}
