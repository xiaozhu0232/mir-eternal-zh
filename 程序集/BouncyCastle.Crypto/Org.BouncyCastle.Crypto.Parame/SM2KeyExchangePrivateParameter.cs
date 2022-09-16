using System;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Multiplier;

namespace Org.BouncyCastle.Crypto.Parameters;

public class SM2KeyExchangePrivateParameters : ICipherParameters
{
	private readonly bool mInitiator;

	private readonly ECPrivateKeyParameters mStaticPrivateKey;

	private readonly ECPoint mStaticPublicPoint;

	private readonly ECPrivateKeyParameters mEphemeralPrivateKey;

	private readonly ECPoint mEphemeralPublicPoint;

	public virtual bool IsInitiator => mInitiator;

	public virtual ECPrivateKeyParameters StaticPrivateKey => mStaticPrivateKey;

	public virtual ECPoint StaticPublicPoint => mStaticPublicPoint;

	public virtual ECPrivateKeyParameters EphemeralPrivateKey => mEphemeralPrivateKey;

	public virtual ECPoint EphemeralPublicPoint => mEphemeralPublicPoint;

	public SM2KeyExchangePrivateParameters(bool initiator, ECPrivateKeyParameters staticPrivateKey, ECPrivateKeyParameters ephemeralPrivateKey)
	{
		if (staticPrivateKey == null)
		{
			throw new ArgumentNullException("staticPrivateKey");
		}
		if (ephemeralPrivateKey == null)
		{
			throw new ArgumentNullException("ephemeralPrivateKey");
		}
		ECDomainParameters parameters = staticPrivateKey.Parameters;
		if (!parameters.Equals(ephemeralPrivateKey.Parameters))
		{
			throw new ArgumentException("Static and ephemeral private keys have different domain parameters");
		}
		ECMultiplier eCMultiplier = new FixedPointCombMultiplier();
		mInitiator = initiator;
		mStaticPrivateKey = staticPrivateKey;
		mStaticPublicPoint = eCMultiplier.Multiply(parameters.G, staticPrivateKey.D).Normalize();
		mEphemeralPrivateKey = ephemeralPrivateKey;
		mEphemeralPublicPoint = eCMultiplier.Multiply(parameters.G, ephemeralPrivateKey.D).Normalize();
	}
}
