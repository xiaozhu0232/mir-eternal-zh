using System;

namespace Org.BouncyCastle.Crypto.Parameters;

public class SM2KeyExchangePublicParameters : ICipherParameters
{
	private readonly ECPublicKeyParameters mStaticPublicKey;

	private readonly ECPublicKeyParameters mEphemeralPublicKey;

	public virtual ECPublicKeyParameters StaticPublicKey => mStaticPublicKey;

	public virtual ECPublicKeyParameters EphemeralPublicKey => mEphemeralPublicKey;

	public SM2KeyExchangePublicParameters(ECPublicKeyParameters staticPublicKey, ECPublicKeyParameters ephemeralPublicKey)
	{
		if (staticPublicKey == null)
		{
			throw new ArgumentNullException("staticPublicKey");
		}
		if (ephemeralPublicKey == null)
		{
			throw new ArgumentNullException("ephemeralPublicKey");
		}
		if (!staticPublicKey.Parameters.Equals(ephemeralPublicKey.Parameters))
		{
			throw new ArgumentException("Static and ephemeral public keys have different domain parameters");
		}
		mStaticPublicKey = staticPublicKey;
		mEphemeralPublicKey = ephemeralPublicKey;
	}
}
