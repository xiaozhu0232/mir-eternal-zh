using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Multiplier;

public abstract class WNafUtilities
{
	private class ConfigureBasepointCallback : IPreCompCallback
	{
		private readonly ECCurve m_curve;

		private readonly int m_confWidth;

		internal ConfigureBasepointCallback(ECCurve curve, int confWidth)
		{
			m_curve = curve;
			m_confWidth = confWidth;
		}

		public PreCompInfo Precompute(PreCompInfo existing)
		{
			WNafPreCompInfo wNafPreCompInfo = existing as WNafPreCompInfo;
			if (wNafPreCompInfo != null && wNafPreCompInfo.ConfWidth == m_confWidth)
			{
				wNafPreCompInfo.PromotionCountdown = 0;
				return wNafPreCompInfo;
			}
			WNafPreCompInfo wNafPreCompInfo2 = new WNafPreCompInfo();
			wNafPreCompInfo2.PromotionCountdown = 0;
			wNafPreCompInfo2.ConfWidth = m_confWidth;
			if (wNafPreCompInfo != null)
			{
				wNafPreCompInfo2.PreComp = wNafPreCompInfo.PreComp;
				wNafPreCompInfo2.PreCompNeg = wNafPreCompInfo.PreCompNeg;
				wNafPreCompInfo2.Twice = wNafPreCompInfo.Twice;
				wNafPreCompInfo2.Width = wNafPreCompInfo.Width;
			}
			return wNafPreCompInfo2;
		}
	}

	private class MapPointCallback : IPreCompCallback
	{
		private readonly WNafPreCompInfo m_infoP;

		private readonly bool m_includeNegated;

		private readonly ECPointMap m_pointMap;

		internal MapPointCallback(WNafPreCompInfo infoP, bool includeNegated, ECPointMap pointMap)
		{
			m_infoP = infoP;
			m_includeNegated = includeNegated;
			m_pointMap = pointMap;
		}

		public PreCompInfo Precompute(PreCompInfo existing)
		{
			WNafPreCompInfo wNafPreCompInfo = new WNafPreCompInfo();
			wNafPreCompInfo.ConfWidth = m_infoP.ConfWidth;
			ECPoint twice = m_infoP.Twice;
			if (twice != null)
			{
				ECPoint eCPoint2 = (wNafPreCompInfo.Twice = m_pointMap.Map(twice));
			}
			ECPoint[] preComp = m_infoP.PreComp;
			ECPoint[] array = new ECPoint[preComp.Length];
			for (int i = 0; i < preComp.Length; i++)
			{
				array[i] = m_pointMap.Map(preComp[i]);
			}
			wNafPreCompInfo.PreComp = array;
			wNafPreCompInfo.Width = m_infoP.Width;
			if (m_includeNegated)
			{
				ECPoint[] array2 = new ECPoint[array.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = array[j].Negate();
				}
				wNafPreCompInfo.PreCompNeg = array2;
			}
			return wNafPreCompInfo;
		}
	}

	private class PrecomputeCallback : IPreCompCallback
	{
		private readonly ECPoint m_p;

		private readonly int m_minWidth;

		private readonly bool m_includeNegated;

		internal PrecomputeCallback(ECPoint p, int minWidth, bool includeNegated)
		{
			m_p = p;
			m_minWidth = minWidth;
			m_includeNegated = includeNegated;
		}

