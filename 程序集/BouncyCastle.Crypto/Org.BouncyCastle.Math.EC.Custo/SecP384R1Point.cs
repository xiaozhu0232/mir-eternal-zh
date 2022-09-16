using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP384R1Point : AbstractFpPoint
{
	public SecP384R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	public SecP384R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal SecP384R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new SecP384R1Point(null, AffineXCoord, AffineYCoord);
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
		SecP384R1FieldElement secP384R1FieldElement = (SecP384R1FieldElement)base.RawXCoord;
		SecP384R1FieldElement secP384R1FieldElement2 = (SecP384R1FieldElement)base.RawYCoord;
		SecP384R1FieldElement secP384R1FieldElement3 = (SecP384R1FieldElement)b.RawXCoord;
		SecP384R1FieldElement secP384R1FieldElement4 = (SecP384R1FieldElement)b.RawYCoord;
		SecP384R1FieldElement secP384R1FieldElement5 = (SecP384R1FieldElement)base.RawZCoords[0];
		SecP384R1FieldElement secP384R1FieldElement6 = (SecP384R1FieldElement)b.RawZCoords[0];
		uint[] array = Nat.Create(24);
		uint[] array2 = Nat.Create(24);
		uint[] array3 = Nat.Create(12);
		uint[] array4 = Nat.Create(12);
		bool isOne = secP384R1FieldElement5.IsOne;
		uint[] array5;
		uint[] array6;
		if (isOne)
		{
			array5 = secP384R1FieldElement3.x;
			array6 = secP384R1FieldElement4.x;
		}
		else
		{
			array6 = array3;
			SecP384R1Field.Square(secP384R1FieldElement5.x, array6);
			array5 = array2;
			SecP384R1Field.Multiply(array6, secP384R1FieldElement3.x, array5);
			SecP384R1Field.Multiply(array6, secP384R1FieldElement5.x, array6);
			SecP384R1Field.Multiply(array6, secP384R1FieldElement4.x, array6);
		}
		bool isOne2 = secP384R1FieldElement6.IsOne;
		uint[] array7;
		uint[] array8;
		if (isOne2)
		{
			array7 = secP384R1FieldElement.x;
			array8 = secP384R1FieldElement2.x;
		}
		else
		{
			array8 = array4;
			SecP384R1Field.Square(secP384R1FieldElement6.x, array8);
			array7 = array;
			SecP384R1Field.Multiply(array8, secP384R1FieldElement.x, array7);
			SecP384R1Field.Multiply(array8, secP384R1FieldElement6.x, array8);
			SecP384R1Field.Multiply(array8, secP384R1FieldElement2.x, array8);
		}
		uint[] array9 = Nat.Create(12);
		SecP384R1Field.Subtract(array7, array5, array9);
		uint[] array10 = Nat.Create(12);
		SecP384R1Field.Subtract(array8, array6, array10);
		if (Nat.IsZero(12, array9))
		{
			if (Nat.IsZero(12, array10))
			{
				return Twice();
			}
			return curve.Infinity;
		}
		uint[] array11 = array3;
		SecP384R1Field.Square(array9, array11);
		uint[] array12 = Nat.Create(12);
		SecP384R1Field.Multiply(array11, array9, array12);
		uint[] array13 = array3;
		SecP384R1Field.Multiply(array11, array7, array13);
		SecP384R1Field.Negate(array12, array12);
		Nat384.Mul(array8, array12, array);
		uint x = Nat.AddBothTo(12, array13, array13, array12);
		SecP384R1Field.Reduce32(x, array12);
		SecP384R1FieldElement secP384R1FieldElement7 = new SecP384R1FieldElement(array4);
		SecP384R1Field.Square(array10, secP384R1FieldElement7.x);
		SecP384R1Field.Subtract(secP384R1FieldElement7.x, array12, secP384R1FieldElement7.x);
		SecP384R1FieldElement secP384R1FieldElement8 = new SecP384R1FieldElement(array12);
		SecP384R1Field.Subtract(array13, secP384R1FieldElement7.x, secP384R1FieldElement8.x);
		Nat384.Mul(secP384R1FieldElement8.x, array10, array2);
		SecP384R1Field.AddExt(array, array2, array);
		SecP384R1Field.Reduce(array, secP384R1FieldElement8.x);
		SecP384R1FieldElement secP384R1FieldElement9 = new SecP384R1FieldElement(array9);
		if (!isOne)
		{
			SecP384R1Field.Multiply(secP384R1FieldElement9.x, secP384R1FieldElement5.x, secP384R1FieldElement9.x);
		}
		if (!isOne2)
		{
			SecP384R1Field.Multiply(secP384R1FieldElement9.x, secP384R1FieldElement6.x, secP384R1FieldElement9.x);
		}
		ECFieldElement[] zs = new ECFieldElement[1] { secP384R1FieldElement9 };
		return new SecP384R1Point(curve, secP384R1FieldElement7, secP384R1FieldElement8, zs, base.IsCompressed);
	}

	public override ECPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		SecP384R1FieldElement secP384R1FieldElement = (SecP384R1FieldElement)base.RawYCoord;
		if (secP384R1FieldElement.IsZero)
		{
			return curve.Infinity;
		}
		SecP384R1FieldElement secP384R1FieldElement2 = (SecP384R1FieldElement)base.RawXCoord;
		SecP384R1FieldElement secP384R1FieldElement3 = (SecP384R1FieldElement)base.RawZCoords[0];
		uint[] array = Nat.Create(12);
		uint[] array2 = Nat.Create(12);
		uint[] array3 = Nat.Create(12);
		SecP384R1Field.Square(secP384R1FieldElement.x, array3);
		uint[] array4 = Nat.Create(12);
		SecP384R1Field.Square(array3, array4);
		bool isOne = secP384R1FieldElement3.IsOne;
		uint[] array5 = secP384R1FieldElement3.x;
		if (!isOne)
		{
			array5 = array2;
			SecP384R1Field.Square(secP384R1FieldElement3.x, array5);
		}
		SecP384R1Field.Subtract(secP384R1FieldElement2.x, array5, array);
		uint[] array6 = array2;
		SecP384R1Field.Add(secP384R1FieldElement2.x, array5, array6);
		SecP384R1Field.Multiply(array6, array, array6);
		uint x = Nat.AddBothTo(12, array6, array6, array6);
		SecP384R1Field.Reduce32(x, array6);
		uint[] array7 = array3;
		SecP384R1Field.Multiply(array3, secP384R1FieldElement2.x, array7);
		x = Nat.ShiftUpBits(12, array7, 2, 0u);
		SecP384R1Field.Reduce32(x, array7);
		x = Nat.ShiftUpBits(12, array4, 3, 0u, array);
		SecP384R1Field.Reduce32(x, array);
		SecP384R1FieldElement secP384R1FieldElement4 = new SecP384R1FieldElement(array4);
		SecP384R1Field.Square(array6, secP384R1FieldElement4.x);
		SecP384R1Field.Subtract(secP384R1FieldElement4.x, array7, secP384R1FieldElement4.x);
		SecP384R1Field.Subtract(secP384R1FieldElement4.x, array7, secP384R1FieldElement4.x);
		SecP384R1FieldElement secP384R1FieldElement5 = new SecP384R1FieldElement(array7);
		SecP384R1Field.Subtract(array7, secP384R1FieldElement4.x, secP384R1FieldElement5.x);
		SecP384R1Field.Multiply(secP384R1FieldElement5.x, array6, secP384R1FieldElement5.x);
		SecP384R1Field.Subtract(secP384R1FieldElement5.x, array, secP384R1FieldElement5.x);
		SecP384R1FieldElement secP384R1FieldElement6 = new SecP384R1FieldElement(array6);
		SecP384R1Field.Twice(secP384R1FieldElement.x, secP384R1FieldElement6.x);
		if (!isOne)
		{
			SecP384R1Field.Multiply(secP384R1FieldElement6.x, secP384R1FieldElement3.x, secP384R1FieldElement6.x);
		}
		return new SecP384R1Point(curve, secP384R1FieldElement4, secP384R1FieldElement5, new ECFieldElement[1] { secP384R1FieldElement6 }, base.IsCompressed);
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
		return new SecP384R1Point(Curve, base.RawXCoord, base.RawYCoord.Negate(), base.RawZCoords, base.IsCompressed);
	}
}
