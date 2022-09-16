using System;
using Org.BouncyCastle.Math.EC.Endo;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Math.Field;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC;

public class ECAlgorithms
{
	public static bool IsF2mCurve(ECCurve c)
	{
		return IsF2mField(c.Field);
	}

	public static bool IsF2mField(IFiniteField field)
	{
		if (field.Dimension > 1 && field.Characteristic.Equals(BigInteger.Two))
		{
			return field is IPolynomialExtensionField;
		}
		return false;
	}

	public static bool IsFpCurve(ECCurve c)
	{
		return IsFpField(c.Field);
	}

	public static bool IsFpField(IFiniteField field)
	{
		return field.Dimension == 1;
	}

	public static ECPoint SumOfMultiplies(ECPoint[] ps, BigInteger[] ks)
	{
		if (ps == null || ks == null || ps.Length != ks.Length || ps.Length < 1)
		{
			throw new ArgumentException("point and scalar arrays should be non-null, and of equal, non-zero, length");
		}
		int num = ps.Length;
		switch (num)
		{
		case 1:
			return ps[0].Multiply(ks[0]);
		case 2:
			return SumOfTwoMultiplies(ps[0], ks[0], ps[1], ks[1]);
		default:
		{
			ECPoint eCPoint = ps[0];
			ECCurve curve = eCPoint.Curve;
			ECPoint[] array = new ECPoint[num];
			array[0] = eCPoint;
			for (int i = 1; i < num; i++)
			{
				array[i] = ImportPoint(curve, ps[i]);
			}
			if (curve.GetEndomorphism() is GlvEndomorphism glvEndomorphism)
			{
				return ImplCheckResult(ImplSumOfMultipliesGlv(array, ks, glvEndomorphism));
			}
			return ImplCheckResult(ImplSumOfMultiplies(array, ks));
		}
		}
	}

	public static ECPoint SumOfTwoMultiplies(ECPoint P, BigInteger a, ECPoint Q, BigInteger b)
	{
		ECCurve curve = P.Curve;
		Q = ImportPoint(curve, Q);
		if (curve is AbstractF2mCurve abstractF2mCurve && abstractF2mCurve.IsKoblitz)
		{
			return ImplCheckResult(P.Multiply(a).Add(Q.Multiply(b)));
		}
		if (curve.GetEndomorphism() is GlvEndomorphism glvEndomorphism)
		{
			return ImplCheckResult(ImplSumOfMultipliesGlv(new ECPoint[2] { P, Q }, new BigInteger[2] { a, b }, glvEndomorphism));
		}
		return ImplCheckResult(ImplShamirsTrickWNaf(P, a, Q, b));
	}

	public static ECPoint ShamirsTrick(ECPoint P, BigInteger k, ECPoint Q, BigInteger l)
	{
		ECCurve curve = P.Curve;
		Q = ImportPoint(curve, Q);
		return ImplCheckResult(ImplShamirsTrickJsf(P, k, Q, l));
	}

	public static ECPoint ImportPoint(ECCurve c, ECPoint p)
	{
		ECCurve curve = p.Curve;
		if (!c.Equals(curve))
		{
			throw new ArgumentException("Point must be on the same curve");
		}
		return c.ImportPoint(p);
	}

	public static void MontgomeryTrick(ECFieldElement[] zs, int off, int len)
	{
		MontgomeryTrick(zs, off, len, null);
	}

	public static void MontgomeryTrick(ECFieldElement[] zs, int off, int len, ECFieldElement scale)
	{
		ECFieldElement[] array = new ECFieldElement[len];
		array[0] = zs[off];
		int num = 0;
		while (++num < len)
		{
			array[num] = array[num - 1].Multiply(zs[off + num]);
		}
		num--;
		if (scale != null)
		{
			array[num] = array[num].Multiply(scale);
		}
		ECFieldElement eCFieldElement = array[num].Invert();
		while (num > 0)
		{
			int num2 = off + num--;
			ECFieldElement b = zs[num2];
			zs[num2] = array[num].Multiply(eCFieldElement);
			eCFieldElement = eCFieldElement.Multiply(b);
		}
		zs[off] = eCFieldElement;
	}

