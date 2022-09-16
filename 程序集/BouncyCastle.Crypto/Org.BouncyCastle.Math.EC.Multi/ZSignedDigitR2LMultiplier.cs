using System;

namespace Org.BouncyCastle.Math.EC.Multiplier;

[Obsolete("Will be removed")]
public class ZSignedDigitR2LMultiplier : AbstractECMultiplier
{
	protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
	{
		ECPoint eCPoint = p.Curve.Infinity;
		ECPoint eCPoint2 = p;
		int bitLength = k.BitLength;
		int lowestSetBit = k.GetLowestSetBit();
		eCPoint2 = eCPoint2.TimesPow2(lowestSetBit);
		int num = lowestSetBit;
		while (++num < bitLength)
		{
			eCPoint = eCPoint.Add(k.TestBit(num) ? eCPoint2 : eCPoint2.Negate());
			eCPoint2 = eCPoint2.Twice();
		}
		return eCPoint.Add(eCPoint2);
	}
}
