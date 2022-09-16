using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP256R1Curve : AbstractFpCurve
{
	private class SecP256R1LookupTable : AbstractECLookupTable
	{
		private readonly SecP256R1Curve m_outer;

		private readonly uint[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecP256R1LookupTable(SecP256R1Curve outer, uint[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			uint[] array = Nat256.Create();
			uint[] array2 = Nat256.Create();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				uint num2 = (uint)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 8; j++)
				{
					uint[] array3;
					uint[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					uint[] array5 = (array3 = array2);
					int num5 = j;
					num4 = num5;
					array5[num5] = array3[num4] ^ (m_table[num + 8 + j] & num2);
				}
				num += 16;
			}
			return CreatePoint(array, array2);
		}

		public override ECPoint LookupVar(int index)
		{
			uint[] array = Nat256.Create();
			uint[] array2 = Nat256.Create();
			int num = index * 8 * 2;
			for (int i = 0; i < 8; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 8 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(uint[] x, uint[] y)
		{
			return m_outer.CreateRawPoint(new SecP256R1FieldElement(x), new SecP256R1FieldElement(y), SECP256R1_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECP256R1_DEFAULT_COORDS = 2;

	private const int SECP256R1_FE_INTS = 8;

	public static readonly BigInteger q = SecP256R1FieldElement.Q;

	private static readonly ECFieldElement[] SECP256R1_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecP256R1FieldElement(BigInteger.One)
	};

	protected readonly SecP256R1Point m_infinity;

	public virtual BigInteger Q => q;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => q.BitLength;

	public SecP256R1Curve()
		: base(q)
	{
		m_infinity = new SecP256R1Point(this, null, null);
		m_a = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFC")));
		m_b = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("5AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E27D2604B")));
		m_order = new BigInteger(1, Hex.DecodeStrict("FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551"));
		m_cofactor = BigInteger.One;
		m_coord = 2;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecP256R1Curve();
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
		return new SecP256R1FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecP256R1Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecP256R1Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		uint[] array = new uint[len * 8 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat256.Copy(((SecP256R1FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 8;
			Nat256.Copy(((SecP256R1FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 8;
		}
		return new SecP256R1LookupTable(this, array, len);
	}

	public override ECFieldElement RandomFieldElement(SecureRandom r)
	{
		uint[] array = Nat256.Create();
		SecP256R1Field.Random(r, array);
		return new SecP256R1FieldElement(array);
	}

	public override ECFieldElement RandomFieldElementMult(SecureRandom r)
	{
		uint[] array = Nat256.Create();
		SecP256R1Field.RandomMult(r, array);
		return new SecP256R1FieldElement(array);
	}
}
