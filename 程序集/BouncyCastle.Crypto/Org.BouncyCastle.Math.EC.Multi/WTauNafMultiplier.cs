using System;
using Org.BouncyCastle.Math.EC.Abc;

namespace Org.BouncyCastle.Math.EC.Multiplier;

public class WTauNafMultiplier : AbstractECMultiplier
{
	private class WTauNafCallback : IPreCompCallback
	{
		private readonly AbstractF2mPoint m_p;

		private readonly sbyte m_a;

		internal WTauNafCallback(AbstractF2mPoint p, sbyte a)
		{
			m_p = p;
			m_a = a;
		}

		public PreCompInfo Precompute(PreCompInfo existing)
		{
			if (existing is WTauNafPreCompInfo)
			{
				return existing;
			}
			WTauNafPreCompInfo wTauNafPreCompInfo = new WTauNafPreCompInfo();
			wTauNafPreCompInfo.PreComp = Tnaf.GetPreComp(m_p, m_a);
			return wTauNafPreCompInfo;
		}
	}

	internal static readonly string PRECOMP_NAME = "bc_wtnaf";

	protected override ECPoint MultiplyPositive(ECPoint point, BigInteger k)
	{
		if (!(point is AbstractF2mPoint))
		{
			throw new ArgumentException("Only AbstractF2mPoint can be used in WTauNafMultiplier");
		}
		AbstractF2mPoint abstractF2mPoint = (AbstractF2mPoint)point;
		AbstractF2mCurve abstractF2mCurve = (AbstractF2mCurve)abstractF2mPoint.Curve;
		int fieldSize = abstractF2mCurve.FieldSize;
		sbyte b = (sbyte)abstractF2mCurve.A.ToBigInteger().IntValue;
		sbyte mu = Tnaf.GetMu(b);
		BigInteger[] si = abstractF2mCurve.GetSi();
		ZTauElement lambda = Tnaf.PartModReduction(k, fieldSize, b, si, mu, 10);
		return MultiplyWTnaf(abstractF2mPoint, lambda, b, mu);
	}

	private AbstractF2mPoint MultiplyWTnaf(AbstractF2mPoint p, ZTauElement lambda, sbyte a, sbyte mu)
	{
		ZTauElement[] alpha = ((a == 0) ? Tnaf.Alpha0 : Tnaf.Alpha1);
		BigInteger tw = Tnaf.GetTw(mu, 4);
		sbyte[] u = Tnaf.TauAdicWNaf(mu, lambda, 4, BigInteger.ValueOf(16L), tw, alpha);
		return MultiplyFromWTnaf(p, u);
	}

	private static AbstractF2mPoint MultiplyFromWTnaf(AbstractF2mPoint p, sbyte[] u)
	{
		AbstractF2mCurve abstractF2mCurve = (AbstractF2mCurve)p.Curve;
		sbyte a = (sbyte)abstractF2mCurve.A.ToBigInteger().IntValue;
		WTauNafCallback callback = new WTauNafCallback(p, a);
		WTauNafPreCompInfo wTauNafPreCompInfo = (WTauNafPreCompInfo)abstractF2mCurve.Precompute(p, PRECOMP_NAME, callback);
		AbstractF2mPoint[] preComp = wTauNafPreCompInfo.PreComp;
		AbstractF2mPoint[] array = new AbstractF2mPoint[preComp.Length];
		for (int i = 0; i < preComp.Length; i++)
		{
			array[i] = (AbstractF2mPoint)preComp[i].Negate();
		}
		AbstractF2mPoint abstractF2mPoint = (AbstractF2mPoint)p.Curve.Infinity;
		int num = 0;
		for (int num2 = u.Length - 1; num2 >= 0; num2--)
		{
			num++;
			int num3 = u[num2];
			if (num3 != 0)
			{
				abstractF2mPoint = abstractF2mPoint.TauPow(num);
				num = 0;
				ECPoint b = ((num3 > 0) ? preComp[num3 >> 1] : array[-num3 >> 1]);
				abstractF2mPoint = (AbstractF2mPoint)abstractF2mPoint.Add(b);
			}
		}
		if (num > 0)
		{
			abstractF2mPoint = abstractF2mPoint.TauPow(num);
		}
		return abstractF2mPoint;
	}
}
