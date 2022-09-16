using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP160R2Curve : AbstractFpCurve
{
	private class SecP160R2LookupTable : AbstractECLookupTable
	{
		private readonly SecP160R2Curve m_outer;

		private readonly uint[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecP160R2LookupTable(SecP160R2Curve outer, uint[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			uint[] array = Nat160.Create();
			uint[] array2 = Nat160.Create();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				uint num2 = (uint)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 5; j++)
				{
					uint[] array3;
					uint[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					uint[] array5 = (array3 = array2);
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
			uint[] array = Nat160.Create();
			uint[] array2 = Nat160.Create();
			int num = index * 5 * 2;
			for (int i = 0; i < 5; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 5 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(uint[] x, uint[] y)
		{
			return m_outer.CreateRawPoint(new SecP160R2FieldElement(x), new SecP160R2FieldElement(y), SECP160R2_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECP160R2_DEFAULT_COORDS = 2;

	private const int SECP160R2_FE_INTS = 5;

	public static readonly BigInteger q = SecP160R2FieldElement.Q;

	private static readonly ECFieldElement[] SECP160R2_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecP160R2FieldElement(BigInteger.One)
	};

	protected readonly SecP160R2Point m_infinity;

	public virtual BigInteger Q => q;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => q.BitLength;

	public SecP160R2Curve()
		: base(q)
	{
		m_infinity = new SecP160R2Point(this, null, null);
		m_a = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFAC70")));
		m_b = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("B4E134D3FB59EB8BAB57274904664D5AF50388BA")));
		m_order = new BigInteger(1, Hex.DecodeStrict("0100000000000000000000351EE786A818F3A1A16B"));
		m_cofactor = BigInteger.One;
		m_coord = 2;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecP160R2Curve();
	}

	public override bool SupportsCoordinateSystem(int coord)
	{
		if (coord == 2)
		{
			return true;
		}
		return false;
	}

	public override ECFieldElement FromBigInteger(BigInteger x)
	{
		return new SecP160R2FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecP160R2Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecP160R2Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		uint[] array = new uint[len * 5 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat160.Copy(((SecP160R2FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 5;
			Nat160.Copy(((SecP160R2FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 5;
		}
		return new SecP160R2LookupTable(this, array, len);
	}

	public override ECFieldElement RandomFieldElement(SecureRandom r)
	{
		uint[] array = Nat160.Create();
		SecP160R2Field.Random(r, array);
		return new SecP160R2FieldElement(array);
	}

	public override ECFieldElement RandomFieldElementMult(SecureRandom r)
	{
		uint[] array = Nat160.Create();
		SecP160R2Field.RandomMult(r, array);
		return new SecP160R2FieldElement(array);
	}
}
