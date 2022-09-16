using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT409K1Curve : AbstractF2mCurve
{
	private class SecT409K1LookupTable : AbstractECLookupTable
	{
		private readonly SecT409K1Curve m_outer;

		private readonly ulong[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecT409K1LookupTable(SecT409K1Curve outer, ulong[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			ulong[] array = Nat448.Create64();
			ulong[] array2 = Nat448.Create64();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				ulong num2 = (ulong)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 7; j++)
				{
					ulong[] array3;
					ulong[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					ulong[] array5 = (array3 = array2);
					int num5 = j;
					num4 = num5;
					array5[num5] = array3[num4] ^ (m_table[num + 7 + j] & num2);
				}
				num += 14;
			}
			return CreatePoint(array, array2);
		}

		public override ECPoint LookupVar(int index)
		{
			ulong[] array = Nat448.Create64();
			ulong[] array2 = Nat448.Create64();
			int num = index * 7 * 2;
			for (int i = 0; i < 7; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 7 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(ulong[] x, ulong[] y)
		{
			return m_outer.CreateRawPoint(new SecT409FieldElement(x), new SecT409FieldElement(y), SECT409K1_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECT409K1_DEFAULT_COORDS = 6;

	private const int SECT409K1_FE_LONGS = 7;

	private static readonly ECFieldElement[] SECT409K1_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecT409FieldElement(BigInteger.One)
	};

	protected readonly SecT409K1Point m_infinity;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => 409;

	public override bool IsKoblitz => true;

	public virtual int M => 409;

	public virtual bool IsTrinomial => true;

	public virtual int K1 => 87;

	public virtual int K2 => 0;

	public virtual int K3 => 0;

	public SecT409K1Curve()
		: base(409, 87, 0, 0)
	{
		m_infinity = new SecT409K1Point(this, null, null);
		m_a = FromBigInteger(BigInteger.Zero);
		m_b = FromBigInteger(BigInteger.One);
		m_order = new BigInteger(1, Hex.DecodeStrict("7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFE5F83B2D4EA20400EC4557D5ED3E3E7CA5B4B5C83B8E01E5FCF"));
		m_cofactor = BigInteger.ValueOf(4L);
		m_coord = 6;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecT409K1Curve();
	}

	public override bool SupportsCoordinateSystem(int coord)
	{
		if (coord == 6)
		{
			return true;
		}
		return false;
	}

	protected override ECMultiplier CreateDefaultMultiplier()
	{
		return new WTauNafMultiplier();
	}

	public override ECFieldElement FromBigInteger(BigInteger x)
	{
		return new SecT409FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecT409K1Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecT409K1Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		ulong[] array = new ulong[len * 7 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat448.Copy64(((SecT409FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 7;
			Nat448.Copy64(((SecT409FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 7;
		}
		return new SecT409K1LookupTable(this, array, len);
	}
}
