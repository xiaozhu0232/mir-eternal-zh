using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Multiplier;

public class WNafL2RMultiplier : AbstractECMultiplier
{
	protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
	{
		int windowSize = WNafUtilities.GetWindowSize(k.BitLength);
		WNafPreCompInfo wNafPreCompInfo = WNafUtilities.Precompute(p, windowSize, includeNegated: true);
		ECPoint[] preComp = wNafPreCompInfo.PreComp;
		ECPoint[] preCompNeg = wNafPreCompInfo.PreCompNeg;
		int width = wNafPreCompInfo.Width;
		int[] array = WNafUtilities.GenerateCompactWindowNaf(width, k);
		ECPoint eCPoint = p.Curve.Infinity;
		int num = array.Length;
		if (num > 1)
		{
			int num2 = array[--num];
			int num3 = num2 >> 16;
			int num4 = num2 & 0xFFFF;
			int num5 = System.Math.Abs(num3);
			ECPoint[] array2 = ((num3 < 0) ? preCompNeg : preComp);
			if (num5 << 2 < 1 << width)
			{
				int num6 = 32 - Integers.NumberOfLeadingZeros(num5);
				int num7 = width - num6;
				int num8 = num5 ^ (1 << num6 - 1);
				int num9 = (1 << width - 1) - 1;
				int num10 = (num8 << num7) + 1;
				eCPoint = array2[num9 >> 1].Add(array2[num10 >> 1]);
				num4 -= num7;
			}
			else
			{
				eCPoint = array2[num5 >> 1];
			}
			eCPoint = eCPoint.TimesPow2(num4);
		}
		while (num > 0)
		{
			int num11 = array[--num];
			int num12 = num11 >> 16;
			int e = num11 & 0xFFFF;
			int num13 = System.Math.Abs(num12);
			ECPoint[] array3 = ((num12 < 0) ? preCompNeg : preComp);
			ECPoint b = array3[num13 >> 1];
			eCPoint = eCPoint.TwicePlus(b);
			eCPoint = eCPoint.TimesPow2(e);
		}
		return eCPoint;
	}
}
