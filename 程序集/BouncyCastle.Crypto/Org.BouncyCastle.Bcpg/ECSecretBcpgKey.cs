using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Bcpg;

public class ECSecretBcpgKey : BcpgObject, IBcpgKey
{
	internal MPInteger x;

	public string Format => "PGP";

	public virtual BigInteger X => x.Value;

	public ECSecretBcpgKey(BcpgInputStream bcpgIn)
	{
		x = new MPInteger(bcpgIn);
	}

	public ECSecretBcpgKey(BigInteger x)
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