	public static ECPoint ReferenceMultiply(ECPoint p, BigInteger k)
	{
		BigInteger bigInteger = k.Abs();
		ECPoint eCPoint = p.Curve.Infinity;
		int bitLength = bigInteger.BitLength;
		if (bitLength > 0)
		{
			if (bigInteger.TestBit(0))
			{
				eCPoint = p;
			}
			for (int i = 1; i < bitLength; i++)
			{
				p = p.Twice();
				if (bigInteger.TestBit(i))
				{
					eCPoint = eCPoint.Add(p);
				}
			}
		}
		if (k.SignValue >= 0)
		{
			return eCPoint;
		}
		return eCPoint.Negate();
	}

	public static ECPoint ValidatePoint(ECPoint p)
	{
		if (!p.IsValid())
		{
			throw new InvalidOperationException("Invalid point");
		}
		return p;
	}

	public static ECPoint CleanPoint(ECCurve c, ECPoint p)
	{
		ECCurve curve = p.Curve;
		if (!c.Equals(curve))
		{
			throw new ArgumentException("Point must be on the same curve", "p");
		}
		return c.DecodePoint(p.GetEncoded(compressed: false));
	}

	internal static ECPoint ImplCheckResult(ECPoint p)
	{
		if (!p.IsValidPartial())
		{
			throw new InvalidOperationException("Invalid result");
		}
		return p;
	}

	internal static ECPoint ImplShamirsTrickJsf(ECPoint P, BigInteger k, ECPoint Q, BigInteger l)
	{
		ECCurve curve = P.Curve;
		ECPoint infinity = curve.Infinity;
		ECPoint eCPoint = P.Add(Q);
		ECPoint eCPoint2 = P.Subtract(Q);
		ECPoint[] array = new ECPoint[4] { Q, eCPoint2, P, eCPoint };
		curve.NormalizeAll(array);
		ECPoint[] array2 = new ECPoint[9]
		{
			array[3].Negate(),
			array[2].Negate(),
			array[1].Negate(),
			array[0].Negate(),
			infinity,
			array[0],
			array[1],
			array[2],
			array[3]
		};
		byte[] array3 = WNafUtilities.GenerateJsf(k, l);
		ECPoint eCPoint3 = infinity;
		int num = array3.Length;
		while (--num >= 0)
		{
			int num2 = array3[num];
			int num3 = num2 << 24 >> 28;
			int num4 = num2 << 28 >> 28;
			int num5 = 4 + num3 * 3 + num4;
			eCPoint3 = eCPoint3.TwicePlus(array2[num5]);
		}
		return eCPoint3;
	}

	internal static ECPoint ImplShamirsTrickWNaf(ECPoint P, BigInteger k, ECPoint Q, BigInteger l)
	{
		bool flag = k.SignValue < 0;
		bool flag2 = l.SignValue < 0;
		BigInteger bigInteger = k.Abs();
		BigInteger bigInteger2 = l.Abs();
		int windowSize = WNafUtilities.GetWindowSize(bigInteger.BitLength, 8);
		int windowSize2 = WNafUtilities.GetWindowSize(bigInteger2.BitLength, 8);
		WNafPreCompInfo wNafPreCompInfo = WNafUtilities.Precompute(P, windowSize, includeNegated: true);
		WNafPreCompInfo wNafPreCompInfo2 = WNafUtilities.Precompute(Q, windowSize2, includeNegated: true);
		ECCurve curve = P.Curve;
		int combSize = FixedPointUtilities.GetCombSize(curve);
		if (!flag && !flag2 && k.BitLength <= combSize && l.BitLength <= combSize && wNafPreCompInfo.IsPromoted && wNafPreCompInfo2.IsPromoted)
		{
			return ImplShamirsTrickFixedPoint(P, k, Q, l);
		}
		int width = System.Math.Min(8, wNafPreCompInfo.Width);
		int width2 = System.Math.Min(8, wNafPreCompInfo2.Width);
		ECPoint[] preCompP = (flag ? wNafPreCompInfo.PreCompNeg : wNafPreCompInfo.PreComp);
		ECPoint[] preCompQ = (flag2 ? wNafPreCompInfo2.PreCompNeg : wNafPreCompInfo2.PreComp);
		ECPoint[] preCompNegP = (flag ? wNafPreCompInfo.PreComp : wNafPreCompInfo.PreCompNeg);
		ECPoint[] preCompNegQ = (flag2 ? wNafPreCompInfo2.PreComp : wNafPreCompInfo2.PreCompNeg);
		byte[] wnafP = WNafUtilities.GenerateWindowNaf(width, bigInteger);
		byte[] wnafQ = WNafUtilities.GenerateWindowNaf(width2, bigInteger2);
		return ImplShamirsTrickWNaf(preCompP, preCompNegP, wnafP, preCompQ, preCompNegQ, wnafQ);
	}