		public PreCompInfo Precompute(PreCompInfo existing)
		{
			WNafPreCompInfo wNafPreCompInfo = existing as WNafPreCompInfo;
			int num = System.Math.Max(2, System.Math.Min(MAX_WIDTH, m_minWidth));
			int reqPreCompLen = 1 << num - 2;
			if (CheckExisting(wNafPreCompInfo, num, reqPreCompLen, m_includeNegated))
			{
				wNafPreCompInfo.DecrementPromotionCountdown();
				return wNafPreCompInfo;
			}
			WNafPreCompInfo wNafPreCompInfo2 = new WNafPreCompInfo();
			ECCurve curve = m_p.Curve;
			ECPoint[] array = null;
			ECPoint[] array2 = null;
			ECPoint eCPoint = null;
			if (wNafPreCompInfo != null)
			{
				int num3 = (wNafPreCompInfo2.PromotionCountdown = wNafPreCompInfo.DecrementPromotionCountdown());
				int num4 = (wNafPreCompInfo2.ConfWidth = wNafPreCompInfo.ConfWidth);
				array = wNafPreCompInfo.PreComp;
				array2 = wNafPreCompInfo.PreCompNeg;
				eCPoint = wNafPreCompInfo.Twice;
			}
			num = System.Math.Min(MAX_WIDTH, System.Math.Max(wNafPreCompInfo2.ConfWidth, num));
			reqPreCompLen = 1 << num - 2;
			int num5 = 0;
			if (array == null)
			{
				array = EMPTY_POINTS;
			}
			else
			{
				num5 = array.Length;
			}
			if (num5 < reqPreCompLen)
			{
				array = ResizeTable(array, reqPreCompLen);
				if (reqPreCompLen == 1)
				{
					array[0] = m_p.Normalize();
				}
				else
				{
					int num6 = num5;
					if (num6 == 0)
					{
						array[0] = m_p;
						num6 = 1;
					}
					ECFieldElement eCFieldElement = null;
					if (reqPreCompLen == 2)
					{
						array[1] = m_p.ThreeTimes();
					}
					else
					{
						ECPoint eCPoint2 = eCPoint;
						ECPoint eCPoint3 = array[num6 - 1];
						if (eCPoint2 == null)
						{
							eCPoint2 = array[0].Twice();
							eCPoint = eCPoint2;
							if (!eCPoint.IsInfinity && ECAlgorithms.IsFpCurve(curve) && curve.FieldSize >= 64)
							{
								switch (curve.CoordinateSystem)
								{
								case 2:
								case 3:
								case 4:
								{
									eCFieldElement = eCPoint.GetZCoord(0);
									eCPoint2 = curve.CreatePoint(eCPoint.XCoord.ToBigInteger(), eCPoint.YCoord.ToBigInteger());
									ECFieldElement eCFieldElement2 = eCFieldElement.Square();
									ECFieldElement scale = eCFieldElement2.Multiply(eCFieldElement);
									eCPoint3 = eCPoint3.ScaleX(eCFieldElement2).ScaleY(scale);
									if (num5 == 0)
									{
										array[0] = eCPoint3;
									}
									break;
								}
								}
							}
						}
						while (num6 < reqPreCompLen)
						{
							eCPoint3 = (array[num6++] = eCPoint3.Add(eCPoint2));
						}
					}
					curve.NormalizeAll(array, num5, reqPreCompLen - num5, eCFieldElement);
				}
			}
			if (m_includeNegated)
			{
				int i;
				if (array2 == null)
				{
					i = 0;
					array2 = new ECPoint[reqPreCompLen];
				}
				else
				{
					i = array2.Length;
					if (i < reqPreCompLen)
					{
						array2 = ResizeTable(array2, reqPreCompLen);
					}
				}
				for (; i < reqPreCompLen; i++)
				{
					array2[i] = array[i].Negate();
				}
			}
			wNafPreCompInfo2.PreComp = array;
			wNafPreCompInfo2.PreCompNeg = array2;
			wNafPreCompInfo2.Twice = eCPoint;
			wNafPreCompInfo2.Width = num;
			return wNafPreCompInfo2;
		}

		private bool CheckExisting(WNafPreCompInfo existingWNaf, int width, int reqPreCompLen, bool includeNegated)
		{
			if (existingWNaf != null && existingWNaf.Width >= System.Math.Max(existingWNaf.ConfWidth, width) && CheckTable(existingWNaf.PreComp, reqPreCompLen))
			{
				if (includeNegated)
				{
					return CheckTable(existingWNaf.PreCompNeg, reqPreCompLen);
				}
				return true;
			}
			return false;
		}

		private bool CheckTable(ECPoint[] table, int reqLen)
		{
			if (table != null)
			{
				return table.Length >= reqLen;
			}
			return false;
		}
	}

	private class PrecomputeWithPointMapCallback : IPreCompCallback
	{
		private readonly ECPoint m_point;

		private readonly ECPointMap m_pointMap;

