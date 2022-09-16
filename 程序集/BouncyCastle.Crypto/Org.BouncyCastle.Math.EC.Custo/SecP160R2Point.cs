using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP160R2Point : AbstractFpPoint
{
	public SecP160R2Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	public SecP160R2Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal SecP160R2Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new SecP160R2Point(null, AffineXCoord, AffineYCoord);
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
		SecP160R2FieldElement secP160R2FieldElement = (SecP160R2FieldElement)base.RawXCoord;
		SecP160R2FieldElement secP160R2FieldElement2 = (SecP160R2FieldElement)base.RawYCoord;
		SecP160R2FieldElement secP160R2FieldElement3 = (SecP160R2FieldElement)b.RawXCoord;
		SecP160R2FieldElement secP160R2FieldElement4 = (SecP160R2FieldElement)b.RawYCoord;
		SecP160R2FieldElement secP160R2FieldElement5 = (SecP160R2FieldElement)base.RawZCoords[0];
		SecP160R2FieldElement secP160R2FieldElement6 = (SecP160R2FieldElement)b.RawZCoords[0];
		uint[] array = Nat160.CreateExt();
		uint[] array2 = Nat160.Create();
		uint[] array3 = Nat160.Create();
		uint[] array4 = Nat160.Create();
		bool isOne = secP160R2FieldElement5.IsOne;
		uint[] array5;
		uint[] array6;
		if (isOne)
		{
			array5 = secP160R2FieldElement3.x;
			array6 = secP160R2FieldElement4.x;
		}
		else
		{
			array6 = array3;
			SecP160R2Field.Square(secP160R2FieldElement5.x, array6);
			array5 = array2;
			SecP160R2Field.Multiply(array6, secP160R2FieldElement3.x, array5);
			SecP160R2Field.Multiply(array6, secP160R2FieldElement5.x, array6);
			SecP160R2Field.Multiply(array6, secP160R2FieldElement4.x, array6);
		}
		bool isOne2 = secP160R2FieldElement6.IsOne;
		uint[] array7;
		uint[] array8;
		if (isOne2)
		{
			array7 = secP160R2FieldElement.x;
			array8 = secP160R2FieldElement2.x;
		}
		else
		{
			array8 = array4;
			SecP160R2Field.Square(secP160R2FieldElement6.x, array8);
			array7 = array;
			SecP160R2Field.Multiply(array8, secP160R2FieldElement.x, array7);
			SecP160R2Field.Multiply(array8, secP160R2FieldElement6.x, array8);
			SecP160R2Field.Multiply(array8, secP160R2FieldElement2.x, array8);
		}
		uint[] array9 = Nat160.Create();
		SecP160R2Field.Subtract(array7, array5, array9);
		uint[] array10 = array2;
		SecP160R2Field.Subtract(array8, array6, array10);
		if (Nat160.IsZero(array9))
		{
			if (Nat160.IsZero(array10))
			{
				return Twice();
			}
			return curve.Infinity;
		}
		uint[] array11 = array3;
		SecP160R2Field.Square(array9, array11);
		uint[] array12 = Nat160.Create();
		SecP160R2Field.Multiply(array11, array9, array12);
		uint[] array13 = array3;
		SecP160R2Field.Multiply(array11, array7, array13);
		SecP160R2Field.Negate(array12, array12);
		Nat160.Mul(array8, array12, array);
		uint x = Nat160.AddBothTo(array13, array13, array12);
		SecP160R2Field.Reduce32(x, array12);
		SecP160R2FieldElement secP160R2FieldElement7 = new SecP160R2FieldElement(array4);
		SecP160R2Field.Square(array10, secP160R2FieldElement7.x);
		SecP160R2Field.Subtract(secP160R2FieldElement7.x, array12, secP160R2FieldElement7.x);
		SecP160R2FieldElement secP160R2FieldElement8 = new SecP160R2FieldElement(array12);
		SecP160R2Field.Subtract(array13, secP160R2FieldElement7.x, secP160R2FieldElement8.x);
		SecP160R2Field.MultiplyAddToExt(secP160R2FieldElement8.x, array10, array);
		SecP160R2Field.Reduce(array, secP160R2FieldElement8.x);
		SecP160R2FieldElement secP160R2FieldElement9 = new SecP160R2FieldElement(array9);
		if (!isOne)
		{
			SecP160R2Field.Multiply(secP160R2FieldElement9.x, secP160R2FieldElement5.x, secP160R2FieldElement9.x);
		}
		if (!isOne2)
		{
			SecP160R2Field.Multiply(secP160R2FieldElement9.x, secP160R2FieldElement6.x, secP160R2FieldElement9.x);
		}
		ECFieldElement[] zs = new ECFieldElement[1] { secP160R2FieldElement9 };
		return new SecP160R2Point(curve, secP160R2FieldElement7, secP160R2FieldElement8, zs, base.IsCompressed);
	}

	public override ECPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		SecP160R2FieldElement secP160R2FieldElement = (SecP160R2FieldElement)base.RawYCoord;
		if (secP160R2FieldElement.IsZero)
		{
			return curve.Infinity;
		}
		SecP160R2FieldElement secP160R2FieldElement2 = (SecP160R2FieldElement)base.RawXCoord;
		SecP160R2FieldElement secP160R2FieldElement3 = (SecP160R2FieldElement)base.RawZCoords[0];
		uint[] array = Nat160.Create();
		uint[] array2 = Nat160.Create();
		uint[] array3 = Nat160.Create();
		SecP160R2Field.Square(secP160R2FieldElement.x, array3);
		uint[] array4 = Nat160.Create();
		SecP160R2Field.Square(array3, array4);
		bool isOne = secP160R2FieldElement3.IsOne;
		uint[] array5 = secP160R2FieldElement3.x;
		if (!isOne)
		{
			array5 = array2;
			SecP160R2Field.Square(secP160R2FieldElement3.x, array5);
		}
		SecP160R2Field.Subtract(secP160R2FieldElement2.x, array5, array);
		uint[] array6 = array2;
		SecP160R2Field.Add(secP160R2FieldElement2.x, array5, array6);
		SecP160R2Field.Multiply(array6, array, array6);
		uint x = Nat160.AddBothTo(array6, array6, array6);
		SecP160R2Field.Reduce32(x, array6);
		uint[] array7 = array3;
		SecP160R2Field.Multiply(array3, secP160R2FieldElement2.x, array7);
		x = Nat.ShiftUpBits(5, array7, 2, 0u);
		SecP160R2Field.Reduce32(x, array7);
		x = Nat.ShiftUpBits(5, array4, 3, 0u, array);
		SecP160R2Field.Reduce32(x, array);
		SecP160R2FieldElement secP160R2FieldElement4 = new SecP160R2FieldElement(array4);
		SecP160R2Field.Square(array6, secP160R2FieldElement4.x);
		SecP160R2Field.Subtract(secP160R2FieldElement4.x, array7, secP160R2FieldElement4.x);
		SecP160R2Field.Subtract(secP160R2FieldElement4.x, array7, secP160R2FieldElement4.x);
		SecP160R2FieldElement secP160R2FieldElement5 = new SecP160R2FieldElement(array7);
		SecP160R2Field.Subtract(array7, secP160R2FieldElement4.x, secP160R2FieldElement5.x);
		SecP160R2Field.Multiply(secP160R2FieldElement5.x, array6, secP160R2FieldElement5.x);
		SecP160R2Field.Subtract(secP160R2FieldElement5.x, array, secP160R2FieldElement5.x);
		SecP160R2FieldElement secP160R2FieldElement6 = new SecP160R2FieldElement(array6);
		SecP160R2Field.Twice(secP160R2FieldElement.x, secP160R2FieldElement6.x);
		if (!isOne)
		{
			SecP160R2Field.Multiply(secP160R2FieldElement6.x, secP160R2FieldElement3.x, secP160R2FieldElement6.x);
		}
		return new SecP160R2Point(curve, secP160R2FieldElement4, secP160R2FieldElement5, new ECFieldElement[1] { secP160R2FieldElement6 }, base.IsCompressed);
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
		return Twice().Add(b);
	}

	public override ECPoint ThreeTimes()
	{
		if (base.IsInfinity || base.RawYCoord.IsZero)
		{
			return this;
		}
		return Twice().Add(this);
	}

	public override ECPoint Negate()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		return new SecP160R2Point(Curve, base.RawXCoord, base.RawYCoord.Negate(), base.RawZCoords, base.IsCompressed);
	}
}