	internal static ECPoint ImplShamirsTrickWNaf(ECEndomorphism endomorphism, ECPoint P, BigInteger k, BigInteger l)
	{
		bool flag = k.SignValue < 0;
		bool flag2 = l.SignValue < 0;
		k = k.Abs();
		l = l.Abs();
		int windowSize = WNafUtilities.GetWindowSize(System.Math.Max(k.BitLength, l.BitLength), 8);
		WNafPreCompInfo wNafPreCompInfo = WNafUtilities.Precompute(P, windowSize, includeNegated: true);
		ECPoint p = EndoUtilities.MapPoint(endomorphism, P);
		WNafPreCompInfo wNafPreCompInfo2 = WNafUtilities.PrecomputeWithPointMap(p, endomorphism.PointMap, wNafPreCompInfo, includeNegated: true);
		int width = System.Math.Min(8, wNafPreCompInfo.Width);
		int width2 = System.Math.Min(8, wNafPreCompInfo2.Width);
		ECPoint[] preCompP = (flag ? wNafPreCompInfo.PreCompNeg : wNafPreCompInfo.PreComp);
		ECPoint[] preCompQ = (flag2 ? wNafPreCompInfo2.PreCompNeg : wNafPreCompInfo2.PreComp);
		ECPoint[] preCompNegP = (flag ? wNafPreCompInfo.PreComp : wNafPreCompInfo.PreCompNeg);
		ECPoint[] preCompNegQ = (flag2 ? wNafPreCompInfo2.PreComp : wNafPreCompInfo2.PreCompNeg);
		byte[] wnafP = WNafUtilities.GenerateWindowNaf(width, k);
		byte[] wnafQ = WNafUtilities.GenerateWindowNaf(width2, l);
		return ImplShamirsTrickWNaf(preCompP, preCompNegP, wnafP, preCompQ, preCompNegQ, wnafQ);
	}

	private static ECPoint ImplShamirsTrickWNaf(ECPoint[] preCompP, ECPoint[] preCompNegP, byte[] wnafP, ECPoint[] preCompQ, ECPoint[] preCompNegQ, byte[] wnafQ)
	{
		int num = System.Math.Max(wnafP.Length, wnafQ.Length);
		ECCurve curve = preCompP[0].Curve;
		ECPoint infinity = curve.Infinity;
		ECPoint eCPoint = infinity;
		int num2 = 0;
		for (int num3 = num - 1; num3 >= 0; num3--)
		{
			int num4 = ((num3 < wnafP.Length) ? ((sbyte)wnafP[num3]) : 0);
			int num5 = ((num3 < wnafQ.Length) ? ((sbyte)wnafQ[num3]) : 0);
			if ((num4 | num5) == 0)
			{
				num2++;
			}
			else
			{
				ECPoint eCPoint2 = infinity;
				if (num4 != 0)
				{
					int num6 = System.Math.Abs(num4);
					ECPoint[] array = ((num4 < 0) ? preCompNegP : preCompP);
					eCPoint2 = eCPoint2.Add(array[num6 >> 1]);
				}
				if (num5 != 0)
				{
					int num7 = System.Math.Abs(num5);
					ECPoint[] array2 = ((num5 < 0) ? preCompNegQ : preCompQ);
					eCPoint2 = eCPoint2.Add(array2[num7 >> 1]);
				}
				if (num2 > 0)
				{
					eCPoint = eCPoint.TimesPow2(num2);
					num2 = 0;
				}
				eCPoint = eCPoint.TwicePlus(eCPoint2);
			}
		}
		if (num2 > 0)
		{
			eCPoint = eCPoint.TimesPow2(num2);
		}
		return eCPoint;
	}

