using System;

namespace Org.BouncyCastle.Math.EC;

public abstract class AbstractFpPoint : ECPointBase
{
	protected internal override bool CompressionYTilde => AffineYCoord.TestBitZero();

	protected AbstractFpPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
	}

	protected AbstractFpPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override bool SatisfiesCurveEquation()
	{
		ECFieldElement rawXCoord = base.RawXCoord;
		ECFieldElement rawYCoord = base.RawYCoord;
		ECFieldElement eCFieldElement = Curve.A;
		ECFieldElement eCFieldElement2 = Curve.B;
		ECFieldElement eCFieldElement3 = rawYCoord.Square();
		switch (CurveCoordinateSystem)
		{
		case 1:
		{
			ECFieldElement eCFieldElement6 = base.RawZCoords[0];
			if (!eCFieldElement6.IsOne)
			{
				ECFieldElement b3 = eCFieldElement6.Square();
				ECFieldElement b4 = eCFieldElement6.Multiply(b3);
				eCFieldElement3 = eCFieldElement3.Multiply(eCFieldElement6);
				eCFieldElement = eCFieldElement.Multiply(b3);
				eCFieldElement2 = eCFieldElement2.Multiply(b4);
			}
			break;
		}
		case 2:
		case 3:
		case 4:
		{
			ECFieldElement eCFieldElement4 = base.RawZCoords[0];
			if (!eCFieldElement4.IsOne)
			{
				ECFieldElement eCFieldElement5 = eCFieldElement4.Square();
				ECFieldElement b = eCFieldElement5.Square();
				ECFieldElement b2 = eCFieldElement5.Multiply(b);
				eCFieldElement = eCFieldElement.Multiply(b);
				eCFieldElement2 = eCFieldElement2.Multiply(b2);
			}
			break;
		}
		default:
			throw new InvalidOperationException("unsupported coordinate system");
		case 0:
			break;
		}
		ECFieldElement other = rawXCoord.Square().Add(eCFieldElement).Multiply(rawXCoord)
			.Add(eCFieldElement2);
		return eCFieldElement3.Equals(other);
	}

	public override ECPoint Subtract(ECPoint b)
	{
		if (b.IsInfinity)
		{
			return this;
		}
		return Add(b.Negate());
	}
}
