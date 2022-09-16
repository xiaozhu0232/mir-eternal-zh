using System;

namespace Org.BouncyCastle.Math.EC;

public class FpCurve : AbstractFpCurve
{
	private const int FP_DEFAULT_COORDS = 4;

	protected readonly BigInteger m_q;

	protected readonly BigInteger m_r;

	protected readonly FpPoint m_infinity;

	public virtual BigInteger Q => m_q;

	public override ECPoint Infinity => m_infinity;

	public override int FieldSize => m_q.BitLength;

	[Obsolete("Use constructor taking order/cofactor")]
	public FpCurve(BigInteger q, BigInteger a, BigInteger b)
		: this(q, a, b, null, null)
	{
	}

	public FpCurve(BigInteger q, BigInteger a, BigInteger b, BigInteger order, BigInteger cofactor)
		: base(q)
	{
		m_q = q;
		m_r = FpFieldElement.CalculateResidue(q);
		m_infinity = new FpPoint(this, null, null, withCompression: false);
		m_a = FromBigInteger(a);
		m_b = FromBigInteger(b);
		m_order = order;
		m_cofactor = cofactor;
		m_coord = 4;
	}

	[Obsolete("Use constructor taking order/cofactor")]
	protected FpCurve(BigInteger q, BigInteger r, ECFieldElement a, ECFieldElement b)
		: this(q, r, a, b, null, null)
	{
	}

	protected FpCurve(BigInteger q, BigInteger r, ECFieldElement a, ECFieldElement b, BigInteger order, BigInteger cofactor)
		: base(q)
	{
		m_q = q;
		m_r = r;
		m_infinity = new FpPoint(this, null, null, withCompression: false);
		m_a = a;
		m_b = b;
		m_order = order;
		m_cofactor = cofactor;
		m_coord = 4;
	}

	protected override ECCurve CloneCurve()
	{
		return new FpCurve(m_q, m_r, m_a, m_b, m_order, m_cofactor);
	}

	public override bool SupportsCoordinateSystem(int coord)
	{
		switch (coord)
		{
		case 0:
		case 1:
		case 2:
		case 4:
			return true;
		default:
			return false;
		}
	}

	public override ECFieldElement FromBigInteger(BigInteger x)
	{
		return new FpFieldElement(m_q, m_r, x);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
	{
		return new FpPoint(this, x, y, withCompression);
	}

	protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
	{
		return new FpPoint(this, x, y, zs, withCompression);
	}

	public override ECPoint ImportPoint(ECPoint p)
	{
		if (this != p.Curve && CoordinateSystem == 2 && !p.IsInfinity)
		{
			switch (p.Curve.CoordinateSystem)
			{
			case 2:
			case 3:
			case 4:
				return new FpPoint(this, FromBigInteger(p.RawXCoord.ToBigInteger()), FromBigInteger(p.RawYCoord.ToBigInteger()), new ECFieldElement[1] { FromBigInteger(p.GetZCoord(0).ToBigInteger()) }, p.IsCompressed);
			}
		}
		return base.ImportPoint(p);
	}
}
