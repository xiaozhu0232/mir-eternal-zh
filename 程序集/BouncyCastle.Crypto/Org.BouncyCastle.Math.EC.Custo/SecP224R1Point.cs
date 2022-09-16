using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP224R1Point : AbstractFpPoint
{
	public SecP224R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	public SecP224R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal SecP224R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new SecP224R1Point(null, AffineXCoord, AffineYCoord);
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
		SecP224R1FieldElement secP224R1FieldElement = (SecP224R1FieldElement)base.RawXCoord;
		SecP224R1FieldElement secP224R1FieldElement2 = (SecP224R1FieldElement)base.RawYCoord;
		SecP224R1FieldElement secP224R1FieldElement3 = (SecP224R1FieldElement)b.RawXCoord;
		SecP224R1FieldElement secP224R1FieldElement4 = (SecP224R1FieldElement)b.RawYCoord;
		SecP224R1FieldElement secP224R1FieldElement5 = (SecP224R1FieldElement)base.RawZCoords[0];
		SecP224R1FieldElement secP224R1FieldElement6 = (SecP224R1FieldElement)b.RawZCoords[0];
		uint[] array = Nat224.CreateExt();
		uint[] array2 = Nat224.Create();
		uint[] array3 = Nat224.Create();
		uint[] array4 = Nat224.Create();
		bool isOne = secP224R1FieldElement5.IsOne;
		uint[] array5;
		uint[] array6;
		if (isOne)
		{
			array5 = secP224R1FieldElement3.x;
			array6 = secP224R1FieldElement4.x;
		}
		else
		{
			array6 = array3;
			SecP224R1Field.Square(secP224R1FieldElement5.x, array6);
			array5 = array2;
			SecP224R1Field.Multiply(array6, secP224R1FieldElement3.x, array5);
			SecP224R1Field.Multiply(array6, secP224R1FieldElement5.x, array6);
			SecP224R1Field.Multiply(array6, secP224R1FieldElement4.x, array6);
		}
		bool isOne2 = secP224R1FieldElement6.IsOne;
		uint[] array7;
		uint[] array8;
		if (isOne2)
		{
			array7 = secP224R1FieldElement.x;
			array8 = secP224R1FieldElement2.x;
		}
		else
		{
			array8 = array4;
			SecP224R1Field.Square(secP224R1FieldElement6.x, array8);
			array7 = array;
			SecP224R1Field.Multiply(array8, secP224R1FieldElement.x, array7);
			SecP224R1Field.Multiply(array8, secP224R1FieldElement6.x, array8);
			SecP224R1Field.Multiply(array8, secP224R1FieldElement2.x, array8);
		}
		uint[] array9 = Nat224.Create();
		SecP224R1Field.Subtract(array7, array5, array9);
		uint[] array10 = array2;
		SecP224R1Field.Subtract(array8, array6, array10);
		if (Nat224.IsZero(array9))
		{
			if (Nat224.IsZero(array10))
			{
				return Twice();
			}
			return curve.Infinity;
		}
		uint[] array11 = array3;
		SecP224R1Field.Square(array9, array11);
		uint[] array12 = Nat224.Create();
		SecP224R1Field.Multiply(array11, array9, array12);
		uint[] array13 = array3;
		SecP224R1Field.Multiply(array11, array7, array13);
		SecP224R1Field.Negate(array12, array12);
		Nat224.Mul(array8, array12, array);
		uint x = Nat224.AddBothTo(array13, array13, array12);
		SecP224R1Field.Reduce32(x, array12);
		SecP224R1FieldElement secP224R1FieldElement7 = new SecP224R1FieldElement(array4);
		SecP224R1Field.Square(array10, secP224R1FieldElement7.x);
		SecP224R1Field.Subtract(secP224R1FieldElement7.x, array12, secP224R1FieldElement7.x);
		SecP224R1FieldElement secP224R1FieldElement8 = new SecP224R1FieldElement(array12);
		SecP224R1Field.Subtract(array13, secP224R1FieldElement7.x, secP224R1FieldElement8.x);
		SecP224R1Field.MultiplyAddToExt(secP224R1FieldElement8.x, array10, array);
		SecP224R1Field.Reduce(array, secP224R1FieldElement8.x);
		SecP224R1FieldElement secP224R1FieldElement9 = new SecP224R1FieldElement(array9);
		if (!isOne)
		{
			SecP224R1Field.Multiply(secP224R1FieldElement9.x, secP224R1FieldElement5.x, secP224R1FieldElement9.x);
		}
		if (!isOne2)
		{
			SecP224R1Field.Multiply(secP224R1FieldElement9.x, secP224R1FieldElement6.x, secP224R1FieldElement9.x);
		}
		ECFieldElement[] zs = new ECFieldElement[1] { secP224R1FieldElement9 };
		return new SecP224R1Point(curve, secP224R1FieldElement7, secP224R1FieldElement8, zs, base.IsCompressed);
	}

	public override ECPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		SecP224R1FieldElement secP224R1FieldElement = (SecP224R1FieldElement)base.RawYCoord;
		if (secP224R1FieldElement.IsZero)
		{
			return curve.Infinity;
		}
		SecP224R1FieldElement secP224R1FieldElement2 = (SecP224R1FieldElement)base.RawXCoord;
		SecP224R1FieldElement secP224R1FieldElement3 = (SecP224R1FieldElement)base.RawZCoords[0];
		uint[] array = Nat224.Create();
		uint[] array2 = Nat224.Create();
		uint[] array3 = Nat224.Create();
		SecP224R1Field.Square(secP224R1FieldElement.x, array3);
		uint[] array4 = Nat224.Create();
		SecP224R1Field.Square(array3, array4);
		bool isOne = secP224R1FieldElement3.IsOne;
		uint[] array5 = secP224R1FieldElement3.x;
		if (!isOne)
		{
			array5 = array2;
			SecP224R1Field.Square(secP224R1FieldElement3.x, array5);
		}
		SecP224R1Field.Subtract(secP224R1FieldElement2.x, array5, array);
		uint[] array6 = array2;
		SecP224R1Field.Add(secP224R1FieldElement2.x, array5, array6);
		SecP224R1Field.Multiply(array6, array, array6);
		uint x = Nat224.AddBothTo(array6, array6, array6);
		SecP224R1Field.Reduce32(x, array6);
		uint[] array7 = array3;
		SecP224R1Field.Multiply(array3, secP224R1FieldElement2.x, array7);
		x = Nat.ShiftUpBits(7, array7, 2, 0u);
		SecP224R1Field.Reduce32(x, array7);
		x = Nat.ShiftUpBits(7, array4, 3, 0u, array);
		SecP224R1Field.Reduce32(x, array);
		SecP224R1FieldElement secP224R1FieldElement4 = new SecP224R1FieldElement(array4);
		SecP224R1Field.Square(array6, secP224R1FieldElement4.x);
		SecP224R1Field.Subtract(secP224R1FieldElement4.x, array7, secP224R1FieldElement4.x);
		SecP224R1Field.Subtract(secP224R1FieldElement4.x, array7, secP224R1FieldElement4.x);
		SecP224R1FieldElement secP224R1FieldElement5 = new SecP224R1FieldElement(array7);
		SecP224R1Field.Subtract(array7, secP224R1FieldElement4.x, secP224R1FieldElement5.x);
		SecP224R1Field.Multiply(secP224R1FieldElement5.x, array6, secP224R1FieldElement5.x);
		SecP224R1Field.Subtract(secP224R1FieldElement5.x, array, secP224R1FieldElement5.x);
		SecP224R1FieldElement secP224R1FieldElement6 = new SecP224R1FieldElement(array6);
		SecP224R1Field.Twice(secP224R1FieldElement.x, secP224R1FieldElement6.x);
		if (!isOne)
		{
			SecP224R1Field.Multiply(secP224R1FieldElement6.x, secP224R1FieldElement3.x, secP224R1FieldElement6.x);
		}
		return new SecP224R1Point(curve, secP224R1FieldElement4, secP224R1FieldElement5, new ECFieldElement[1] { secP224R1FieldElement6 }, base.IsCompressed);
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
		return new SecP224R1Point(Curve, base.RawXCoord, base.RawYCoord.Negate(), base.RawZCoords, base.IsCompressed);
	}
}
