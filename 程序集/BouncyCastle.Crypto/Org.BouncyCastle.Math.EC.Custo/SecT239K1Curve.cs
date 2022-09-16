using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT239K1Curve : AbstractF2mCurve
{
	private class SecT239K1LookupTable : AbstractECLookupTable
	{
		private readonly SecT239K1Curve m_outer;

		private readonly ulong[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecT239K1LookupTable(SecT239K1Curve outer, ulong[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			ulong[] array = Nat256.Create64();
			ulong[] array2 = Nat256.Create64();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				ulong num2 = (ulong)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 4; j++)
				{
					ulong[] array3;
					ulong[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					ulong[] array5 = (array3 = array2);
					int num5 = j;
					num4 = num5;
					array5[num5] = array3[num4] ^ (m_table[num + 4 + j] & num2);
				}
				num += 8;
			}
			return CreatePoint(array, array2);
		}

		public override ECPoint LookupVar(int index)
		{
			ulong[] array = Nat256.Create64();
			ulong[] array2 = Nat256.Create64();
			int num = index * 4 * 2;
			for (int i = 0; i < 4; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 4 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(ulong[] x, ulong[] y)
		{
			return m_outer.CreateRawPoint(new SecT239FieldElement(x), new SecT239FieldElement(y), SECT239K1_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECT239K1_DEFAULT_COORDS = 6;

	private const int SECT239K1_FE_LONGS = 4;

	private static readonly ECFieldElement[] SECT239K1_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecT239FieldElement(BigInteger.One)
	};

	protected readonly SecT239K1Point m_infinity;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => 239;

	public override bool IsKoblitz => true;

	public virtual int M => 239;

	public virtual bool IsTrinomial => true;

	public virtual int K1 => 158;

	public virtual int K2 => 0;

	public virtual int K3 => 0;

	public SecT239K1Curve()
		: base(239, 158, 0, 0)
	{
		m_infinity = new SecT239K1Point(this, null, null);
		m_a = FromBigInteger(BigInteger.Zero);
		m_b = FromBigInteger(BigInteger.One);
		m_order = new BigInteger(1, Hex.DecodeStrict("2000000000000000000000000000005A79FEC67CB6E91F1C1DA800E478A5"));
		m_cofactor = BigInteger.ValueOf(4L);
		m_coord = 6;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecT239K1Curve();
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
		return new SecT239FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecT239K1Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecT239K1Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		ulong[] array = new ulong[len * 4 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat256.Copy64(((SecT239FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 4;
			Nat256.Copy64(((SecT239FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 4;
		}
		return new SecT239K1LookupTable(this, array, len);
	}
}