		private readonly WNafPreCompInfo m_fromWNaf;

		private readonly bool m_includeNegated;

		internal PrecomputeWithPointMapCallback(ECPoint point, ECPointMap pointMap, WNafPreCompInfo fromWNaf, bool includeNegated)
		{
			m_point = point;
			m_pointMap = pointMap;
			m_fromWNaf = fromWNaf;
			m_includeNegated = includeNegated;
		}

		public PreCompInfo Precompute(PreCompInfo existing)
		{
			WNafPreCompInfo wNafPreCompInfo = existing as WNafPreCompInfo;
			int width = m_fromWNaf.Width;
			int reqPreCompLen = m_fromWNaf.PreComp.Length;
			if (CheckExisting(wNafPreCompInfo, width, reqPreCompLen, m_includeNegated))
			{
				wNafPreCompInfo.DecrementPromotionCountdown();
				return wNafPreCompInfo;
			}
			WNafPreCompInfo wNafPreCompInfo2 = new WNafPreCompInfo();
			wNafPreCompInfo2.PromotionCountdown = m_fromWNaf.PromotionCountdown;
			ECPoint twice = m_fromWNaf.Twice;
			if (twice != null)
			{
				ECPoint eCPoint2 = (wNafPreCompInfo2.Twice = m_pointMap.Map(twice));
			}
			ECPoint[] preComp = m_fromWNaf.PreComp;
			ECPoint[] array = new ECPoint[preComp.Length];
			for (int i = 0; i < preComp.Length; i++)
			{
				array[i] = m_pointMap.Map(preComp[i]);
			}
			wNafPreCompInfo2.PreComp = array;
			wNafPreCompInfo2.Width = width;
			if (m_includeNegated)
			{
				ECPoint[] array2 = new ECPoint[array.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = array[j].Negate();
				}
				wNafPreCompInfo2.PreCompNeg = array2;
			}
			return wNafPreCompInfo2;
		}

		private bool CheckExisting(WNafPreCompInfo existingWNaf, int width, int reqPreCompLen, bool includeNegated)
		{
			if (existingWNaf != null && existingWNaf.Width >= width && CheckTable(existingWNaf.PreComp, reqPreCompLen))
			{
				if (includeNegated)
				{
					return CheckTable(existingWNaf.PreCompNeg, reqPreCompLen);
				}
				return true;
			}
			return false;
		}

		private bool CheckTable(ECPoint[] table, int reqLen)
		{
			if (table != null)
			{
				return table.Length >= reqLen;
			}
			return false;
		}
	}

	public static readonly string PRECOMP_NAME = "bc_wnaf";

	private static readonly int[] DEFAULT_WINDOW_SIZE_CUTOFFS = new int[6] { 13, 41, 121, 337, 897, 2305 };

	private static readonly int MAX_WIDTH = 16;

	private static readonly ECPoint[] EMPTY_POINTS = new ECPoint[0];

	public static void ConfigureBasepoint(ECPoint p)
	{
		ECCurve curve = p.Curve;
		if (curve != null)
		{
			int bits = curve.Order?.BitLength ?? (curve.FieldSize + 1);
			int confWidth = System.Math.Min(MAX_WIDTH, GetWindowSize(bits) + 3);
			curve.Precompute(p, PRECOMP_NAME, new ConfigureBasepointCallback(curve, confWidth));
		}
	}

	public static int[] GenerateCompactNaf(BigInteger k)
	{
		if (k.BitLength >> 16 != 0)
		{
			throw new ArgumentException("must have bitlength < 2^16", "k");
		}
		if (k.SignValue == 0)
		{
			return Arrays.EmptyInts;
		}
		BigInteger bigInteger = k.ShiftLeft(1).Add(k);
		int bitLength = bigInteger.BitLength;
		int[] array = new int[bitLength >> 1];
		BigInteger bigInteger2 = bigInteger.Xor(k);
		int num = bitLength - 1;
		int num2 = 0;
		int num3 = 0;
		for (int i = 1; i < num; i++)
		{
			if (!bigInteger2.TestBit(i))
			{
				num3++;
				continue;
			}
			int num4 = ((!k.TestBit(i)) ? 1 : (-1));
			array[num2++] = (num4 << 16) | num3;
			num3 = 1;
			i++;
		}
		array[num2++] = 0x10000 | num3;
		if (array.Length > num2)
		{
			array = Trim(array, num2);
		}
		return array;
	}

