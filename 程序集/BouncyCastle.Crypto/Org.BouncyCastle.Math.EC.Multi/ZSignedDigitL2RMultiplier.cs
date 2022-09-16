using System;

namespace Org.BouncyCastle.Math.EC.Multiplier;

[Obsolete("Will be removed")]
public class ZSignedDigitL2RMultiplier : AbstractECMultiplier
{
	protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
	{
		ECPoint eCPoint = p.Normalize();
		ECPoint eCPoint2 = eCPoint.Negate();
		ECPoint eCPoint3 = eCPoint;
		int bitLength = k.BitLength;
		int lowestSetBit = k.GetLowestSetBit();
		int num = bitLength;
		while (--num > lowestSetBit)
		{
			eCPoint3 = eCPoint3.TwicePlus(k.TestBit(num) ? eCPoint : eCPoint2);
		}
		return eCPoint3.TimesPow2(lowestSetBit);
	}
}
