using System;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT193R1Point : AbstractF2mPoint
{
	public override ECFieldElement YCoord
	{
		get
		{
			ECFieldElement rawXCoord = base.RawXCoord;
			ECFieldElement rawYCoord = base.RawYCoord;
			if (base.IsInfinity || rawXCoord.IsZero)
			{
				return rawYCoord;
			}
			ECFieldElement eCFieldElement = rawYCoord.Add(rawXCoord).Multiply(rawXCoord);
			ECFieldElement eCFieldElement2 = base.RawZCoords[0];
			if (!eCFieldElement2.IsOne)
			{
				eCFieldElement = eCFieldElement.Divide(eCFieldElement2);
			}
			return eCFieldElement;
		}
	}

	protected internal override bool CompressionYTilde
	{
		get
		{
			ECFieldElement rawXCoord = base.RawXCoord;
			if (rawXCoord.IsZero)
			{
				return false;
			}
			ECFieldElement rawYCoord = base.RawYCoord;
			return rawYCoord.TestBitZero() != rawXCoord.TestBitZero();
		}
	}

	public SecT193R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	public SecT193R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal SecT193R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new SecT193R1Point(null, AffineXCoord, AffineYCoord);
	}

	public override ECPoint Add(ECPoint b)
	{
		if (base.IsInfinity)
		{
			return b;
		}
		if (b.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		ECFieldElement rawXCoord = base.RawXCoord;
		ECFieldElement rawXCoord2 = b.RawXCoord;
		if (rawXCoord.IsZero)
		{
			if (rawXCoord2.IsZero)
			{
				return curve.Infinity;
			}
			return b.Add(this);
		}
		ECFieldElement rawYCoord = base.RawYCoord;
		ECFieldElement eCFieldElement = base.RawZCoords[0];
		ECFieldElement rawYCoord2 = b.RawYCoord;
		ECFieldElement eCFieldElement2 = b.RawZCoords[0];
		bool isOne = eCFieldElement.IsOne;
		ECFieldElement eCFieldElement3 = rawXCoord2;
		ECFieldElement eCFieldElement4 = rawYCoord2;
		if (!isOne)
		{
			eCFieldElement3 = eCFieldElement3.Multiply(eCFieldElement);
			eCFieldElement4 = eCFieldElement4.Multiply(eCFieldElement);
		}
		bool isOne2 = eCFieldElement2.IsOne;
		ECFieldElement eCFieldElement5 = rawXCoord;
		ECFieldElement eCFieldElement6 = rawYCoord;
		if (!isOne2)
		{
			eCFieldElement5 = eCFieldElement5.Multiply(eCFieldElement2);
			eCFieldElement6 = eCFieldElement6.Multiply(eCFieldElement2);
		}
		ECFieldElement eCFieldElement7 = eCFieldElement6.Add(eCFieldElement4);
		ECFieldElement eCFieldElement8 = eCFieldElement5.Add(eCFieldElement3);
		if (eCFieldElement8.IsZero)
		{
			if (eCFieldElement7.IsZero)
			{
				return Twice();
			}
			return curve.Infinity;
		}
		ECFieldElement eCFieldElement10;
		ECFieldElement y;
		ECFieldElement eCFieldElement12;
		if (rawXCoord2.IsZero)
		{
			ECPoint eCPoint = Normalize();
			rawXCoord = eCPoint.XCoord;
			ECFieldElement yCoord = eCPoint.YCoord;
			ECFieldElement b2 = rawYCoord2;
			ECFieldElement eCFieldElement9 = yCoord.Add(b2).Divide(rawXCoord);
			eCFieldElement10 = eCFieldElement9.Square().Add(eCFieldElement9).Add(rawXCoord)
				.Add(curve.A);
			if (eCFieldElement10.IsZero)
			{
				return new SecT193R1Point(curve, eCFieldElement10, curve.B.Sqrt(), base.IsCompressed);
			}
			ECFieldElement eCFieldElement11 = eCFieldElement9.Multiply(rawXCoord.Add(eCFieldElement10)).Add(eCFieldElement10).Add(yCoord);
			y = eCFieldElement11.Divide(eCFieldElement10).Add(eCFieldElement10);
			eCFieldElement12 = curve.FromBigInteger(BigInteger.One);
		}
		else
		{
			eCFieldElement8 = eCFieldElement8.Square();
			ECFieldElement eCFieldElement13 = eCFieldElement7.Multiply(eCFieldElement5);
			ECFieldElement eCFieldElement14 = eCFieldElement7.Multiply(eCFieldElement3);
			eCFieldElement10 = eCFieldElement13.Multiply(eCFieldElement14);
			if (eCFieldElement10.IsZero)
			{
				return new SecT193R1Point(curve, eCFieldElement10, curve.B.Sqrt(), base.IsCompressed);
			}
			ECFieldElement eCFieldElement15 = eCFieldElement7.Multiply(eCFieldElement8);
			if (!isOne2)
			{
				eCFieldElement15 = eCFieldElement15.Multiply(eCFieldElement2);
			}
			y = eCFieldElement14.Add(eCFieldElement8).SquarePlusProduct(eCFieldElement15, rawYCoord.Add(eCFieldElement));
			eCFieldElement12 = eCFieldElement15;
			if (!isOne)
			{
				eCFieldElement12 = eCFieldElement12.Multiply(eCFieldElement);
			}
		}
		return new SecT193R1Point(curve, eCFieldElement10, y, new ECFieldElement[1] { eCFieldElement12 }, base.IsCompressed);
	}

	public override ECPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		ECFieldElement rawXCoord = base.RawXCoord;
		if (rawXCoord.IsZero)
		{
			return curve.Infinity;
		}
		ECFieldElement rawYCoord = base.RawYCoord;
		ECFieldElement eCFieldElement = base.RawZCoords[0];
		bool isOne = eCFieldElement.IsOne;
		ECFieldElement eCFieldElement2 = (isOne ? rawYCoord : rawYCoord.Multiply(eCFieldElement));
		ECFieldElement b = (isOne ? eCFieldElement : eCFieldElement.Square());
		ECFieldElement a = curve.A;
		ECFieldElement b2 = (isOne ? a : a.Multiply(b));
		ECFieldElement eCFieldElement3 = rawYCoord.Square().Add(eCFieldElement2).Add(b2);
		if (eCFieldElement3.IsZero)
		{
			return new SecT193R1Point(curve, eCFieldElement3, curve.B.Sqrt(), base.IsCompressed);
		}
		ECFieldElement eCFieldElement4 = eCFieldElement3.Square();
		ECFieldElement eCFieldElement5 = (isOne ? eCFieldElement3 : eCFieldElement3.Multiply(b));
		ECFieldElement eCFieldElement6 = (isOne ? rawXCoord : rawXCoord.Multiply(eCFieldElement));
		ECFieldElement y = eCFieldElement6.SquarePlusProduct(eCFieldElement3, eCFieldElement2).Add(eCFieldElement4).Add(eCFieldElement5);
		return new SecT193R1Point(curve, eCFieldElement4, y, new ECFieldElement[1] { eCFieldElement5 }, base.IsCompressed);
	}

	public override ECPoint TwicePlus(ECPoint b)
	{
		if (base.IsInfinity)
		{
			return b;
		}
		if (b.IsInfinity)
		{
			return Twice();
		}
		ECCurve curve = Curve;
		ECFieldElement rawXCoord = base.RawXCoord;
		if (rawXCoord.IsZero)
		{
			return b;
		}
		ECFieldElement rawXCoord2 = b.RawXCoord;
		ECFieldElement eCFieldElement = b.RawZCoords[0];
		if (rawXCoord2.IsZero || !eCFieldElement.IsOne)
		{
			return Twice().Add(b);
		}
		ECFieldElement rawYCoord = base.RawYCoord;
		ECFieldElement eCFieldElement2 = base.RawZCoords[0];
		ECFieldElement rawYCoord2 = b.RawYCoord;
		ECFieldElement x = rawXCoord.Square();
		ECFieldElement b2 = rawYCoord.Square();
		ECFieldElement eCFieldElement3 = eCFieldElement2.Square();
		ECFieldElement b3 = rawYCoord.Multiply(eCFieldElement2);
		ECFieldElement b4 = curve.A.Multiply(eCFieldElement3).Add(b2).Add(b3);
		ECFieldElement eCFieldElement4 = rawYCoord2.AddOne();
		ECFieldElement eCFieldElement5 = curve.A.Add(eCFieldElement4).Multiply(eCFieldElement3).Add(b2)
			.MultiplyPlusProduct(b4, x, eCFieldElement3);
		ECFieldElement eCFieldElement6 = rawXCoord2.Multiply(eCFieldElement3);
		ECFieldElement eCFieldElement7 = eCFieldElement6.Add(b4).Square();
		if (eCFieldElement7.IsZero)
		{
			if (eCFieldElement5.IsZero)
			{
				return b.Twice();
			}
			return curve.Infinity;
		}
		if (eCFieldElement5.IsZero)
		{
			return new SecT193R1Point(curve, eCFieldElement5, curve.B.Sqrt(), base.IsCompressed);
		}
		ECFieldElement x2 = eCFieldElement5.Square().Multiply(eCFieldElement6);
		ECFieldElement eCFieldElement8 = eCFieldElement5.Multiply(eCFieldElement7).Multiply(eCFieldElement3);
		ECFieldElement y = eCFieldElement5.Add(eCFieldElement7).Square().MultiplyPlusProduct(b4, eCFieldElement4, eCFieldElement8);
		return new SecT193R1Point(curve, x2, y, new ECFieldElement[1] { eCFieldElement8 }, base.IsCompressed);
	}

	public override ECPoint Negate()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECFieldElement rawXCoord = base.RawXCoord;
		if (rawXCoord.IsZero)
		{
			return this;
		}
		ECFieldElement rawYCoord = base.RawYCoord;
		ECFieldElement eCFieldElement = base.RawZCoords[0];
		return new SecT193R1Point(Curve, rawXCoord, rawYCoord.Add(eCFieldElement), new ECFieldElement[1] { eCFieldElement }, base.IsCompressed);
	}
}
