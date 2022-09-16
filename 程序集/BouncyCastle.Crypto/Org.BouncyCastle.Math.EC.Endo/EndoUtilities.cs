using Org.BouncyCastle.Math.EC.Multiplier;

namespace Org.BouncyCastle.Math.EC.Endo;

public abstract class EndoUtilities
{
	private class MapPointCallback : IPreCompCallback
	{
		private readonly ECEndomorphism m_endomorphism;

		private readonly ECPoint m_point;

		internal MapPointCallback(ECEndomorphism endomorphism, ECPoint point)
		{
			m_endomorphism = endomorphism;
			m_point = point;
		}

		public PreCompInfo Precompute(PreCompInfo existing)
		{
			EndoPreCompInfo endoPreCompInfo = existing as EndoPreCompInfo;
			if (CheckExisting(endoPreCompInfo, m_endomorphism))
			{
				return endoPreCompInfo;
			}
			ECPoint mappedPoint = m_endomorphism.PointMap.Map(m_point);
			EndoPreCompInfo endoPreCompInfo2 = new EndoPreCompInfo();
			endoPreCompInfo2.Endomorphism = m_endomorphism;
			endoPreCompInfo2.MappedPoint = mappedPoint;
			return endoPreCompInfo2;
		}

		private bool CheckExisting(EndoPreCompInfo existingEndo, ECEndomorphism endomorphism)
		{
			if (existingEndo != null && existingEndo.Endomorphism == endomorphism)
			{
				return existingEndo.MappedPoint != null;
			}
			return false;
		}
	}

	public static readonly string PRECOMP_NAME = "bc_endo";

	public static BigInteger[] DecomposeScalar(ScalarSplitParameters p, BigInteger k)
	{
		int bits = p.Bits;
		BigInteger bigInteger = CalculateB(k, p.G1, bits);
		BigInteger bigInteger2 = CalculateB(k, p.G2, bits);
		BigInteger bigInteger3 = k.Subtract(bigInteger.Multiply(p.V1A).Add(bigInteger2.Multiply(p.V2A)));
		BigInteger bigInteger4 = bigInteger.Multiply(p.V1B).Add(bigInteger2.Multiply(p.V2B)).Negate();
		return new BigInteger[2] { bigInteger3, bigInteger4 };
	}

	public static ECPoint MapPoint(ECEndomorphism endomorphism, ECPoint p)
	{
		EndoPreCompInfo endoPreCompInfo = (EndoPreCompInfo)p.Curve.Precompute(p, PRECOMP_NAME, new MapPointCallback(endomorphism, p));
		return endoPreCompInfo.MappedPoint;
	}

	private static BigInteger CalculateB(BigInteger k, BigInteger g, int t)
	{
		bool flag = g.SignValue < 0;
		BigInteger bigInteger = k.Multiply(g.Abs());
		bool flag2 = bigInteger.TestBit(t - 1);
		bigInteger = bigInteger.ShiftRight(t);
		if (flag2)
		{
			bigInteger = bigInteger.Add(BigInteger.One);
		}
		if (!flag)
		{
			return bigInteger;
		}
		return bigInteger.Negate();
	}
}