	internal static ECPoint ImplSumOfMultiplies(ECPoint[] ps, BigInteger[] ks)
	{
		int num = ps.Length;
		bool[] array = new bool[num];
		WNafPreCompInfo[] array2 = new WNafPreCompInfo[num];
		byte[][] array3 = new byte[num][];
		for (int i = 0; i < num; i++)
		{
			BigInteger bigInteger = ks[i];
			array[i] = bigInteger.SignValue < 0;
			bigInteger = bigInteger.Abs();
			int windowSize = WNafUtilities.GetWindowSize(bigInteger.BitLength, 8);
			WNafPreCompInfo wNafPreCompInfo = WNafUtilities.Precompute(ps[i], windowSize, includeNegated: true);
			int width = System.Math.Min(8, wNafPreCompInfo.Width);
			array2[i] = wNafPreCompInfo;
			array3[i] = WNafUtilities.GenerateWindowNaf(width, bigInteger);
		}
		return ImplSumOfMultiplies(array, array2, array3);
	}

	internal static ECPoint ImplSumOfMultipliesGlv(ECPoint[] ps, BigInteger[] ks, GlvEndomorphism glvEndomorphism)
	{
		BigInteger order = ps[0].Curve.Order;
		int num = ps.Length;
		BigInteger[] array = new BigInteger[num << 1];
		int i = 0;
		int num2 = 0;
		for (; i < num; i++)
		{
			BigInteger[] array2 = glvEndomorphism.DecomposeScalar(ks[i].Mod(order));
			array[num2++] = array2[0];
			array[num2++] = array2[1];
		}
		if (glvEndomorphism.HasEfficientPointMap)
		{
			return ImplSumOfMultiplies(glvEndomorphism, ps, array);
		}
		ECPoint[] array3 = new ECPoint[num << 1];
		int j = 0;
		int num3 = 0;
		for (; j < num; j++)
		{
			ECPoint eCPoint = ps[j];
			ECPoint eCPoint2 = EndoUtilities.MapPoint(glvEndomorphism, eCPoint);
			array3[num3++] = eCPoint;
			array3[num3++] = eCPoint2;
		}
		return ImplSumOfMultiplies(array3, array);
	}

	internal static ECPoint ImplSumOfMultiplies(ECEndomorphism endomorphism, ECPoint[] ps, BigInteger[] ks)
	{
		int num = ps.Length;
		int num2 = num << 1;
		bool[] array = new bool[num2];
		WNafPreCompInfo[] array2 = new WNafPreCompInfo[num2];
		byte[][] array3 = new byte[num2][];
		ECPointMap pointMap = endomorphism.PointMap;
		for (int i = 0; i < num; i++)
		{
			int num3 = i << 1;
			int num4 = num3 + 1;
			BigInteger bigInteger = ks[num3];
			array[num3] = bigInteger.SignValue < 0;
			bigInteger = bigInteger.Abs();
			BigInteger bigInteger2 = ks[num4];
			array[num4] = bigInteger2.SignValue < 0;
			bigInteger2 = bigInteger2.Abs();
			int windowSize = WNafUtilities.GetWindowSize(System.Math.Max(bigInteger.BitLength, bigInteger2.BitLength), 8);
			ECPoint p = ps[i];
			WNafPreCompInfo wNafPreCompInfo = WNafUtilities.Precompute(p, windowSize, includeNegated: true);
			ECPoint p2 = EndoUtilities.MapPoint(endomorphism, p);
			WNafPreCompInfo wNafPreCompInfo2 = WNafUtilities.PrecomputeWithPointMap(p2, pointMap, wNafPreCompInfo, includeNegated: true);
			int width = System.Math.Min(8, wNafPreCompInfo.Width);
			int width2 = System.Math.Min(8, wNafPreCompInfo2.Width);
			array2[num3] = wNafPreCompInfo;
			array2[num4] = wNafPreCompInfo2;
			array3[num3] = WNafUtilities.GenerateWindowNaf(width, bigInteger);
			array3[num4] = WNafUtilities.GenerateWindowNaf(width2, bigInteger2);
		}
		return ImplSumOfMultiplies(array, array2, array3);
	}

