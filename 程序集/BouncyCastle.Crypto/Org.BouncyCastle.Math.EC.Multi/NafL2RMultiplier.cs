using System;

namespace Org.BouncyCastle.Math.EC.Multiplier;

[Obsolete("Will be removed")]
public class NafL2RMultiplier : AbstractECMultiplier
{
	protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
	{
		int[] array = WNafUtilities.GenerateCompactNaf(k);
		ECPoint eCPoint = p.Normalize();
		ECPoint eCPoint2 = eCPoint.Negate();
		ECPoint eCPoint3 = p.Curve.Infinity;
		int num = array.Length;
		while (--num >= 0)
		{
			int num2 = array[num];
			int num3 = num2 >> 16;
			int e = num2 & 0xFFFF;
			eCPoint3 = eCPoint3.TwicePlus((num3 < 0) ? eCPoint2 : eCPoint);
			eCPoint3 = eCPoint3.TimesPow2(e);
		}
		return eCPoint3;
	}
}
