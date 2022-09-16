using System;

namespace Org.BouncyCastle.Math.EC;

public abstract class ECPointBase : ECPoint
{
	protected internal ECPointBase(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
	}

	protected internal ECPointBase(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	public override byte[] GetEncoded(bool compressed)
	{
		if (base.IsInfinity)
		{
			return new byte[1];
		}
		ECPoint eCPoint = Normalize();
		byte[] encoded = eCPoint.XCoord.GetEncoded();
		if (compressed)
		{
			byte[] array = new byte[encoded.Length + 1];
			array[0] = (byte)(eCPoint.CompressionYTilde ? 3u : 2u);
			Array.Copy(encoded, 0, array, 1, encoded.Length);
			return array;
		}
		byte[] encoded2 = eCPoint.YCoord.GetEncoded();
		byte[] array2 = new byte[encoded.Length + encoded2.Length + 1];
		array2[0] = 4;
		Array.Copy(encoded, 0, array2, 1, encoded.Length);
		Array.Copy(encoded2, 0, array2, encoded.Length + 1, encoded2.Length);
		return array2;
	}

	public override ECPoint Multiply(BigInteger k)
	{
		return Curve.GetMultiplier().Multiply(this, k);
	}
}
