using System;

namespace Org.BouncyCastle.Math.EC;

public class F2mPoint : AbstractF2mPoint
{
	public override ECFieldElement YCoord
	{
		get
		{
			int curveCoordinateSystem = CurveCoordinateSystem;
			switch (curveCoordinateSystem)
			{
			case 5:
			case 6:
			{
				ECFieldElement rawXCoord = base.RawXCoord;
				ECFieldElement rawYCoord = base.RawYCoord;
				if (base.IsInfinity || rawXCoord.IsZero)
				{
					return rawYCoord;
				}
				ECFieldElement eCFieldElement = rawYCoord.Add(rawXCoord).Multiply(rawXCoord);
				if (6 == curveCoordinateSystem)
				{
					ECFieldElement eCFieldElement2 = base.RawZCoords[0];
					if (!eCFieldElement2.IsOne)
					{
						eCFieldElement = eCFieldElement.Divide(eCFieldElement2);
					}
				}
				return eCFieldElement;
			}
			default:
				return base.RawYCoord;
			}
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
			switch (CurveCoordinateSystem)
			{
			case 5:
			case 6:
				return rawYCoord.TestBitZero() != rawXCoord.TestBitZero();
			default:
				return rawYCoord.Divide(rawXCoord).TestBitZero();
			}
		}
	}

	[Obsolete("Use ECCurve.CreatePoint to construct points")]
	public F2mPoint(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	[Obsolete("Per-point compression property will be removed, see GetEncoded(bool)")]
	public F2mPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
		if (x != null)
		{
			F2mFieldElement.CheckFieldElements(x, y);
			if (curve != null)
			{
				F2mFieldElement.CheckFieldElements(x, curve.A);
			}
		}
	}

