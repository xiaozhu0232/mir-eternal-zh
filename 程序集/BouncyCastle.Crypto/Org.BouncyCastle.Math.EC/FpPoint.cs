using System;

namespace Org.BouncyCastle.Math.EC;

public class FpPoint : AbstractFpPoint
{
	[Obsolete("Use ECCurve.CreatePoint to construct points")]
	public FpPoint(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	[Obsolete("Per-point compression property will be removed, see GetEncoded(bool)")]
	public FpPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal FpPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new FpPoint(null, AffineXCoord, AffineYCoord, withCompression: false);
	}

	public override ECFieldElement GetZCoord(int index)
	{
		if (index == 1 && 4 == CurveCoordinateSystem)
		{
			return GetJacobianModifiedW();
		}
		return base.GetZCoord(index);
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
		if (this == b)
		{
			return Twice();
		}
		ECCurve curve = Curve;
		int coordinateSystem = curve.CoordinateSystem;
		ECFieldElement rawXCoord = base.RawXCoord;
		ECFieldElement rawYCoord = base.RawYCoord;
		ECFieldElement rawXCoord2 = b.RawXCoord;
		ECFieldElement rawYCoord2 = b.RawYCoord;
		switch (coordinateSystem)
		{
		case 0:
		{
			ECFieldElement eCFieldElement32 = rawXCoord2.Subtract(rawXCoord);
			ECFieldElement eCFieldElement33 = rawYCoord2.Subtract(rawYCoord);
			if (eCFieldElement32.IsZero)
			{
				if (eCFieldElement33.IsZero)
				{
					return Twice();
				}
				return Curve.Infinity;
			}
			ECFieldElement eCFieldElement34 = eCFieldElement33.Divide(eCFieldElement32);
			ECFieldElement eCFieldElement35 = eCFieldElement34.Square().Subtract(rawXCoord).Subtract(rawXCoord2);
			ECFieldElement y3 = eCFieldElement34.Multiply(rawXCoord.Subtract(eCFieldElement35)).Subtract(rawYCoord);
			return new FpPoint(Curve, eCFieldElement35, y3, base.IsCompressed);
		}
		case 1:
		{
			ECFieldElement eCFieldElement21 = base.RawZCoords[0];
			ECFieldElement eCFieldElement22 = b.RawZCoords[0];
			bool isOne3 = eCFieldElement21.IsOne;
			bool isOne4 = eCFieldElement22.IsOne;
			ECFieldElement eCFieldElement23 = (isOne3 ? rawYCoord2 : rawYCoord2.Multiply(eCFieldElement21));
			ECFieldElement eCFieldElement24 = (isOne4 ? rawYCoord : rawYCoord.Multiply(eCFieldElement22));
			ECFieldElement eCFieldElement25 = eCFieldElement23.Subtract(eCFieldElement24);
			ECFieldElement eCFieldElement26 = (isOne3 ? rawXCoord2 : rawXCoord2.Multiply(eCFieldElement21));
			ECFieldElement b6 = (isOne4 ? rawXCoord : rawXCoord.Multiply(eCFieldElement22));
			ECFieldElement eCFieldElement27 = eCFieldElement26.Subtract(b6);
			if (eCFieldElement27.IsZero)
			{
				if (eCFieldElement25.IsZero)
				{
					return Twice();
				}
				return curve.Infinity;
			}
			ECFieldElement b7 = (isOne3 ? eCFieldElement22 : (isOne4 ? eCFieldElement21 : eCFieldElement21.Multiply(eCFieldElement22)));
			ECFieldElement eCFieldElement28 = eCFieldElement27.Square();
			ECFieldElement eCFieldElement29 = eCFieldElement28.Multiply(eCFieldElement27);
			ECFieldElement eCFieldElement30 = eCFieldElement28.Multiply(b6);
			ECFieldElement b8 = eCFieldElement25.Square().Multiply(b7).Subtract(eCFieldElement29)
				.Subtract(Two(eCFieldElement30));
			ECFieldElement x = eCFieldElement27.Multiply(b8);
			ECFieldElement y2 = eCFieldElement30.Subtract(b8).MultiplyMinusProduct(eCFieldElement25, eCFieldElement24, eCFieldElement29);
			ECFieldElement eCFieldElement31 = eCFieldElement29.Multiply(b7);
			return new FpPoint(curve, x, y2, new ECFieldElement[1] { eCFieldElement31 }, base.IsCompressed);
		}
		case 2:
		case 4:
		{
			ECFieldElement eCFieldElement = base.RawZCoords[0];
			ECFieldElement eCFieldElement2 = b.RawZCoords[0];
			bool isOne = eCFieldElement.IsOne;
			ECFieldElement zSquared = null;
			ECFieldElement eCFieldElement7;
			ECFieldElement y;
			ECFieldElement eCFieldElement8;
			if (!isOne && eCFieldElement.Equals(eCFieldElement2))
			{
				ECFieldElement eCFieldElement3 = rawXCoord.Subtract(rawXCoord2);
				ECFieldElement eCFieldElement4 = rawYCoord.Subtract(rawYCoord2);
				if (eCFieldElement3.IsZero)
				{
					if (eCFieldElement4.IsZero)
					{
						return Twice();
					}
					return curve.Infinity;
				}
				ECFieldElement eCFieldElement5 = eCFieldElement3.Square();
				ECFieldElement eCFieldElement6 = rawXCoord.Multiply(eCFieldElement5);
				ECFieldElement b2 = rawXCoord2.Multiply(eCFieldElement5);
				ECFieldElement b3 = eCFieldElement6.Subtract(b2).Multiply(rawYCoord);
				eCFieldElement7 = eCFieldElement4.Square().Subtract(eCFieldElement6).Subtract(b2);
				y = eCFieldElement6.Subtract(eCFieldElement7).Multiply(eCFieldElement4).Subtract(b3);
				eCFieldElement8 = eCFieldElement3;
				if (isOne)
				{
					zSquared = eCFieldElement5;
				}
				else
				{
					eCFieldElement8 = eCFieldElement8.Multiply(eCFieldElement);
				}
			}
			else
			{
				ECFieldElement b4;
				ECFieldElement b5;
				if (isOne)
				{
					ECFieldElement eCFieldElement9 = eCFieldElement;
					b4 = rawXCoord2;
					b5 = rawYCoord2;
				}
				else
				{
					ECFieldElement eCFieldElement9 = eCFieldElement.Square();
					b4 = eCFieldElement9.Multiply(rawXCoord2);
					ECFieldElement eCFieldElement10 = eCFieldElement9.Multiply(eCFieldElement);
					b5 = eCFieldElement10.Multiply(rawYCoord2);
				}
				bool isOne2 = eCFieldElement2.IsOne;
				ECFieldElement eCFieldElement12;
				ECFieldElement eCFieldElement13;
				if (isOne2)
				{
					ECFieldElement eCFieldElement11 = eCFieldElement2;
					eCFieldElement12 = rawXCoord;
					eCFieldElement13 = rawYCoord;
				}
				else
				{
					ECFieldElement eCFieldElement11 = eCFieldElement2.Square();
					eCFieldElement12 = eCFieldElement11.Multiply(rawXCoord);
					ECFieldElement eCFieldElement14 = eCFieldElement11.Multiply(eCFieldElement2);
					eCFieldElement13 = eCFieldElement14.Multiply(rawYCoord);
				}
				ECFieldElement eCFieldElement15 = eCFieldElement12.Subtract(b4);
				ECFieldElement eCFieldElement16 = eCFieldElement13.Subtract(b5);
				if (eCFieldElement15.IsZero)
				{
					if (eCFieldElement16.IsZero)
					{
						return Twice();
					}
					return curve.Infinity;
				}
				ECFieldElement eCFieldElement17 = eCFieldElement15.Square();
				ECFieldElement eCFieldElement18 = eCFieldElement17.Multiply(eCFieldElement15);
				ECFieldElement eCFieldElement19 = eCFieldElement17.Multiply(eCFieldElement12);
				eCFieldElement7 = eCFieldElement16.Square().Add(eCFieldElement18).Subtract(Two(eCFieldElement19));
				y = eCFieldElement19.Subtract(eCFieldElement7).MultiplyMinusProduct(eCFieldElement16, eCFieldElement18, eCFieldElement13);
				eCFieldElement8 = eCFieldElement15;
				if (!isOne)
				{
					eCFieldElement8 = eCFieldElement8.Multiply(eCFieldElement);
				}
				if (!isOne2)
				{
					eCFieldElement8 = eCFieldElement8.Multiply(eCFieldElement2);
				}
				if (eCFieldElement8 == eCFieldElement15)
				{
					zSquared = eCFieldElement17;
				}
			}
			ECFieldElement[] zs;
			if (coordinateSystem == 4)
			{
				ECFieldElement eCFieldElement20 = CalculateJacobianModifiedW(eCFieldElement8, zSquared);
				zs = new ECFieldElement[2] { eCFieldElement8, eCFieldElement20 };
			}
			else
			{
				zs = new ECFieldElement[1] { eCFieldElement8 };
			}
			return new FpPoint(curve, eCFieldElement7, y, zs, base.IsCompressed);
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
		ECFieldElement rawYCoord = base.RawYCoord;
		if (rawYCoord.IsZero)
		{
			return curve.Infinity;
		}
		int coordinateSystem = curve.CoordinateSystem;
		ECFieldElement rawXCoord = base.RawXCoord;
		switch (coordinateSystem)
		{
		case 0:
		{
			ECFieldElement x3 = rawXCoord.Square();
			ECFieldElement eCFieldElement10 = Three(x3).Add(Curve.A).Divide(Two(rawYCoord));
			ECFieldElement eCFieldElement11 = eCFieldElement10.Square().Subtract(Two(rawXCoord));
			ECFieldElement y2 = eCFieldElement10.Multiply(rawXCoord.Subtract(eCFieldElement11)).Subtract(rawYCoord);
			return new FpPoint(Curve, eCFieldElement11, y2, base.IsCompressed);
		}
		case 1:
		{
			ECFieldElement eCFieldElement12 = base.RawZCoords[0];
			bool isOne2 = eCFieldElement12.IsOne;
			ECFieldElement eCFieldElement13 = curve.A;
			if (!eCFieldElement13.IsZero && !isOne2)
			{
				eCFieldElement13 = eCFieldElement13.Multiply(eCFieldElement12.Square());
			}
			eCFieldElement13 = eCFieldElement13.Add(Three(rawXCoord.Square()));
			ECFieldElement eCFieldElement14 = (isOne2 ? rawYCoord : rawYCoord.Multiply(eCFieldElement12));
			ECFieldElement eCFieldElement15 = (isOne2 ? rawYCoord.Square() : eCFieldElement14.Multiply(rawYCoord));
			ECFieldElement x4 = rawXCoord.Multiply(eCFieldElement15);
			ECFieldElement eCFieldElement16 = Four(x4);
			ECFieldElement eCFieldElement17 = eCFieldElement13.Square().Subtract(Two(eCFieldElement16));
			ECFieldElement eCFieldElement18 = Two(eCFieldElement14);
			ECFieldElement x5 = eCFieldElement17.Multiply(eCFieldElement18);
			ECFieldElement eCFieldElement19 = Two(eCFieldElement15);
			ECFieldElement y3 = eCFieldElement16.Subtract(eCFieldElement17).Multiply(eCFieldElement13).Subtract(Two(eCFieldElement19.Square()));
			ECFieldElement x6 = (isOne2 ? Two(eCFieldElement19) : eCFieldElement18.Square());
			ECFieldElement eCFieldElement20 = Two(x6).Multiply(eCFieldElement14);
			return new FpPoint(curve, x5, y3, new ECFieldElement[1] { eCFieldElement20 }, base.IsCompressed);
		}
		case 2:
		{
			ECFieldElement eCFieldElement = base.RawZCoords[0];
			bool isOne = eCFieldElement.IsOne;
			ECFieldElement eCFieldElement2 = rawYCoord.Square();
			ECFieldElement x = eCFieldElement2.Square();
			ECFieldElement a = curve.A;
			ECFieldElement eCFieldElement3 = a.Negate();
			ECFieldElement eCFieldElement4;
			ECFieldElement eCFieldElement5;
			if (eCFieldElement3.ToBigInteger().Equals(BigInteger.ValueOf(3L)))
			{
				ECFieldElement b = (isOne ? eCFieldElement : eCFieldElement.Square());
				eCFieldElement4 = Three(rawXCoord.Add(b).Multiply(rawXCoord.Subtract(b)));
				eCFieldElement5 = Four(eCFieldElement2.Multiply(rawXCoord));
			}
			else
			{
				ECFieldElement x2 = rawXCoord.Square();
				eCFieldElement4 = Three(x2);
				if (isOne)
				{
					eCFieldElement4 = eCFieldElement4.Add(a);
				}
				else if (!a.IsZero)
				{
					ECFieldElement eCFieldElement6 = (isOne ? eCFieldElement : eCFieldElement.Square());
					ECFieldElement eCFieldElement7 = eCFieldElement6.Square();
					eCFieldElement4 = ((eCFieldElement3.BitLength >= a.BitLength) ? eCFieldElement4.Add(eCFieldElement7.Multiply(a)) : eCFieldElement4.Subtract(eCFieldElement7.Multiply(eCFieldElement3)));
				}
				eCFieldElement5 = Four(rawXCoord.Multiply(eCFieldElement2));
			}
			ECFieldElement eCFieldElement8 = eCFieldElement4.Square().Subtract(Two(eCFieldElement5));
			ECFieldElement y = eCFieldElement5.Subtract(eCFieldElement8).Multiply(eCFieldElement4).Subtract(Eight(x));
			ECFieldElement eCFieldElement9 = Two(rawYCoord);
			if (!isOne)
			{
				eCFieldElement9 = eCFieldElement9.Multiply(eCFieldElement);
			}
			return new FpPoint(curve, eCFieldElement8, y, new ECFieldElement[1] { eCFieldElement9 }, base.IsCompressed);
		}
		case 4:
			return TwiceJacobianModified(calculateW: true);
		default:
			throw new InvalidOperationException("unsupported coordinate system");
		}
	}

	public override ECPoint TwicePlus(ECPoint b)
	{
		if (this == b)
		{
			return ThreeTimes();
		}
		if (base.IsInfinity)
		{
			return b;
		}
		if (b.IsInfinity)
		{
			return Twice();
		}
		ECFieldElement rawYCoord = base.RawYCoord;
		if (rawYCoord.IsZero)
		{
			return b;
		}
		ECCurve curve = Curve;
		switch (curve.CoordinateSystem)
		{
		case 0:
		{
			ECFieldElement rawXCoord = base.RawXCoord;
			ECFieldElement rawXCoord2 = b.RawXCoord;
			ECFieldElement rawYCoord2 = b.RawYCoord;
			ECFieldElement eCFieldElement = rawXCoord2.Subtract(rawXCoord);
			ECFieldElement eCFieldElement2 = rawYCoord2.Subtract(rawYCoord);
			if (eCFieldElement.IsZero)
			{
				if (eCFieldElement2.IsZero)
				{
					return ThreeTimes();
				}
				return this;
			}
			ECFieldElement eCFieldElement3 = eCFieldElement.Square();
			ECFieldElement b2 = eCFieldElement2.Square();
			ECFieldElement eCFieldElement4 = eCFieldElement3.Multiply(Two(rawXCoord).Add(rawXCoord2)).Subtract(b2);
			if (eCFieldElement4.IsZero)
			{
				return Curve.Infinity;
			}
			ECFieldElement eCFieldElement5 = eCFieldElement4.Multiply(eCFieldElement);
			ECFieldElement b3 = eCFieldElement5.Invert();
			ECFieldElement eCFieldElement6 = eCFieldElement4.Multiply(b3).Multiply(eCFieldElement2);
			ECFieldElement eCFieldElement7 = Two(rawYCoord).Multiply(eCFieldElement3).Multiply(eCFieldElement).Multiply(b3)
				.Subtract(eCFieldElement6);
			ECFieldElement eCFieldElement8 = eCFieldElement7.Subtract(eCFieldElement6).Multiply(eCFieldElement6.Add(eCFieldElement7)).Add(rawXCoord2);
			ECFieldElement y = rawXCoord.Subtract(eCFieldElement8).Multiply(eCFieldElement7).Subtract(rawYCoord);
			return new FpPoint(Curve, eCFieldElement8, y, base.IsCompressed);
		}
		case 4:
			return TwiceJacobianModified(calculateW: false).Add(b);
		default:
			return Twice().Add(b);
		}
	}

	public override ECPoint ThreeTimes()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECFieldElement rawYCoord = base.RawYCoord;
		if (rawYCoord.IsZero)
		{
			return this;
		}
		ECCurve curve = Curve;
		switch (curve.CoordinateSystem)
		{
		case 0:
		{
			ECFieldElement rawXCoord = base.RawXCoord;
			ECFieldElement eCFieldElement = Two(rawYCoord);
			ECFieldElement eCFieldElement2 = eCFieldElement.Square();
			ECFieldElement eCFieldElement3 = Three(rawXCoord.Square()).Add(Curve.A);
			ECFieldElement b = eCFieldElement3.Square();
			ECFieldElement eCFieldElement4 = Three(rawXCoord).Multiply(eCFieldElement2).Subtract(b);
			if (eCFieldElement4.IsZero)
			{
				return Curve.Infinity;
			}
			ECFieldElement eCFieldElement5 = eCFieldElement4.Multiply(eCFieldElement);
			ECFieldElement b2 = eCFieldElement5.Invert();
			ECFieldElement eCFieldElement6 = eCFieldElement4.Multiply(b2).Multiply(eCFieldElement3);
			ECFieldElement eCFieldElement7 = eCFieldElement2.Square().Multiply(b2).Subtract(eCFieldElement6);
			ECFieldElement eCFieldElement8 = eCFieldElement7.Subtract(eCFieldElement6).Multiply(eCFieldElement6.Add(eCFieldElement7)).Add(rawXCoord);
			ECFieldElement y = rawXCoord.Subtract(eCFieldElement8).Multiply(eCFieldElement7).Subtract(rawYCoord);
			return new FpPoint(Curve, eCFieldElement8, y, base.IsCompressed);
		}
		case 4:
			return TwiceJacobianModified(calculateW: false).Add(this);
		default:
			return Twice().Add(this);
		}
	}

	public override ECPoint TimesPow2(int e)
	{
		if (e < 0)
		{
			throw new ArgumentException("cannot be negative", "e");
		}
		if (e == 0 || base.IsInfinity)
		{
			return this;
		}
		if (e == 1)
		{
			return Twice();
		}
		ECCurve curve = Curve;
		ECFieldElement eCFieldElement = base.RawYCoord;
		if (eCFieldElement.IsZero)
		{
			return curve.Infinity;
		}
		int coordinateSystem = curve.CoordinateSystem;
		ECFieldElement eCFieldElement2 = curve.A;
		ECFieldElement eCFieldElement3 = base.RawXCoord;
		ECFieldElement eCFieldElement4 = ((base.RawZCoords.Length < 1) ? curve.FromBigInteger(BigInteger.One) : base.RawZCoords[0]);
		if (!eCFieldElement4.IsOne)
		{
			switch (coordinateSystem)
			{
			case 1:
			{
				ECFieldElement eCFieldElement5 = eCFieldElement4.Square();
				eCFieldElement3 = eCFieldElement3.Multiply(eCFieldElement4);
				eCFieldElement = eCFieldElement.Multiply(eCFieldElement5);
				eCFieldElement2 = CalculateJacobianModifiedW(eCFieldElement4, eCFieldElement5);
				break;
			}
			case 2:
				eCFieldElement2 = CalculateJacobianModifiedW(eCFieldElement4, null);
				break;
			case 4:
				eCFieldElement2 = GetJacobianModifiedW();
				break;
			}
		}
		for (int i = 0; i < e; i++)
		{
			if (eCFieldElement.IsZero)
			{
				return curve.Infinity;
			}
			ECFieldElement x = eCFieldElement3.Square();
			ECFieldElement eCFieldElement6 = Three(x);
			ECFieldElement eCFieldElement7 = Two(eCFieldElement);
			ECFieldElement eCFieldElement8 = eCFieldElement7.Multiply(eCFieldElement);
			ECFieldElement eCFieldElement9 = Two(eCFieldElement3.Multiply(eCFieldElement8));
			ECFieldElement x2 = eCFieldElement8.Square();
			ECFieldElement eCFieldElement10 = Two(x2);
			if (!eCFieldElement2.IsZero)
			{
				eCFieldElement6 = eCFieldElement6.Add(eCFieldElement2);
				eCFieldElement2 = Two(eCFieldElement10.Multiply(eCFieldElement2));
			}
			eCFieldElement3 = eCFieldElement6.Square().Subtract(Two(eCFieldElement9));
			eCFieldElement = eCFieldElement6.Multiply(eCFieldElement9.Subtract(eCFieldElement3)).Subtract(eCFieldElement10);
			eCFieldElement4 = (eCFieldElement4.IsOne ? eCFieldElement7 : eCFieldElement7.Multiply(eCFieldElement4));
		}
		switch (coordinateSystem)
		{
		case 0:
		{
			ECFieldElement eCFieldElement11 = eCFieldElement4.Invert();
			ECFieldElement eCFieldElement12 = eCFieldElement11.Square();
			ECFieldElement b = eCFieldElement12.Multiply(eCFieldElement11);
			return new FpPoint(curve, eCFieldElement3.Multiply(eCFieldElement12), eCFieldElement.Multiply(b), base.IsCompressed);
		}
		case 1:
			eCFieldElement3 = eCFieldElement3.Multiply(eCFieldElement4);
			eCFieldElement4 = eCFieldElement4.Multiply(eCFieldElement4.Square());
			return new FpPoint(curve, eCFieldElement3, eCFieldElement, new ECFieldElement[1] { eCFieldElement4 }, base.IsCompressed);
		case 2:
			return new FpPoint(curve, eCFieldElement3, eCFieldElement, new ECFieldElement[1] { eCFieldElement4 }, base.IsCompressed);
		case 4:
			return new FpPoint(curve, eCFieldElement3, eCFieldElement, new ECFieldElement[2] { eCFieldElement4, eCFieldElement2 }, base.IsCompressed);
		default:
			throw new InvalidOperationException("unsupported coordinate system");
		}
	}

	protected virtual ECFieldElement Two(ECFieldElement x)
	{
		return x.Add(x);
	}

	protected virtual ECFieldElement Three(ECFieldElement x)
	{
		return Two(x).Add(x);
	}

	protected virtual ECFieldElement Four(ECFieldElement x)
	{
		return Two(Two(x));
	}

	protected virtual ECFieldElement Eight(ECFieldElement x)
	{
		return Four(Two(x));
	}

	protected virtual ECFieldElement DoubleProductFromSquares(ECFieldElement a, ECFieldElement b, ECFieldElement aSquared, ECFieldElement bSquared)
	{
		return a.Add(b).Square().Subtract(aSquared)
			.Subtract(bSquared);
	}

	public override ECPoint Negate()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		if (curve.CoordinateSystem != 0)
		{
			return new FpPoint(curve, base.RawXCoord, base.RawYCoord.Negate(), base.RawZCoords, base.IsCompressed);
		}
		return new FpPoint(curve, base.RawXCoord, base.RawYCoord.Negate(), base.IsCompressed);
	}

