using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Bcpg;

public class MPInteger : BcpgObject
{
	private readonly BigInteger val;

	public BigInteger Value => val;

	public MPInteger(BcpgInputStream bcpgIn)
	{
		if (bcpgIn == null)
		{
			throw new ArgumentNullException("bcpgIn");
		}
		int num = (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte();
		byte[] array = new byte[(num + 7) / 8];
		bcpgIn.ReadFully(array);
		val = new BigInteger(1, array);
	}

	public MPInteger(BigInteger val)
	{
		if (val == null)
		{
			throw new ArgumentNullException("val");
		}
		if (val.SignValue < 0)
		{
			throw new ArgumentException("Values must be positive", "val");
		}
		this.val = val;
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WriteShort((short)val.BitLength);
		bcpgOut.Write(val.ToByteArrayUnsigned());
	}

	internal static void Encode(BcpgOutputStream bcpgOut, BigInteger val)
	{
		bcpgOut.WriteShort((short)val.BitLength);
		bcpgOut.Write(val.ToByteArrayUnsigned());
	}
}
