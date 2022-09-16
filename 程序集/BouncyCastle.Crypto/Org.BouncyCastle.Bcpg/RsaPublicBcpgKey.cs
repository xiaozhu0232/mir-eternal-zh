using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Bcpg;

public class RsaPublicBcpgKey : BcpgObject, IBcpgKey
{
	private readonly MPInteger n;

	private readonly MPInteger e;

	public BigInteger PublicExponent => e.Value;

	public BigInteger Modulus => n.Value;

	public string Format => "PGP";

	public RsaPublicBcpgKey(BcpgInputStream bcpgIn)
	{
		n = new MPInteger(bcpgIn);
		e = new MPInteger(bcpgIn);
	}

	public RsaPublicBcpgKey(BigInteger n, BigInteger e)
	{
		this.n = new MPInteger(n);
		this.e = new MPInteger(e);
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
		bcpgOut.WriteObjects(n, e);
	}
}
