using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP192K1Point : AbstractFpPoint
{
	public SecP192K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	public SecP192K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal SecP192K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new SecP192K1Point(null, AffineXCoord, AffineYCoord);
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
		SecP192K1FieldElement secP192K1FieldElement = (SecP192K1FieldElement)base.RawXCoord;
		SecP192K1FieldElement secP192K1FieldElement2 = (SecP192K1FieldElement)base.RawYCoord;
		SecP192K1FieldElement secP192K1FieldElement3 = (SecP192K1FieldElement)b.RawXCoord;
		SecP192K1FieldElement secP192K1FieldElement4 = (SecP192K1FieldElement)b.RawYCoord;
		SecP192K1FieldElement secP192K1FieldElement5 = (SecP192K1FieldElement)base.RawZCoords[0];
		SecP192K1FieldElement secP192K1FieldElement6 = (SecP192K1FieldElement)b.RawZCoords[0];
		uint[] array = Nat192.CreateExt();
		uint[] array2 = Nat192.Create();
		uint[] array3 = Nat192.Create();
		uint[] array4 = Nat192.Create();
		bool isOne = secP192K1FieldElement5.IsOne;
		uint[] array5;
		uint[] array6;
		if (isOne)
		{
			array5 = secP192K1FieldElement3.x;
			array6 = secP192K1FieldElement4.x;
		}
		else
		{
			array6 = array3;
			SecP192K1Field.Square(secP192K1FieldElement5.x, array6);
			array5 = array2;
			SecP192K1Field.Multiply(array6, secP192K1FieldElement3.x, array5);
			SecP192K1Field.Multiply(array6, secP192K1FieldElement5.x, array6);
			SecP192K1Field.Multiply(array6, secP192K1FieldElement4.x, array6);
		}
		bool isOne2 = secP192K1FieldElement6.IsOne;
		uint[] array7;
		uint[] array8;
		if (isOne2)
		{
			array7 = secP192K1FieldElement.x;
			array8 = secP192K1FieldElement2.x;
		}
		else
		{
			array8 = array4;
			SecP192K1Field.Square(secP192K1FieldElement6.x, array8);
			array7 = array;
			SecP192K1Field.Multiply(array8, secP192K1FieldElement.x, array7);
			SecP192K1Field.Multiply(array8, secP192K1FieldElement6.x, array8);
			SecP192K1Field.Multiply(array8, secP192K1FieldElement2.x, array8);
		}
		uint[] array9 = Nat192.Create();
		SecP192K1Field.Subtract(array7, array5, array9);
		uint[] array10 = array2;
		SecP192K1Field.Subtract(array8, array6, array10);
		if (Nat192.IsZero(array9))
		{
			if (Nat192.IsZero(array10))
			{
				return Twice();
			}
			return curve.Infinity;
		}
		uint[] array11 = array3;
		SecP192K1Field.Square(array9, array11);
		uint[] array12 = Nat192.Create();
		SecP192K1Field.Multiply(array11, array9, array12);
		uint[] array13 = array3;
		SecP192K1Field.Multiply(array11, array7, array13);
		SecP192K1Field.Negate(array12, array12);
		Nat192.Mul(array8, array12, array);
		uint x = Nat192.AddBothTo(array13, array13, array12);
		SecP192K1Field.Reduce32(x, array12);
		SecP192K1FieldElement secP192K1FieldElement7 = new SecP192K1FieldElement(array4);
		SecP192K1Field.Square(array10, secP192K1FieldElement7.x);
		SecP192K1Field.Subtract(secP192K1FieldElement7.x, array12, secP192K1FieldElement7.x);
		SecP192K1FieldElement secP192K1FieldElement8 = new SecP192K1FieldElement(array12);
		SecP192K1Field.Subtract(array13, secP192K1FieldElement7.x, secP192K1FieldElement8.x);
		SecP192K1Field.MultiplyAddToExt(secP192K1FieldElement8.x, array10, array);
		SecP192K1Field.Reduce(array, secP192K1FieldElement8.x);
		SecP192K1FieldElement secP192K1FieldElement9 = new SecP192K1FieldElement(array9);
		if (!isOne)
		{
			SecP192K1Field.Multiply(secP192K1FieldElement9.x, secP192K1FieldElement5.x, secP192K1FieldElement9.x);
		}
		if (!isOne2)
		{
			SecP192K1Field.Multiply(secP192K1FieldElement9.x, secP192K1FieldElement6.x, secP192K1FieldElement9.x);
		}
		ECFieldElement[] zs = new ECFieldElement[1] { secP192K1FieldElement9 };
		return new SecP192K1Point(curve, secP192K1FieldElement7, secP192K1FieldElement8, zs, base.IsCompressed);
	}

	public override ECPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		SecP192K1FieldElement secP192K1FieldElement = (SecP192K1FieldElement)base.RawYCoord;
		if (secP192K1FieldElement.IsZero)
		{
			return curve.Infinity;
		}
		SecP192K1FieldElement secP192K1FieldElement2 = (SecP192K1FieldElement)base.RawXCoord;
		SecP192K1FieldElement secP192K1FieldElement3 = (SecP192K1FieldElement)base.RawZCoords[0];
		uint[] array = Nat192.Create();
		SecP192K1Field.Square(secP192K1FieldElement.x, array);
		uint[] array2 = Nat192.Create();
		SecP192K1Field.Square(array, array2);
		uint[] array3 = Nat192.Create();
		SecP192K1Field.Square(secP192K1FieldElement2.x, array3);
		uint x = Nat192.AddBothTo(array3, array3, array3);
		SecP192K1Field.Reduce32(x, array3);
		uint[] array4 = array;
		SecP192K1Field.Multiply(array, secP192K1FieldElement2.x, array4);
		x = Nat.ShiftUpBits(6, array4, 2, 0u);
		SecP192K1Field.Reduce32(x, array4);
		uint[] array5 = Nat192.Create();
		x = Nat.ShiftUpBits(6, array2, 3, 0u, array5);
		SecP192K1Field.Reduce32(x, array5);
		SecP192K1FieldElement secP192K1FieldElement4 = new SecP192K1FieldElement(array2);
		SecP192K1Field.Square(array3, secP192K1FieldElement4.x);
		SecP192K1Field.Subtract(secP192K1FieldElement4.x, array4, secP192K1FieldElement4.x);
		SecP192K1Field.Subtract(secP192K1FieldElement4.x, array4, secP192K1FieldElement4.x);
		SecP192K1FieldElement secP192K1FieldElement5 = new SecP192K1FieldElement(array4);
		SecP192K1Field.Subtract(array4, secP192K1FieldElement4.x, secP192K1FieldElement5.x);
		SecP192K1Field.Multiply(secP192K1FieldElement5.x, array3, secP192K1FieldElement5.x);
		SecP192K1Field.Subtract(secP192K1FieldElement5.x, array5, secP192K1FieldElement5.x);
		SecP192K1FieldElement secP192K1FieldElement6 = new SecP192K1FieldElement(array3);
		SecP192K1Field.Twice(secP192K1FieldElement.x, secP192K1FieldElement6.x);
		if (!secP192K1FieldElement3.IsOne)
		{
			SecP192K1Field.Multiply(secP192K1FieldElement6.x, secP192K1FieldElement3.x, secP192K1FieldElement6.x);
		}
		return new SecP192K1Point(curve, secP192K1FieldElement4, secP192K1FieldElement5, new ECFieldElement[1] { secP192K1FieldElement6 }, base.IsCompressed);
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
		return new SecP192K1Point(Curve, base.RawXCoord, base.RawYCoord.Negate(), base.RawZCoords, base.IsCompressed);
	}
}
