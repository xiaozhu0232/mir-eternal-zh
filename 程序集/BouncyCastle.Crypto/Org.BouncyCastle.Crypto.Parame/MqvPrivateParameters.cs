using System;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Multiplier;

namespace Org.BouncyCastle.Crypto.Parameters;

public class MqvPrivateParameters : ICipherParameters
{
	private readonly ECPrivateKeyParameters staticPrivateKey;

	private readonly ECPrivateKeyParameters ephemeralPrivateKey;

	private readonly ECPublicKeyParameters ephemeralPublicKey;

	public virtual ECPrivateKeyParameters StaticPrivateKey => staticPrivateKey;

	public virtual ECPrivateKeyParameters EphemeralPrivateKey => ephemeralPrivateKey;

	public virtual ECPublicKeyParameters EphemeralPublicKey => ephemeralPublicKey;

	public MqvPrivateParameters(ECPrivateKeyParameters staticPrivateKey, ECPrivateKeyParameters ephemeralPrivateKey)
		: this(staticPrivateKey, ephemeralPrivateKey, null)
	{
	}

	public MqvPrivateParameters(ECPrivateKeyParameters staticPrivateKey, ECPrivateKeyParameters ephemeralPrivateKey, ECPublicKeyParameters ephemeralPublicKey)
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
		if (ephemeralPublicKey == null)
		{
			ECPoint q = new FixedPointCombMultiplier().Multiply(parameters.G, ephemeralPrivateKey.D);
			ephemeralPublicKey = new ECPublicKeyParameters(q, parameters);
		}
		else if (!parameters.Equals(ephemeralPublicKey.Parameters))
		{
			throw new ArgumentException("Ephemeral public key has different domain parameters");
		}
		this.staticPrivateKey = staticPrivateKey;
		this.ephemeralPrivateKey = ephemeralPrivateKey;
		this.ephemeralPublicKey = ephemeralPublicKey;
	}
}
