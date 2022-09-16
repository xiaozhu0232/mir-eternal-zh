using System;

namespace Org.BouncyCastle.Crypto;

public class AsymmetricCipherKeyPair
{
	private readonly AsymmetricKeyParameter publicParameter;

	private readonly AsymmetricKeyParameter privateParameter;

	public AsymmetricKeyParameter Public => publicParameter;

	public AsymmetricKeyParameter Private => privateParameter;

	public AsymmetricCipherKeyPair(AsymmetricKeyParameter publicParameter, AsymmetricKeyParameter privateParameter)
	{
		if (publicParameter.IsPrivate)
		{
			throw new ArgumentException("Expected a public key", "publicParameter");
		}
		if (!privateParameter.IsPrivate)
		{
			throw new ArgumentException("Expected a private key", "privateParameter");
		}
		this.publicParameter = publicParameter;
		this.privateParameter = privateParameter;
	}
}
