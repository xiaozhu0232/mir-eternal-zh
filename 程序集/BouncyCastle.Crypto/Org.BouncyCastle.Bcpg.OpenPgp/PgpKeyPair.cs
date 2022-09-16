using System;
using Org.BouncyCastle.Crypto;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpKeyPair
{
	private readonly PgpPublicKey pub;

	private readonly PgpPrivateKey priv;

	public long KeyId => pub.KeyId;

	public PgpPublicKey PublicKey => pub;

	public PgpPrivateKey PrivateKey => priv;

	public PgpKeyPair(PublicKeyAlgorithmTag algorithm, AsymmetricCipherKeyPair keyPair, DateTime time)
		: this(algorithm, keyPair.Public, keyPair.Private, time)
	{
	}

	public PgpKeyPair(PublicKeyAlgorithmTag algorithm, AsymmetricKeyParameter pubKey, AsymmetricKeyParameter privKey, DateTime time)
	{
		pub = new PgpPublicKey(algorithm, pubKey, time);
		priv = new PgpPrivateKey(pub.KeyId, pub.PublicKeyPacket, privKey);
	}

	public PgpKeyPair(PgpPublicKey pub, PgpPrivateKey priv)
	{
		this.pub = pub;
		this.priv = priv;
	}
}
