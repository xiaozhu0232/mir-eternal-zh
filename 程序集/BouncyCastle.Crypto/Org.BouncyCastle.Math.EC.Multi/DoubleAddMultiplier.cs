using System;

namespace Org.BouncyCastle.Math.EC.Multiplier;

[Obsolete("Will be removed")]
public class DoubleAddMultiplier : AbstractECMultiplier
{
	protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
	{
		ECPoint[] array = new ECPoint[2]
		{
			p.Curve.Infinity,
			p
		};
		int bitLength = k.BitLength;
		for (int i = 0; i < bitLength; i++)
		{
			int num = (k.TestBit(i) ? 1 : 0);
			int num2 = 1 - num;
			array[num2] = array[num2].TwicePlus(array[num]);
		}
		return array[0];
	}
}
