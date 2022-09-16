using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT113R2Curve : AbstractF2mCurve
{
	private class SecT113R2LookupTable : AbstractECLookupTable
	{
		private readonly SecT113R2Curve m_outer;

		private readonly ulong[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecT113R2LookupTable(SecT113R2Curve outer, ulong[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			ulong[] array = Nat128.Create64();
			ulong[] array2 = Nat128.Create64();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				ulong num2 = (ulong)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 2; j++)
				{
					ulong[] array3;
					ulong[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					ulong[] array5 = (array3 = array2);
					int num5 = j;
					num4 = num5;
					array5[num5] = array3[num4] ^ (m_table[num + 2 + j] & num2);
				}
				num += 4;
			}
			return CreatePoint(array, array2);
		}

		public override ECPoint LookupVar(int index)
		{
			ulong[] array = Nat128.Create64();
			ulong[] array2 = Nat128.Create64();
			int num = index * 2 * 2;
			for (int i = 0; i < 2; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 2 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(ulong[] x, ulong[] y)
		{
			return m_outer.CreateRawPoint(new SecT113FieldElement(x), new SecT113FieldElement(y), SECT113R2_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECT113R2_DEFAULT_COORDS = 6;

	private const int SECT113R2_FE_LONGS = 2;

	private static readonly ECFieldElement[] SECT113R2_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecT113FieldElement(BigInteger.One)
	};

	protected readonly SecT113R2Point m_infinity;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => 113;

	public override bool IsKoblitz => false;

	public virtual int M => 113;

	public virtual bool IsTrinomial => true;

	public virtual int K1 => 9;

	public virtual int K2 => 0;

	public virtual int K3 => 0;

	public SecT113R2Curve()
		: base(113, 9, 0, 0)
	{
		m_infinity = new SecT113R2Point(this, null, null);
		m_a = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("00689918DBEC7E5A0DD6DFC0AA55C7")));
		m_b = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("0095E9A9EC9B297BD4BF36E059184F")));
		m_order = new BigInteger(1, Hex.DecodeStrict("010000000000000108789B2496AF93"));
		m_cofactor = BigInteger.Two;
		m_coord = 6;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecT113R2Curve();
	}

	public override bool SupportsCoordinateSystem(int coord)
	{
		if (coord == 6)
		{
			return true;
		}
		return false;
	}

	public override ECFieldElement FromBigInteger(BigInteger x)
	{
		return new SecT113FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecT113R2Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecT113R2Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		ulong[] array = new ulong[len * 2 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat128.Copy64(((SecT113FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 2;
			Nat128.Copy64(((SecT113FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 2;
		}
		return new SecT113R2LookupTable(this, array, len);
	}
}
