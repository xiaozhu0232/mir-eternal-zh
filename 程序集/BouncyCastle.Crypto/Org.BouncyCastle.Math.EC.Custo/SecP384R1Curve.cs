using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP384R1Curve : AbstractFpCurve
{
	private class SecP384R1LookupTable : AbstractECLookupTable
	{
		private readonly SecP384R1Curve m_outer;

		private readonly uint[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecP384R1LookupTable(SecP384R1Curve outer, uint[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			uint[] array = Nat.Create(12);
			uint[] array2 = Nat.Create(12);
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				uint num2 = (uint)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 12; j++)
				{
					uint[] array3;
					uint[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					uint[] array5 = (array3 = array2);
					int num5 = j;
					num4 = num5;
					array5[num5] = array3[num4] ^ (m_table[num + 12 + j] & num2);
				}
				num += 24;
			}
			return CreatePoint(array, array2);
		}

		public override ECPoint LookupVar(int index)
		{
			uint[] array = Nat.Create(12);
			uint[] array2 = Nat.Create(12);
			int num = index * 12 * 2;
			for (int i = 0; i < 12; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 12 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(uint[] x, uint[] y)
		{
			return m_outer.CreateRawPoint(new SecP384R1FieldElement(x), new SecP384R1FieldElement(y), SECP384R1_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECP384R1_DEFAULT_COORDS = 2;

	private const int SECP384R1_FE_INTS = 12;

	public static readonly BigInteger q = SecP384R1FieldElement.Q;

	private static readonly ECFieldElement[] SECP384R1_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecP384R1FieldElement(BigInteger.One)
	};

	protected readonly SecP384R1Point m_infinity;

	public virtual BigInteger Q => q;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => q.BitLength;

	public SecP384R1Curve()
		: base(q)
	{
		m_infinity = new SecP384R1Point(this, null, null);
		m_a = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFF0000000000000000FFFFFFFC")));
		m_b = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("B3312FA7E23EE7E4988E056BE3F82D19181D9C6EFE8141120314088F5013875AC656398D8A2ED19D2A85C8EDD3EC2AEF")));
		m_order = new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFC7634D81F4372DDF581A0DB248B0A77AECEC196ACCC52973"));
		m_cofactor = BigInteger.One;
		m_coord = 2;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecP384R1Curve();
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
		return new SecP384R1FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecP384R1Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecP384R1Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		uint[] array = new uint[len * 12 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat.Copy(12, ((SecP384R1FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 12;
			Nat.Copy(12, ((SecP384R1FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 12;
		}
		return new SecP384R1LookupTable(this, array, len);
	}

	public override ECFieldElement RandomFieldElement(SecureRandom r)
	{
		uint[] array = Nat.Create(12);
		SecP384R1Field.Random(r, array);
		return new SecP384R1FieldElement(array);
	}

	public override ECFieldElement RandomFieldElementMult(SecureRandom r)
	{
		uint[] array = Nat.Create(12);
		SecP384R1Field.RandomMult(r, array);
		return new SecP384R1FieldElement(array);
	}
}