	protected virtual ECFieldElement CalculateJacobianModifiedW(ECFieldElement Z, ECFieldElement ZSquared)
	{
		ECFieldElement a = Curve.A;
		if (a.IsZero || Z.IsOne)
		{
			return a;
		}
		if (ZSquared == null)
		{
			ZSquared = Z.Square();
		}
		ECFieldElement eCFieldElement = ZSquared.Square();
		ECFieldElement eCFieldElement2 = a.Negate();
		if (eCFieldElement2.BitLength < a.BitLength)
		{
			return eCFieldElement.Multiply(eCFieldElement2).Negate();
		}
		return eCFieldElement.Multiply(a);
	}

	protected virtual ECFieldElement GetJacobianModifiedW()
	{
		ECFieldElement[] rawZCoords = base.RawZCoords;
		ECFieldElement eCFieldElement = rawZCoords[1];
		if (eCFieldElement == null)
		{
			eCFieldElement = (rawZCoords[1] = CalculateJacobianModifiedW(rawZCoords[0], null));
		}
		return eCFieldElement;
	}

	protected virtual FpPoint TwiceJacobianModified(bool calculateW)
	{
		ECFieldElement rawXCoord = base.RawXCoord;
		ECFieldElement rawYCoord = base.RawYCoord;
		ECFieldElement eCFieldElement = base.RawZCoords[0];
		ECFieldElement jacobianModifiedW = GetJacobianModifiedW();
		ECFieldElement x = rawXCoord.Square();
		ECFieldElement eCFieldElement2 = Three(x).Add(jacobianModifiedW);
		ECFieldElement eCFieldElement3 = Two(rawYCoord);
		ECFieldElement eCFieldElement4 = eCFieldElement3.Multiply(rawYCoord);
		ECFieldElement eCFieldElement5 = Two(rawXCoord.Multiply(eCFieldElement4));
		ECFieldElement eCFieldElement6 = eCFieldElement2.Square().Subtract(Two(eCFieldElement5));
		ECFieldElement x2 = eCFieldElement4.Square();
		ECFieldElement eCFieldElement7 = Two(x2);
		ECFieldElement y = eCFieldElement2.Multiply(eCFieldElement5.Subtract(eCFieldElement6)).Subtract(eCFieldElement7);
		ECFieldElement eCFieldElement8 = (calculateW ? Two(eCFieldElement7.Multiply(jacobianModifiedW)) : null);
		ECFieldElement eCFieldElement9 = (eCFieldElement.IsOne ? eCFieldElement3 : eCFieldElement3.Multiply(eCFieldElement));
		return new FpPoint(Curve, eCFieldElement6, y, new ECFieldElement[2] { eCFieldElement9, eCFieldElement8 }, base.IsCompressed);
	}
}