	public static int[] GenerateCompactWindowNaf(int width, BigInteger k)
	{
		switch (width)
		{
		case 2:
			return GenerateCompactNaf(k);
		default:
			throw new ArgumentException("must be in the range [2, 16]", "width");
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		{
			if (k.BitLength >> 16 != 0)
			{
				throw new ArgumentException("must have bitlength < 2^16", "k");
			}
			if (k.SignValue == 0)
			{
				return Arrays.EmptyInts;
			}
			int[] array = new int[k.BitLength / width + 1];
			int num = 1 << width;
			int num2 = num - 1;
			int num3 = num >> 1;
			bool flag = false;
			int num4 = 0;
			int num5 = 0;
			while (num5 <= k.BitLength)
			{
				if (k.TestBit(num5) == flag)
				{
					num5++;
					continue;
				}
				k = k.ShiftRight(num5);
				int num6 = k.IntValue & num2;
				if (flag)
				{
					num6++;
				}
				flag = (num6 & num3) != 0;
				if (flag)
				{
					num6 -= num;
				}
				int num7 = ((num4 > 0) ? (num5 - 1) : num5);
				array[num4++] = (num6 << 16) | num7;
				num5 = width;
			}
			if (array.Length > num4)
			{
				array = Trim(array, num4);
			}
			return array;
		}
		}
	}

	public static byte[] GenerateJsf(BigInteger g, BigInteger h)
	{
		int num = System.Math.Max(g.BitLength, h.BitLength) + 1;
		byte[] array = new byte[num];
		BigInteger bigInteger = g;
		BigInteger bigInteger2 = h;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		while ((num3 | num4) != 0 || bigInteger.BitLength > num5 || bigInteger2.BitLength > num5)
		{
			int num6 = ((int)((uint)bigInteger.IntValue >> num5) + num3) & 7;
			int num7 = ((int)((uint)bigInteger2.IntValue >> num5) + num4) & 7;
			int num8 = num6 & 1;
			if (num8 != 0)
			{
				num8 -= num6 & 2;
				if (num6 + num8 == 4 && (num7 & 3) == 2)
				{
					num8 = -num8;
				}
			}
			int num9 = num7 & 1;
			if (num9 != 0)
			{
				num9 -= num7 & 2;
				if (num7 + num9 == 4 && (num6 & 3) == 2)
				{
					num9 = -num9;
				}
			}
			if (num3 << 1 == 1 + num8)
			{
				num3 ^= 1;
			}
			if (num4 << 1 == 1 + num9)
			{
				num4 ^= 1;
			}
			if (++num5 == 30)
			{
				num5 = 0;
				bigInteger = bigInteger.ShiftRight(30);
				bigInteger2 = bigInteger2.ShiftRight(30);
			}
			array[num2++] = (byte)((uint)(num8 << 4) | ((uint)num9 & 0xFu));
		}
		if (array.Length > num2)
		{
			array = Trim(array, num2);
		}
		return array;
	}

	public static byte[] GenerateNaf(BigInteger k)
	{
		if (k.SignValue == 0)
		{
			return Arrays.EmptyBytes;
		}
		BigInteger bigInteger = k.ShiftLeft(1).Add(k);
		int num = bigInteger.BitLength - 1;
		byte[] array = new byte[num];
		BigInteger bigInteger2 = bigInteger.Xor(k);
		for (int i = 1; i < num; i++)
		{
			if (bigInteger2.TestBit(i))
			{
				array[i - 1] = (byte)((!k.TestBit(i)) ? 1u : uint.MaxValue);
				i++;
			}
		}
		array[num - 1] = 1;
		return array;
	}

