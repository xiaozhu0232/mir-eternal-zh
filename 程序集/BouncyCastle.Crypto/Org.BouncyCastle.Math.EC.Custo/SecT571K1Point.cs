using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT571K1Point : AbstractF2mPoint
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

	public SecT571K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	public SecT571K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal SecT571K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new SecT571K1Point(null, AffineXCoord, AffineYCoord);
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
		SecT571FieldElement secT571FieldElement = (SecT571FieldElement)base.RawXCoord;
		SecT571FieldElement secT571FieldElement2 = (SecT571FieldElement)b.RawXCoord;
		if (secT571FieldElement.IsZero)
		{
			if (secT571FieldElement2.IsZero)
			{
				return curve.Infinity;
			}
			return b.Add(this);
		}
		SecT571FieldElement secT571FieldElement3 = (SecT571FieldElement)base.RawYCoord;
		SecT571FieldElement secT571FieldElement4 = (SecT571FieldElement)base.RawZCoords[0];
		SecT571FieldElement secT571FieldElement5 = (SecT571FieldElement)b.RawYCoord;
		SecT571FieldElement secT571FieldElement6 = (SecT571FieldElement)b.RawZCoords[0];
		ulong[] array = Nat576.Create64();
		ulong[] array2 = Nat576.Create64();
		ulong[] array3 = Nat576.Create64();
		ulong[] array4 = Nat576.Create64();
		ulong[] array5 = (secT571FieldElement4.IsOne ? null : SecT571Field.PrecompMultiplicand(secT571FieldElement4.x));
		ulong[] array6;
		ulong[] y;
		if (array5 == null)
		{
			array6 = secT571FieldElement2.x;
			y = secT571FieldElement5.x;
		}
		else
		{
			SecT571Field.MultiplyPrecomp(secT571FieldElement2.x, array5, array6 = array2);
			SecT571Field.MultiplyPrecomp(secT571FieldElement5.x, array5, y = array4);
		}
		ulong[] array7 = (secT571FieldElement6.IsOne ? null : SecT571Field.PrecompMultiplicand(secT571FieldElement6.x));
		ulong[] x;
		ulong[] x2;
		if (array7 == null)
		{
			x = secT571FieldElement.x;
			x2 = secT571FieldElement3.x;
		}
		else
		{
			SecT571Field.MultiplyPrecomp(secT571FieldElement.x, array7, x = array);
			SecT571Field.MultiplyPrecomp(secT571FieldElement3.x, array7, x2 = array3);
		}
		ulong[] array8 = array3;
		SecT571Field.Add(x2, y, array8);
		ulong[] array9 = array4;
		SecT571Field.Add(x, array6, array9);
		if (Nat576.IsZero64(array9))
		{
			if (Nat576.IsZero64(array8))
			{
				return Twice();
			}
			return curve.Infinity;
		}
		SecT571FieldElement secT571FieldElement7;
		SecT571FieldElement secT571FieldElement8;
		SecT571FieldElement secT571FieldElement9;
		if (secT571FieldElement2.IsZero)
		{
			ECPoint eCPoint = Normalize();
			secT571FieldElement = (SecT571FieldElement)eCPoint.XCoord;
			ECFieldElement yCoord = eCPoint.YCoord;
			ECFieldElement b2 = secT571FieldElement5;
			ECFieldElement eCFieldElement = yCoord.Add(b2).Divide(secT571FieldElement);
			secT571FieldElement7 = (SecT571FieldElement)eCFieldElement.Square().Add(eCFieldElement).Add(secT571FieldElement);
			if (secT571FieldElement7.IsZero)
			{
				return new SecT571K1Point(curve, secT571FieldElement7, curve.B, base.IsCompressed);
			}
			ECFieldElement eCFieldElement2 = eCFieldElement.Multiply(secT571FieldElement.Add(secT571FieldElement7)).Add(secT571FieldElement7).Add(yCoord);
			secT571FieldElement8 = (SecT571FieldElement)eCFieldElement2.Divide(secT571FieldElement7).Add(secT571FieldElement7);
			secT571FieldElement9 = (SecT571FieldElement)curve.FromBigInteger(BigInteger.One);
		}
		else
		{
			SecT571Field.Square(array9, array9);
			ulong[] precomp = SecT571Field.PrecompMultiplicand(array8);
			ulong[] array10 = array;
			ulong[] array11 = array2;
			SecT571Field.MultiplyPrecomp(x, precomp, array10);
			SecT571Field.MultiplyPrecomp(array6, precomp, array11);
			secT571FieldElement7 = new SecT571FieldElement(array);
			SecT571Field.Multiply(array10, array11, secT571FieldElement7.x);
			if (secT571FieldElement7.IsZero)
			{
				return new SecT571K1Point(curve, secT571FieldElement7, curve.B, base.IsCompressed);
			}
			secT571FieldElement9 = new SecT571FieldElement(array3);
			SecT571Field.MultiplyPrecomp(array9, precomp, secT571FieldElement9.x);
			if (array7 != null)
			{
				SecT571Field.MultiplyPrecomp(secT571FieldElement9.x, array7, secT571FieldElement9.x);
			}
			ulong[] array12 = Nat576.CreateExt64();
			SecT571Field.Add(array11, array9, array4);
			SecT571Field.SquareAddToExt(array4, array12);
			SecT571Field.Add(secT571FieldElement3.x, secT571FieldElement4.x, array4);
			SecT571Field.MultiplyAddToExt(array4, secT571FieldElement9.x, array12);
			secT571FieldElement8 = new SecT571FieldElement(array4);
			SecT571Field.Reduce(array12, secT571FieldElement8.x);
			if (array5 != null)
			{
				SecT571Field.MultiplyPrecomp(secT571FieldElement9.x, array5, secT571FieldElement9.x);
			}
		}
		return new SecT571K1Point(curve, secT571FieldElement7, secT571FieldElement8, new ECFieldElement[1] { secT571FieldElement9 }, base.IsCompressed);
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
		ECFieldElement eCFieldElement2 = (isOne ? eCFieldElement : eCFieldElement.Square());
		ECFieldElement eCFieldElement3 = ((!isOne) ? rawYCoord.Add(eCFieldElement).Multiply(rawYCoord) : rawYCoord.Square().Add(rawYCoord));
		if (eCFieldElement3.IsZero)
		{
			return new SecT571K1Point(curve, eCFieldElement3, curve.B, base.IsCompressed);
		}
		ECFieldElement eCFieldElement4 = eCFieldElement3.Square();
		ECFieldElement eCFieldElement5 = (isOne ? eCFieldElement3 : eCFieldElement3.Multiply(eCFieldElement2));
		ECFieldElement eCFieldElement6 = rawYCoord.Add(rawXCoord).Square();
		ECFieldElement b = (isOne ? eCFieldElement : eCFieldElement2.Square());
		ECFieldElement y = eCFieldElement6.Add(eCFieldElement3).Add(eCFieldElement2).Multiply(eCFieldElement6)
			.Add(b)
			.Add(eCFieldElement4)
			.Add(eCFieldElement5);
		return new SecT571K1Point(curve, eCFieldElement4, y, new ECFieldElement[1] { eCFieldElement5 }, base.IsCompressed);
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
		ECFieldElement eCFieldElement3 = rawYCoord.Square();
		ECFieldElement eCFieldElement4 = eCFieldElement2.Square();
		ECFieldElement b2 = rawYCoord.Multiply(eCFieldElement2);
		ECFieldElement b3 = eCFieldElement3.Add(b2);
		ECFieldElement eCFieldElement5 = rawYCoord2.AddOne();
		ECFieldElement eCFieldElement6 = eCFieldElement5.Multiply(eCFieldElement4).Add(eCFieldElement3).MultiplyPlusProduct(b3, x, eCFieldElement4);
		ECFieldElement eCFieldElement7 = rawXCoord2.Multiply(eCFieldElement4);
		ECFieldElement eCFieldElement8 = eCFieldElement7.Add(b3).Square();
		if (eCFieldElement8.IsZero)
		{
			if (eCFieldElement6.IsZero)
			{
				return b.Twice();
			}
			return curve.Infinity;
		}
		if (eCFieldElement6.IsZero)
		{
			return new SecT571K1Point(curve, eCFieldElement6, curve.B, base.IsCompressed);
		}
		ECFieldElement x2 = eCFieldElement6.Square().Multiply(eCFieldElement7);
		ECFieldElement eCFieldElement9 = eCFieldElement6.Multiply(eCFieldElement8).Multiply(eCFieldElement4);
		ECFieldElement y = eCFieldElement6.Add(eCFieldElement8).Square().MultiplyPlusProduct(b3, eCFieldElement5, eCFieldElement9);
		return new SecT571K1Point(curve, x2, y, new ECFieldElement[1] { eCFieldElement9 }, base.IsCompressed);
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
		return new SecT571K1Point(Curve, rawXCoord, rawYCoord.Add(eCFieldElement), new ECFieldElement[1] { eCFieldElement }, base.IsCompressed);
	}
}
