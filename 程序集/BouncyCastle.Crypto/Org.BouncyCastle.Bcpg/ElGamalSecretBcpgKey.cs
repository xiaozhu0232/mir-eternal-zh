using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Bcpg;

public class ElGamalSecretBcpgKey : BcpgObject, IBcpgKey
{
	internal MPInteger x;

	public string Format => "PGP";

	public BigInteger X => x.Value;

	public ElGamalSecretBcpgKey(BcpgInputStream bcpgIn)
	{
		x = new MPInteger(bcpgIn);
	}

	public ElGamalSecretBcpgKey(BigInteger x)
	{
		this.x = new MPInteger(x);
	}

	public override byte[] GetEncoded()
	{
		try
		{
			return base.GetEncoded();
		}
		catch (Exception)
		{
			return null;
		}
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WriteObject(x);
	}
}
