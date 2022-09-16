using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Bcpg;

public abstract class ECPublicBcpgKey : BcpgObject, IBcpgKey
{
	internal DerObjectIdentifier oid;

	internal BigInteger point;

	public string Format => "PGP";

	public virtual BigInteger EncodedPoint => point;

	public virtual DerObjectIdentifier CurveOid => oid;

	protected ECPublicBcpgKey(BcpgInputStream bcpgIn)
	{
		oid = DerObjectIdentifier.GetInstance(Asn1Object.FromByteArray(ReadBytesOfEncodedLength(bcpgIn)));
		point = new MPInteger(bcpgIn).Value;
	}

	protected ECPublicBcpgKey(DerObjectIdentifier oid, ECPoint point)
	{
		this.point = new BigInteger(1, point.GetEncoded(compressed: false));
		this.oid = oid;
	}

	protected ECPublicBcpgKey(DerObjectIdentifier oid, BigInteger encodedPoint)
	{
		point = encodedPoint;
		this.oid = oid;
	}

	public override byte[] GetEncoded()
	{
		try
		{
			return base.GetEncoded();
		}
		catch (IOException)
		{
			return null;
		}
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		byte[] encoded = oid.GetEncoded();
		bcpgOut.Write(encoded, 1, encoded.Length - 1);
		MPInteger bcpgObject = new MPInteger(point);
		bcpgOut.WriteObject(bcpgObject);
	}

	protected static byte[] ReadBytesOfEncodedLength(BcpgInputStream bcpgIn)
	{
		int num = bcpgIn.ReadByte();
		if (num < 0)
		{
			throw new EndOfStreamException();
		}
		if (num == 0 || num == 255)
		{
			throw new IOException("future extensions not yet implemented");
		}
		if (num > 127)
		{
			throw new IOException("unsupported OID");
		}
		byte[] array = new byte[num + 2];
		bcpgIn.ReadFully(array, 2, array.Length - 2);
		array[0] = 6;
		array[1] = (byte)num;
		return array;
	}
}
