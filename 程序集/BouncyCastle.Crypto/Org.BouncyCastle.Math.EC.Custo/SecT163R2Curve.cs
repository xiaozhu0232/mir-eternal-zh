using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT163R2Curve : AbstractF2mCurve
{
	private class SecT163R2LookupTable : AbstractECLookupTable
	{
		private readonly SecT163R2Curve m_outer;

		private readonly ulong[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecT163R2LookupTable(SecT163R2Curve outer, ulong[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			ulong[] array = Nat192.Create64();
			ulong[] array2 = Nat192.Create64();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				ulong num2 = (ulong)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 3; j++)
				{
					ulong[] array3;
					ulong[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					ulong[] array5 = (array3 = array2);
					int num5 = j;
					num4 = num5;
					array5[num5] = array3[num4] ^ (m_table[num + 3 + j] & num2);
				}
				num += 6;
			}
			return CreatePoint(array, array2);
		}

		public override ECPoint LookupVar(int index)
		{
			ulong[] array = Nat192.Create64();
			ulong[] array2 = Nat192.Create64();
			int num = index * 3 * 2;
			for (int i = 0; i < 3; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 3 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(ulong[] x, ulong[] y)
		{
			return m_outer.CreateRawPoint(new SecT163FieldElement(x), new SecT163FieldElement(y), SECT163R2_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECT163R2_DEFAULT_COORDS = 6;

	private const int SECT163R2_FE_LONGS = 3;

	private static readonly ECFieldElement[] SECT163R2_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecT163FieldElement(BigInteger.One)
	};

	protected readonly SecT163R2Point m_infinity;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => 163;

	public override bool IsKoblitz => false;

	public virtual int M => 163;

	public virtual bool IsTrinomial => false;

	public virtual int K1 => 3;

	public virtual int K2 => 6;

	public virtual int K3 => 7;

	public SecT163R2Curve()
		: base(163, 3, 6, 7)
	{
		m_infinity = new SecT163R2Point(this, null, null);
		m_a = FromBigInteger(BigInteger.One);
		m_b = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("020A601907B8C953CA1481EB10512F78744A3205FD")));
		m_order = new BigInteger(1, Hex.DecodeStrict("040000000000000000000292FE77E70C12A4234C33"));
		m_cofactor = BigInteger.Two;
		m_coord = 6;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecT163R2Curve();
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
		return new SecT163FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecT163R2Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecT163R2Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		ulong[] array = new ulong[len * 3 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat192.Copy64(((SecT163FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 3;
			Nat192.Copy64(((SecT163FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 3;
		}
		return new SecT163R2LookupTable(this, array, len);
	}
}
