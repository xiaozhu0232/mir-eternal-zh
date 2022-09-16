using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT283R1Curve : AbstractF2mCurve
{
	private class SecT283R1LookupTable : AbstractECLookupTable
	{
		private readonly SecT283R1Curve m_outer;

		private readonly ulong[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecT283R1LookupTable(SecT283R1Curve outer, ulong[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			ulong[] array = Nat320.Create64();
			ulong[] array2 = Nat320.Create64();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				ulong num2 = (ulong)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 5; j++)
				{
					ulong[] array3;
					ulong[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					ulong[] array5 = (array3 = array2);
					int num5 = j;
					num4 = num5;
					array5[num5] = array3[num4] ^ (m_table[num + 5 + j] & num2);
				}
				num += 10;
			}
			return CreatePoint(array, array2);
		}

		public override ECPoint LookupVar(int index)
		{
			ulong[] array = Nat320.Create64();
			ulong[] array2 = Nat320.Create64();
			int num = index * 5 * 2;
			for (int i = 0; i < 5; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 5 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(ulong[] x, ulong[] y)
		{
			return m_outer.CreateRawPoint(new SecT283FieldElement(x), new SecT283FieldElement(y), SECT283R1_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECT283R1_DEFAULT_COORDS = 6;

	private const int SECT283R1_FE_LONGS = 5;

	private static readonly ECFieldElement[] SECT283R1_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecT283FieldElement(BigInteger.One)
	};

	protected readonly SecT283R1Point m_infinity;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => 283;

	public override bool IsKoblitz => false;

	public virtual int M => 283;

	public virtual bool IsTrinomial => false;

	public virtual int K1 => 5;

	public virtual int K2 => 7;

	public virtual int K3 => 12;

	public SecT283R1Curve()
		: base(283, 5, 7, 12)
	{
		m_infinity = new SecT283R1Point(this, null, null);
		m_a = FromBigInteger(BigInteger.One);
		m_b = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("027B680AC8B8596DA5A4AF8A19A0303FCA97FD7645309FA2A581485AF6263E313B79A2F5")));
		m_order = new BigInteger(1, Hex.DecodeStrict("03FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEF90399660FC938A90165B042A7CEFADB307"));
		m_cofactor = BigInteger.Two;
		m_coord = 6;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecT283R1Curve();
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
		return new SecT283FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecT283R1Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecT283R1Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		ulong[] array = new ulong[len * 5 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat320.Copy64(((SecT283FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 5;
			Nat320.Copy64(((SecT283FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 5;
		}
		return new SecT283R1LookupTable(this, array, len);
	}
}