	private static ECPoint ImplSumOfMultiplies(bool[] negs, WNafPreCompInfo[] infos, byte[][] wnafs)
	{
		int num = 0;
		int num2 = wnafs.Length;
		for (int i = 0; i < num2; i++)
		{
			num = System.Math.Max(num, wnafs[i].Length);
		}
		ECCurve curve = infos[0].PreComp[0].Curve;
		ECPoint infinity = curve.Infinity;
		ECPoint eCPoint = infinity;
		int num3 = 0;
		for (int num4 = num - 1; num4 >= 0; num4--)
		{
			ECPoint eCPoint2 = infinity;
			for (int j = 0; j < num2; j++)
			{
				byte[] array = wnafs[j];
				int num5 = ((num4 < array.Length) ? ((sbyte)array[num4]) : 0);
				if (num5 != 0)
				{
					int num6 = System.Math.Abs(num5);
					WNafPreCompInfo wNafPreCompInfo = infos[j];
					ECPoint[] array2 = ((num5 < 0 == negs[j]) ? wNafPreCompInfo.PreComp : wNafPreCompInfo.PreCompNeg);
					eCPoint2 = eCPoint2.Add(array2[num6 >> 1]);
				}
			}
			if (eCPoint2 == infinity)
			{
				num3++;
			}
			else
			{
				if (num3 > 0)
				{
					eCPoint = eCPoint.TimesPow2(num3);
					num3 = 0;
				}
				eCPoint = eCPoint.TwicePlus(eCPoint2);
			}
		}
		if (num3 > 0)
		{
			eCPoint = eCPoint.TimesPow2(num3);
		}
		return eCPoint;
	}

	private static ECPoint ImplShamirsTrickFixedPoint(ECPoint p, BigInteger k, ECPoint q, BigInteger l)
	{
		ECCurve curve = p.Curve;
		int combSize = FixedPointUtilities.GetCombSize(curve);
		if (k.BitLength > combSize || l.BitLength > combSize)
		{
			throw new InvalidOperationException("fixed-point comb doesn't support scalars larger than the curve order");
		}
		FixedPointPreCompInfo fixedPointPreCompInfo = FixedPointUtilities.Precompute(p);
		FixedPointPreCompInfo fixedPointPreCompInfo2 = FixedPointUtilities.Precompute(q);
		ECLookupTable lookupTable = fixedPointPreCompInfo.LookupTable;
		ECLookupTable lookupTable2 = fixedPointPreCompInfo2.LookupTable;
		int width = fixedPointPreCompInfo.Width;
		int width2 = fixedPointPreCompInfo2.Width;
		if (width != width2)
		{
			FixedPointCombMultiplier fixedPointCombMultiplier = new FixedPointCombMultiplier();
			ECPoint eCPoint = fixedPointCombMultiplier.Multiply(p, k);
			ECPoint b = fixedPointCombMultiplier.Multiply(q, l);
			return eCPoint.Add(b);
		}
		int num = width;
		int num2 = (combSize + num - 1) / num;
		ECPoint eCPoint2 = curve.Infinity;
		int num3 = num2 * num;
		uint[] array = Nat.FromBigInteger(num3, k);
		uint[] array2 = Nat.FromBigInteger(num3, l);
		int num4 = num3 - 1;
		for (int i = 0; i < num2; i++)
		{
			uint num5 = 0u;
			uint num6 = 0u;
			for (int num7 = num4 - i; num7 >= 0; num7 -= num2)
			{
				uint num8 = array[num7 >> 5] >> num7;
				num5 ^= num8 >> 1;
				num5 <<= 1;
				num5 ^= num8;
				uint num9 = array2[num7 >> 5] >> num7;
				num6 ^= num9 >> 1;
				num6 <<= 1;
				num6 ^= num9;
			}
			ECPoint eCPoint3 = lookupTable.LookupVar((int)num5);
			ECPoint b2 = lookupTable2.LookupVar((int)num6);
			ECPoint b3 = eCPoint3.Add(b2);
			eCPoint2 = eCPoint2.TwicePlus(b3);
		}
		return eCPoint2.Add(fixedPointPreCompInfo.Offset).Add(fixedPointPreCompInfo2.Offset);
	}
}
