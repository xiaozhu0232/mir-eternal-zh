namespace Org.BouncyCastle.Math.EC.Multiplier;

public class FixedPointUtilities
{
	private class FixedPointCallback : IPreCompCallback
	{
		private readonly ECPoint m_p;

		internal FixedPointCallback(ECPoint p)
		{
			m_p = p;
		}

		public PreCompInfo Precompute(PreCompInfo existing)
		{
			FixedPointPreCompInfo fixedPointPreCompInfo = ((existing is FixedPointPreCompInfo) ? ((FixedPointPreCompInfo)existing) : null);
			ECCurve curve = m_p.Curve;
			int combSize = GetCombSize(curve);
			int num = ((combSize > 250) ? 6 : 5);
			int num2 = 1 << num;
			if (CheckExisting(fixedPointPreCompInfo, num2))
			{
				return fixedPointPreCompInfo;
			}
			int e = (combSize + num - 1) / num;
			ECPoint[] array = new ECPoint[num + 1];
			array[0] = m_p;
			for (int i = 1; i < num; i++)
			{
				array[i] = array[i - 1].TimesPow2(e);
			}
			array[num] = array[0].Subtract(array[1]);
			curve.NormalizeAll(array);
			ECPoint[] array2 = new ECPoint[num2];
			array2[0] = array[0];
			for (int num3 = num - 1; num3 >= 0; num3--)
			{
				ECPoint b = array[num3];
				int num4 = 1 << num3;
				for (int j = num4; j < num2; j += num4 << 1)
				{
					array2[j] = array2[j - num4].Add(b);
				}
			}
			curve.NormalizeAll(array2);
			FixedPointPreCompInfo fixedPointPreCompInfo2 = new FixedPointPreCompInfo();
			fixedPointPreCompInfo2.LookupTable = curve.CreateCacheSafeLookupTable(array2, 0, array2.Length);
			fixedPointPreCompInfo2.Offset = array[num];
			fixedPointPreCompInfo2.Width = num;
			return fixedPointPreCompInfo2;
		}

		private bool CheckExisting(FixedPointPreCompInfo existingFP, int n)
		{
			if (existingFP != null)
			{
				return CheckTable(existingFP.LookupTable, n);
			}
			return false;
		}

		private bool CheckTable(ECLookupTable table, int n)
		{
			if (table != null)
			{
				return table.Size >= n;
			}
			return false;
		}
	}

	public static readonly string PRECOMP_NAME = "bc_fixed_point";

	public static int GetCombSize(ECCurve c)
	{
		return c.Order?.BitLength ?? (c.FieldSize + 1);
	}

	public static FixedPointPreCompInfo GetFixedPointPreCompInfo(PreCompInfo preCompInfo)
	{
		return preCompInfo as FixedPointPreCompInfo;
	}

	public static FixedPointPreCompInfo Precompute(ECPoint p)
	{
		return (FixedPointPreCompInfo)p.Curve.Precompute(p, PRECOMP_NAME, new FixedPointCallback(p));
	}
}