	internal F2mPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new F2mPoint(null, AffineXCoord, AffineYCoord, withCompression: false);
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
		int coordinateSystem = curve.CoordinateSystem;
		ECFieldElement rawXCoord = base.RawXCoord;
		ECFieldElement rawXCoord2 = b.RawXCoord;
		switch (coordinateSystem)
		{
		case 0:
		{
			ECFieldElement rawYCoord5 = base.RawYCoord;
			ECFieldElement rawYCoord6 = b.RawYCoord;
			ECFieldElement eCFieldElement29 = rawXCoord.Add(rawXCoord2);
			ECFieldElement eCFieldElement30 = rawYCoord5.Add(rawYCoord6);
			if (eCFieldElement29.IsZero)
			{
				if (eCFieldElement30.IsZero)
				{
					return Twice();
				}
				return curve.Infinity;
			}
			ECFieldElement eCFieldElement31 = eCFieldElement30.Divide(eCFieldElement29);
			ECFieldElement eCFieldElement32 = eCFieldElement31.Square().Add(eCFieldElement31).Add(eCFieldElement29)
				.Add(curve.A);
			ECFieldElement y3 = eCFieldElement31.Multiply(rawXCoord.Add(eCFieldElement32)).Add(eCFieldElement32).Add(rawYCoord5);
			return new F2mPoint(curve, eCFieldElement32, y3, base.IsCompressed);
		}
		case 1:
		{
			ECFieldElement rawYCoord3 = base.RawYCoord;
			ECFieldElement eCFieldElement16 = base.RawZCoords[0];
			ECFieldElement rawYCoord4 = b.RawYCoord;
			ECFieldElement eCFieldElement17 = b.RawZCoords[0];
			bool isOne3 = eCFieldElement16.IsOne;
			ECFieldElement eCFieldElement18 = rawYCoord4;
			ECFieldElement eCFieldElement19 = rawXCoord2;
			if (!isOne3)
			{
				eCFieldElement18 = eCFieldElement18.Multiply(eCFieldElement16);
				eCFieldElement19 = eCFieldElement19.Multiply(eCFieldElement16);
			}
			bool isOne4 = eCFieldElement17.IsOne;
			ECFieldElement eCFieldElement20 = rawYCoord3;
			ECFieldElement eCFieldElement21 = rawXCoord;
			if (!isOne4)
			{
				eCFieldElement20 = eCFieldElement20.Multiply(eCFieldElement17);
				eCFieldElement21 = eCFieldElement21.Multiply(eCFieldElement17);
			}
			ECFieldElement eCFieldElement22 = eCFieldElement18.Add(eCFieldElement20);
			ECFieldElement eCFieldElement23 = eCFieldElement19.Add(eCFieldElement21);
			if (eCFieldElement23.IsZero)
			{
				if (eCFieldElement22.IsZero)
				{
					return Twice();
				}
				return curve.Infinity;
			}
			ECFieldElement eCFieldElement24 = eCFieldElement23.Square();
			ECFieldElement eCFieldElement25 = eCFieldElement24.Multiply(eCFieldElement23);
			ECFieldElement b3 = (isOne3 ? eCFieldElement17 : (isOne4 ? eCFieldElement16 : eCFieldElement16.Multiply(eCFieldElement17)));
			ECFieldElement eCFieldElement26 = eCFieldElement22.Add(eCFieldElement23);
			ECFieldElement eCFieldElement27 = eCFieldElement26.MultiplyPlusProduct(eCFieldElement22, eCFieldElement24, curve.A).Multiply(b3).Add(eCFieldElement25);
			ECFieldElement x = eCFieldElement23.Multiply(eCFieldElement27);
			ECFieldElement b4 = (isOne4 ? eCFieldElement24 : eCFieldElement24.Multiply(eCFieldElement17));
			ECFieldElement y2 = eCFieldElement22.MultiplyPlusProduct(rawXCoord, eCFieldElement23, rawYCoord3).MultiplyPlusProduct(b4, eCFieldElement26, eCFieldElement27);
			ECFieldElement eCFieldElement28 = eCFieldElement25.Multiply(b3);
			return new F2mPoint(curve, x, y2, new ECFieldElement[1] { eCFieldElement28 }, base.IsCompressed);
		}
		case 6:
		{
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
				rawXCoord = eCPoint.RawXCoord;
				ECFieldElement yCoord = eCPoint.YCoord;
				ECFieldElement b2 = rawYCoord2;
				ECFieldElement eCFieldElement9 = yCoord.Add(b2).Divide(rawXCoord);
				eCFieldElement10 = eCFieldElement9.Square().Add(eCFieldElement9).Add(rawXCoord)
					.Add(curve.A);
				if (eCFieldElement10.IsZero)
				{
					return new F2mPoint(curve, eCFieldElement10, curve.B.Sqrt(), base.IsCompressed);
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
					return new F2mPoint(curve, eCFieldElement10, curve.B.Sqrt(), base.IsCompressed);
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
			return new F2mPoint(curve, eCFieldElement10, y, new ECFieldElement[1] { eCFieldElement12 }, base.IsCompressed);
		}
		default:
			throw new InvalidOperationException("unsupported coordinate system");
		}
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
		switch (curve.CoordinateSystem)
		{
		case 0:
		{
			ECFieldElement rawYCoord2 = base.RawYCoord;
			ECFieldElement eCFieldElement11 = rawYCoord2.Divide(rawXCoord).Add(rawXCoord);
			ECFieldElement x = eCFieldElement11.Square().Add(eCFieldElement11).Add(curve.A);
			ECFieldElement y = rawXCoord.SquarePlusProduct(x, eCFieldElement11.AddOne());
			return new F2mPoint(curve, x, y, base.IsCompressed);
		}
		case 1:
		{
			ECFieldElement rawYCoord3 = base.RawYCoord;
			ECFieldElement eCFieldElement12 = base.RawZCoords[0];
			bool isOne2 = eCFieldElement12.IsOne;
			ECFieldElement eCFieldElement13 = (isOne2 ? rawXCoord : rawXCoord.Multiply(eCFieldElement12));
			ECFieldElement b3 = (isOne2 ? rawYCoord3 : rawYCoord3.Multiply(eCFieldElement12));
			ECFieldElement eCFieldElement14 = rawXCoord.Square();
			ECFieldElement eCFieldElement15 = eCFieldElement14.Add(b3);
			ECFieldElement eCFieldElement16 = eCFieldElement13;
			ECFieldElement eCFieldElement17 = eCFieldElement16.Square();
			ECFieldElement eCFieldElement18 = eCFieldElement15.Add(eCFieldElement16);
			ECFieldElement eCFieldElement19 = eCFieldElement18.MultiplyPlusProduct(eCFieldElement15, eCFieldElement17, curve.A);
			ECFieldElement x2 = eCFieldElement16.Multiply(eCFieldElement19);
			ECFieldElement y2 = eCFieldElement14.Square().MultiplyPlusProduct(eCFieldElement16, eCFieldElement19, eCFieldElement18);
			ECFieldElement eCFieldElement20 = eCFieldElement16.Multiply(eCFieldElement17);
			return new F2mPoint(curve, x2, y2, new ECFieldElement[1] { eCFieldElement20 }, base.IsCompressed);
		}
		case 6:
		{
			ECFieldElement rawYCoord = base.RawYCoord;
			ECFieldElement eCFieldElement = base.RawZCoords[0];
			bool isOne = eCFieldElement.IsOne;
			ECFieldElement eCFieldElement2 = (isOne ? rawYCoord : rawYCoord.Multiply(eCFieldElement));
			ECFieldElement eCFieldElement3 = (isOne ? eCFieldElement : eCFieldElement.Square());
			ECFieldElement a = curve.A;
			ECFieldElement eCFieldElement4 = (isOne ? a : a.Multiply(eCFieldElement3));
			ECFieldElement eCFieldElement5 = rawYCoord.Square().Add(eCFieldElement2).Add(eCFieldElement4);
			if (eCFieldElement5.IsZero)
			{
				return new F2mPoint(curve, eCFieldElement5, curve.B.Sqrt(), base.IsCompressed);
			}
			ECFieldElement eCFieldElement6 = eCFieldElement5.Square();
			ECFieldElement eCFieldElement7 = (isOne ? eCFieldElement5 : eCFieldElement5.Multiply(eCFieldElement3));
			ECFieldElement b = curve.B;
			ECFieldElement eCFieldElement9;
			if (b.BitLength < curve.FieldSize >> 1)
			{
				ECFieldElement eCFieldElement8 = rawYCoord.Add(rawXCoord).Square();
				ECFieldElement b2 = ((!b.IsOne) ? eCFieldElement4.SquarePlusProduct(b, eCFieldElement3.Square()) : eCFieldElement4.Add(eCFieldElement3).Square());
				eCFieldElement9 = eCFieldElement8.Add(eCFieldElement5).Add(eCFieldElement3).Multiply(eCFieldElement8)
					.Add(b2)
					.Add(eCFieldElement6);
				if (a.IsZero)
				{
					eCFieldElement9 = eCFieldElement9.Add(eCFieldElement7);
				}
				else if (!a.IsOne)
				{
					eCFieldElement9 = eCFieldElement9.Add(a.AddOne().Multiply(eCFieldElement7));
				}
			}
			else
			{
				ECFieldElement eCFieldElement10 = (isOne ? rawXCoord : rawXCoord.Multiply(eCFieldElement));
				eCFieldElement9 = eCFieldElement10.SquarePlusProduct(eCFieldElement5, eCFieldElement2).Add(eCFieldElement6).Add(eCFieldElement7);
			}
			return new F2mPoint(curve, eCFieldElement6, eCFieldElement9, new ECFieldElement[1] { eCFieldElement7 }, base.IsCompressed);
		}
		default:
			throw new InvalidOperationException("unsupported coordinate system");
		}
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
		int coordinateSystem = curve.CoordinateSystem;
		int num = coordinateSystem;
		if (num == 6)
		{
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
				return new F2mPoint(curve, eCFieldElement5, curve.B.Sqrt(), base.IsCompressed);
			}
			ECFieldElement x2 = eCFieldElement5.Square().Multiply(eCFieldElement6);
			ECFieldElement eCFieldElement8 = eCFieldElement5.Multiply(eCFieldElement7).Multiply(eCFieldElement3);
			ECFieldElement y = eCFieldElement5.Add(eCFieldElement7).Square().MultiplyPlusProduct(b4, eCFieldElement4, eCFieldElement8);
			return new F2mPoint(curve, x2, y, new ECFieldElement[1] { eCFieldElement8 }, base.IsCompressed);
		}
		return Twice().Add(b);
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
		ECCurve curve = Curve;
		switch (curve.CoordinateSystem)
		{
		case 0:
		{
			ECFieldElement rawYCoord4 = base.RawYCoord;
			return new F2mPoint(curve, rawXCoord, rawYCoord4.Add(rawXCoord), base.IsCompressed);
		}
		case 1:
		{
			ECFieldElement rawYCoord3 = base.RawYCoord;
			ECFieldElement eCFieldElement2 = base.RawZCoords[0];
			return new F2mPoint(curve, rawXCoord, rawYCoord3.Add(rawXCoord), new ECFieldElement[1] { eCFieldElement2 }, base.IsCompressed);
		}
		case 5:
		{
			ECFieldElement rawYCoord2 = base.RawYCoord;
			return new F2mPoint(curve, rawXCoord, rawYCoord2.AddOne(), base.IsCompressed);
		}
		case 6:
		{
			ECFieldElement rawYCoord = base.RawYCoord;
			ECFieldElement eCFieldElement = base.RawZCoords[0];
			return new F2mPoint(curve, rawXCoord, rawYCoord.Add(eCFieldElement), new ECFieldElement[1] { eCFieldElement }, base.IsCompressed);
		}
		default:
			throw new InvalidOperationException("unsupported coordinate system");
		}
	}
}
