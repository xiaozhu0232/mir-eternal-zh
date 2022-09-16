using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP224R1Curve : AbstractFpCurve
{
	private class SecP224R1LookupTable : AbstractECLookupTable
	{
		private readonly SecP224R1Curve m_outer;

		private readonly uint[] m_table;

		private readonly int m_size;

		public override int Size => m_size;

		internal SecP224R1LookupTable(SecP224R1Curve outer, uint[] table, int size)
		{
			m_outer = outer;
			m_table = table;
			m_size = size;
		}

		public override ECPoint Lookup(int index)
		{
			uint[] array = Nat224.Create();
			uint[] array2 = Nat224.Create();
			int num = 0;
			for (int i = 0; i < m_size; i++)
			{
				uint num2 = (uint)((i ^ index) - 1 >> 31);
				for (int j = 0; j < 7; j++)
				{
					uint[] array3;
					uint[] array4 = (array3 = array);
					int num3 = j;
					nint num4 = num3;
					array4[num3] = array3[num4] ^ (m_table[num + j] & num2);
					uint[] array5 = (array3 = array2);
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
			uint[] array = Nat224.Create();
			uint[] array2 = Nat224.Create();
			int num = index * 7 * 2;
			for (int i = 0; i < 7; i++)
			{
				array[i] = m_table[num + i];
				array2[i] = m_table[num + 7 + i];
			}
			return CreatePoint(array, array2);
		}

		private ECPoint CreatePoint(uint[] x, uint[] y)
		{
			return m_outer.CreateRawPoint(new SecP224R1FieldElement(x), new SecP224R1FieldElement(y), SECP224R1_AFFINE_ZS, withCompression: false);
		}
	}

	private const int SECP224R1_DEFAULT_COORDS = 2;

	private const int SECP224R1_FE_INTS = 7;

	public static readonly BigInteger q = SecP224R1FieldElement.Q;

	private static readonly ECFieldElement[] SECP224R1_AFFINE_ZS = new ECFieldElement[1]
	{
		new SecP224R1FieldElement(BigInteger.One)
	};

	protected readonly SecP224R1Point m_infinity;

	public virtual BigInteger Q => q;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => q.BitLength;

	public SecP224R1Curve()
		: base(q)
	{
		m_infinity = new SecP224R1Point(this, null, null);
		m_a = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFE")));
		m_b = FromBigInteger(new BigInteger(1, Hex.DecodeStrict("B4050A850C04B3ABF54132565044B0B7D7BFD8BA270B39432355FFB4")));
		m_order = new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFF16A2E0B8F03E13DD29455C5C2A3D"));
		m_cofactor = BigInteger.One;
		m_coord = 2;
	}

	protected override ECCurve CloneCurve()
	{
		return new SecP224R1Curve();
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
		return new SecP224R1FieldElement(x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new SecP224R1Point(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new SecP224R1Point(this, x, y, zs, withCompression);
	}

	public override ECLookupTable CreateCacheSafeLookupTable(ECPoint[] points, int off, int len)
	{
		uint[] array = new uint[len * 7 * 2];
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			ECPoint eCPoint = points[off + i];
			Nat224.Copy(((SecP224R1FieldElement)eCPoint.RawXCoord).x, 0, array, num);
			num += 7;
			Nat224.Copy(((SecP224R1FieldElement)eCPoint.RawYCoord).x, 0, array, num);
			num += 7;
		}
		return new SecP224R1LookupTable(this, array, len);
	}

	public override ECFieldElement RandomFieldElement(SecureRandom r)
	{
		uint[] array = Nat224.Create();
		SecP224R1Field.Random(r, array);
		return new SecP224R1FieldElement(array);
	}

	public override ECFieldElement RandomFieldElementMult(SecureRandom r)
	{
		uint[] array = Nat224.Create();
		SecP224R1Field.RandomMult(r, array);
		return new SecP224R1FieldElement(array);
	}
}
