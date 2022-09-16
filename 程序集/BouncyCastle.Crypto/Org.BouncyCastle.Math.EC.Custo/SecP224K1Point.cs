using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP224K1Point : AbstractFpPoint
{
	public SecP224K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	public SecP224K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal SecP224K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new SecP224K1Point(null, AffineXCoord, AffineYCoord);
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
		SecP224K1FieldElement secP224K1FieldElement = (SecP224K1FieldElement)base.RawXCoord;
		SecP224K1FieldElement secP224K1FieldElement2 = (SecP224K1FieldElement)base.RawYCoord;
		SecP224K1FieldElement secP224K1FieldElement3 = (SecP224K1FieldElement)b.RawXCoord;
		SecP224K1FieldElement secP224K1FieldElement4 = (SecP224K1FieldElement)b.RawYCoord;
		SecP224K1FieldElement secP224K1FieldElement5 = (SecP224K1FieldElement)base.RawZCoords[0];
		SecP224K1FieldElement secP224K1FieldElement6 = (SecP224K1FieldElement)b.RawZCoords[0];
		uint[] array = Nat224.CreateExt();
		uint[] array2 = Nat224.Create();
		uint[] array3 = Nat224.Create();
		uint[] array4 = Nat224.Create();
		bool isOne = secP224K1FieldElement5.IsOne;
		uint[] array5;
		uint[] array6;
		if (isOne)
		{
			array5 = secP224K1FieldElement3.x;
			array6 = secP224K1FieldElement4.x;
		}
		else
		{
			array6 = array3;
			SecP224K1Field.Square(secP224K1FieldElement5.x, array6);
			array5 = array2;
			SecP224K1Field.Multiply(array6, secP224K1FieldElement3.x, array5);
			SecP224K1Field.Multiply(array6, secP224K1FieldElement5.x, array6);
			SecP224K1Field.Multiply(array6, secP224K1FieldElement4.x, array6);
		}
		bool isOne2 = secP224K1FieldElement6.IsOne;
		uint[] array7;
		uint[] array8;
		if (isOne2)
		{
			array7 = secP224K1FieldElement.x;
			array8 = secP224K1FieldElement2.x;
		}
		else
		{
			array8 = array4;
			SecP224K1Field.Square(secP224K1FieldElement6.x, array8);
			array7 = array;
			SecP224K1Field.Multiply(array8, secP224K1FieldElement.x, array7);
			SecP224K1Field.Multiply(array8, secP224K1FieldElement6.x, array8);
			SecP224K1Field.Multiply(array8, secP224K1FieldElement2.x, array8);
		}
		uint[] array9 = Nat224.Create();
		SecP224K1Field.Subtract(array7, array5, array9);
		uint[] array10 = array2;
		SecP224K1Field.Subtract(array8, array6, array10);
		if (Nat224.IsZero(array9))
		{
			if (Nat224.IsZero(array10))
			{
				return Twice();
			}
			return curve.Infinity;
		}
		uint[] array11 = array3;
		SecP224K1Field.Square(array9, array11);
		uint[] array12 = Nat224.Create();
		SecP224K1Field.Multiply(array11, array9, array12);
		uint[] array13 = array3;
		SecP224K1Field.Multiply(array11, array7, array13);
		SecP224K1Field.Negate(array12, array12);
		Nat224.Mul(array8, array12, array);
		uint x = Nat224.AddBothTo(array13, array13, array12);
		SecP224K1Field.Reduce32(x, array12);
		SecP224K1FieldElement secP224K1FieldElement7 = new SecP224K1FieldElement(array4);
		SecP224K1Field.Square(array10, secP224K1FieldElement7.x);
		SecP224K1Field.Subtract(secP224K1FieldElement7.x, array12, secP224K1FieldElement7.x);
		SecP224K1FieldElement secP224K1FieldElement8 = new SecP224K1FieldElement(array12);
		SecP224K1Field.Subtract(array13, secP224K1FieldElement7.x, secP224K1FieldElement8.x);
		SecP224K1Field.MultiplyAddToExt(secP224K1FieldElement8.x, array10, array);
		SecP224K1Field.Reduce(array, secP224K1FieldElement8.x);
		SecP224K1FieldElement secP224K1FieldElement9 = new SecP224K1FieldElement(array9);
		if (!isOne)
		{
			SecP224K1Field.Multiply(secP224K1FieldElement9.x, secP224K1FieldElement5.x, secP224K1FieldElement9.x);
		}
		if (!isOne2)
		{
			SecP224K1Field.Multiply(secP224K1FieldElement9.x, secP224K1FieldElement6.x, secP224K1FieldElement9.x);
		}
		ECFieldElement[] zs = new ECFieldElement[1] { secP224K1FieldElement9 };
		return new SecP224K1Point(curve, secP224K1FieldElement7, secP224K1FieldElement8, zs, base.IsCompressed);
	}

	public override ECPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		SecP224K1FieldElement secP224K1FieldElement = (SecP224K1FieldElement)base.RawYCoord;
		if (secP224K1FieldElement.IsZero)
		{
			return curve.Infinity;
		}
		SecP224K1FieldElement secP224K1FieldElement2 = (SecP224K1FieldElement)base.RawXCoord;
		SecP224K1FieldElement secP224K1FieldElement3 = (SecP224K1FieldElement)base.RawZCoords[0];
		uint[] array = Nat224.Create();
		SecP224K1Field.Square(secP224K1FieldElement.x, array);
		uint[] array2 = Nat224.Create();
		SecP224K1Field.Square(array, array2);
		uint[] array3 = Nat224.Create();
		SecP224K1Field.Square(secP224K1FieldElement2.x, array3);
		uint x = Nat224.AddBothTo(array3, array3, array3);
		SecP224K1Field.Reduce32(x, array3);
		uint[] array4 = array;
		SecP224K1Field.Multiply(array, secP224K1FieldElement2.x, array4);
		x = Nat.ShiftUpBits(7, array4, 2, 0u);
		SecP224K1Field.Reduce32(x, array4);
		uint[] array5 = Nat224.Create();
		x = Nat.ShiftUpBits(7, array2, 3, 0u, array5);
		SecP224K1Field.Reduce32(x, array5);
		SecP224K1FieldElement secP224K1FieldElement4 = new SecP224K1FieldElement(array2);
		SecP224K1Field.Square(array3, secP224K1FieldElement4.x);
		SecP224K1Field.Subtract(secP224K1FieldElement4.x, array4, secP224K1FieldElement4.x);
		SecP224K1Field.Subtract(secP224K1FieldElement4.x, array4, secP224K1FieldElement4.x);
		SecP224K1FieldElement secP224K1FieldElement5 = new SecP224K1FieldElement(array4);
		SecP224K1Field.Subtract(array4, secP224K1FieldElement4.x, secP224K1FieldElement5.x);
		SecP224K1Field.Multiply(secP224K1FieldElement5.x, array3, secP224K1FieldElement5.x);
		SecP224K1Field.Subtract(secP224K1FieldElement5.x, array5, secP224K1FieldElement5.x);
		SecP224K1FieldElement secP224K1FieldElement6 = new SecP224K1FieldElement(array3);
		SecP224K1Field.Twice(secP224K1FieldElement.x, secP224K1FieldElement6.x);
		if (!secP224K1FieldElement3.IsOne)
		{
			SecP224K1Field.Multiply(secP224K1FieldElement6.x, secP224K1FieldElement3.x, secP224K1FieldElement6.x);
		}
		return new SecP224K1Point(curve, secP224K1FieldElement4, secP224K1FieldElement5, new ECFieldElement[1] { secP224K1FieldElement6 }, base.IsCompressed);
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
		return new SecP224K1Point(Curve, base.RawXCoord, base.RawYCoord.Negate(), base.RawZCoords, base.IsCompressed);
	}
}