	public static byte[] GenerateWindowNaf(int width, BigInteger k)
	{
		switch (width)
		{
		case 2:
			return GenerateNaf(k);
		default:
			throw new ArgumentException("must be in the range [2, 8]", "width");
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		{
			if (k.SignValue == 0)
			{
				return Arrays.EmptyBytes;
			}
			byte[] array = new byte[k.BitLength + 1];
			int num = 1 << width;
			int num2 = num - 1;
			int num3 = num >> 1;
			bool flag = false;
			int num4 = 0;
			int num5 = 0;
			while (num5 <= k.BitLength)
			{
				if (k.TestBit(num5) == flag)
				{
					num5++;
					continue;
				}
				k = k.ShiftRight(num5);
				int num6 = k.IntValue & num2;
				if (flag)
				{
					num6++;
				}
				flag = (num6 & num3) != 0;
				if (flag)
				{
					num6 -= num;
				}
				num4 += ((num4 > 0) ? (num5 - 1) : num5);
				array[num4++] = (byte)num6;
				num5 = width;
			}
			if (array.Length > num4)
			{
				array = Trim(array, num4);
			}
			return array;
		}
		}
	}

	public static int GetNafWeight(BigInteger k)
	{
		if (k.SignValue == 0)
		{
			return 0;
		}
		BigInteger bigInteger = k.ShiftLeft(1).Add(k);
		BigInteger bigInteger2 = bigInteger.Xor(k);
		return bigInteger2.BitCount;
	}

	public static WNafPreCompInfo GetWNafPreCompInfo(ECPoint p)
	{
		return GetWNafPreCompInfo(p.Curve.GetPreCompInfo(p, PRECOMP_NAME));
	}

	public static WNafPreCompInfo GetWNafPreCompInfo(PreCompInfo preCompInfo)
	{
		return preCompInfo as WNafPreCompInfo;
	}

	public static int GetWindowSize(int bits)
	{
		return GetWindowSize(bits, DEFAULT_WINDOW_SIZE_CUTOFFS, MAX_WIDTH);
	}

	public static int GetWindowSize(int bits, int maxWidth)
	{
		return GetWindowSize(bits, DEFAULT_WINDOW_SIZE_CUTOFFS, maxWidth);
	}

	public static int GetWindowSize(int bits, int[] windowSizeCutoffs)
	{
		return GetWindowSize(bits, windowSizeCutoffs, MAX_WIDTH);
	}

	public static int GetWindowSize(int bits, int[] windowSizeCutoffs, int maxWidth)
	{
		int i;
		for (i = 0; i < windowSizeCutoffs.Length && bits >= windowSizeCutoffs[i]; i++)
		{
		}
		return System.Math.Max(2, System.Math.Min(maxWidth, i + 2));
	}

	[Obsolete]
	public static ECPoint MapPointWithPrecomp(ECPoint p, int minWidth, bool includeNegated, ECPointMap pointMap)
	{
		ECCurve curve = p.Curve;
		WNafPreCompInfo infoP = Precompute(p, minWidth, includeNegated);
		ECPoint eCPoint = pointMap.Map(p);
		curve.Precompute(eCPoint, PRECOMP_NAME, new MapPointCallback(infoP, includeNegated, pointMap));
		return eCPoint;
	}

	public static WNafPreCompInfo Precompute(ECPoint p, int minWidth, bool includeNegated)
	{
		return (WNafPreCompInfo)p.Curve.Precompute(p, PRECOMP_NAME, new PrecomputeCallback(p, minWidth, includeNegated));
	}

	public static WNafPreCompInfo PrecomputeWithPointMap(ECPoint p, ECPointMap pointMap, WNafPreCompInfo fromWNaf, bool includeNegated)
	{
		return (WNafPreCompInfo)p.Curve.Precompute(p, PRECOMP_NAME, new PrecomputeWithPointMapCallback(p, pointMap, fromWNaf, includeNegated));
	}

	private static byte[] Trim(byte[] a, int length)
	{
		byte[] array = new byte[length];
		Array.Copy(a, 0, array, 0, array.Length);
		return array;
	}

	private static int[] Trim(int[] a, int length)
	{
		int[] array = new int[length];
		Array.Copy(a, 0, array, 0, array.Length);
		return array;
	}

	private static ECPoint[] ResizeTable(ECPoint[] a, int length)
	{
		ECPoint[] array = new ECPoint[length];
		Array.Copy(a, 0, array, 0, a.Length);
		return array;
	}
}
