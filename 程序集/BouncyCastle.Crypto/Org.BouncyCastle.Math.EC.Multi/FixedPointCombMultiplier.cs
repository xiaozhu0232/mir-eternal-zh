using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Multiplier;

public class FixedPointCombMultiplier : AbstractECMultiplier
{
	protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
	{
		ECCurve curve = p.Curve;
		int combSize = FixedPointUtilities.GetCombSize(curve);
		if (k.BitLength > combSize)
		{
			throw new InvalidOperationException("fixed-point comb doesn't support scalars larger than the curve order");
		}
		FixedPointPreCompInfo fixedPointPreCompInfo = FixedPointUtilities.Precompute(p);
		ECLookupTable lookupTable = fixedPointPreCompInfo.LookupTable;
		int width = fixedPointPreCompInfo.Width;
		int num = (combSize + width - 1) / width;
		ECPoint eCPoint = curve.Infinity;
		int num2 = num * width;
		uint[] array = Nat.FromBigInteger(num2, k);
		int num3 = num2 - 1;
		for (int i = 0; i < num; i++)
		{
			uint num4 = 0u;
			for (int num5 = num3 - i; num5 >= 0; num5 -= num)
			{
				uint num6 = array[num5 >> 5] >> num5;
				num4 ^= num6 >> 1;
				num4 <<= 1;
				num4 ^= num6;
			}
			ECPoint b = lookupTable.Lookup((int)num4);
			eCPoint = eCPoint.TwicePlus(b);
		}
		return eCPoint.Add(fixedPointPreCompInfo.Offset);
	}
}
