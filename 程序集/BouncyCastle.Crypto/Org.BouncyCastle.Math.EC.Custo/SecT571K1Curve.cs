using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT571K1Curve : AbstractF2mCurve
{
	private class SecT571K1LookupTable : AbstractECLookupTable
	{
		private readonly SecT571K1Curve m_outer;

		private readonly ulong[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecT571K1LookupTable(SecT571K1Curve outer, ulong[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			ulong[] array = Nat576.Create64();
			ulong[] array2 = Nat576.Create64();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				ulong num2 = (ulong)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 9; j++)
				{
					ulong[] array3;
					ulong[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					ulong[] array5 = (array3 = array2);
					int num5 = j;
					num4 = num5;
					array5[num5] = array3[num4] ^ (m_table[num + 9 + j] & num2);
				}
				num += 18;
			}
			return CreatePoint(array, array2);
		}

		public override ECPoint LookupVar(int index)
		{
			ulong[] array = Nat576.Create64();
			ulong[] array2 = Nat576.Create64();
			int num = index * 9 * 2;
			for (int i = 0; i < 9; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 9 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(ulong[] x, ulong[] y)
		{
			return m_outer.CreateRawPoint(new SecT571FieldElement(x), new SecT571FieldElement(y), SECT571K1_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECT571K1_DEFAULT_COORDS = 6;

	private const int SECT571K1_FE_LONGS = 9;

	private static readonly ECFieldElement[] SECT571K1_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecT571FieldElement(BigInteger.One)
	};

	protected readonly SecT571K1Point m_infinity;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => 571;

	public override bool IsKoblitz => true;

	public virtual int M => 571;

	public virtual bool IsTrinomial => false;

	public virtual int K1 => 2;

	public virtual int K2 => 5;

	public virtual int K3 => 10;

	public SecT571K1Curve()
		: base(571, 2, 5, 10)
	{
		m_infinity = new SecT571K1Point(this, null, null);
		m_a = FromBigInteger(BigInteger.Zero);
		m_b = FromBigInteger(BigInteger.One);
		m_order = new BigInteger(1, Hex.DecodeStrict("020000000000000000000000000000000000000000000000000000000000000000000000131850E1F19A63E4B391A8DB917F4138B630D84BE5D639381E91DEB45CFE778F637C1001"));
		m_cofactor = BigInteger.ValueOf(4L);
		m_coord = 6;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecT571K1Curve();
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
		return new SecT571FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecT571K1Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecT571K1Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		ulong[] array = new ulong[len * 9 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat576.Copy64(((SecT571FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 9;
			Nat576.Copy64(((SecT571FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 9;
		}
		return new SecT571K1LookupTable(this, array, len);
	}
}
