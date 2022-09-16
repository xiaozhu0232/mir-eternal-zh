using System;
using Org.BouncyCastle.Math.Raw;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP128R1Point : AbstractFpPoint
{
	public SecP128R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
		: this(curve, x, y, withCompression: false)
	{
	}

	public SecP128R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
		: base(curve, x, y, withCompression)
	{
		if (x == null != (y == null))
		{
			throw new ArgumentException("Exactly one of the field elements is null");
		}
	}

	internal SecP128R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		: base(curve, x, y, zs, withCompression)
	{
	}

	protected override ECPoint Detach()
	{
		return new SecP128R1Point(null, AffineXCoord, AffineYCoord);
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
		SecP128R1FieldElement secP128R1FieldElement = (SecP128R1FieldElement)base.RawXCoord;
		SecP128R1FieldElement secP128R1FieldElement2 = (SecP128R1FieldElement)base.RawYCoord;
		SecP128R1FieldElement secP128R1FieldElement3 = (SecP128R1FieldElement)b.RawXCoord;
		SecP128R1FieldElement secP128R1FieldElement4 = (SecP128R1FieldElement)b.RawYCoord;
		SecP128R1FieldElement secP128R1FieldElement5 = (SecP128R1FieldElement)base.RawZCoords[0];
		SecP128R1FieldElement secP128R1FieldElement6 = (SecP128R1FieldElement)b.RawZCoords[0];
		uint[] array = Nat128.CreateExt();
		uint[] array2 = Nat128.Create();
		uint[] array3 = Nat128.Create();
		uint[] array4 = Nat128.Create();
		bool isOne = secP128R1FieldElement5.IsOne;
		uint[] array5;
		uint[] array6;
		if (isOne)
		{
			array5 = secP128R1FieldElement3.x;
			array6 = secP128R1FieldElement4.x;
		}
		else
		{
			array6 = array3;
			SecP128R1Field.Square(secP128R1FieldElement5.x, array6);
			array5 = array2;
			SecP128R1Field.Multiply(array6, secP128R1FieldElement3.x, array5);
			SecP128R1Field.Multiply(array6, secP128R1FieldElement5.x, array6);
			SecP128R1Field.Multiply(array6, secP128R1FieldElement4.x, array6);
		}
		bool isOne2 = secP128R1FieldElement6.IsOne;
		uint[] array7;
		uint[] array8;
		if (isOne2)
		{
			array7 = secP128R1FieldElement.x;
			array8 = secP128R1FieldElement2.x;
		}
		else
		{
			array8 = array4;
			SecP128R1Field.Square(secP128R1FieldElement6.x, array8);
			array7 = array;
			SecP128R1Field.Multiply(array8, secP128R1FieldElement.x, array7);
			SecP128R1Field.Multiply(array8, secP128R1FieldElement6.x, array8);
			SecP128R1Field.Multiply(array8, secP128R1FieldElement2.x, array8);
		}
		uint[] array9 = Nat128.Create();
		SecP128R1Field.Subtract(array7, array5, array9);
		uint[] array10 = array2;
		SecP128R1Field.Subtract(array8, array6, array10);
		if (Nat128.IsZero(array9))
		{
			if (Nat128.IsZero(array10))
			{
				return Twice();
			}
			return curve.Infinity;
		}
		uint[] array11 = array3;
		SecP128R1Field.Square(array9, array11);
		uint[] array12 = Nat128.Create();
		SecP128R1Field.Multiply(array11, array9, array12);
		uint[] array13 = array3;
		SecP128R1Field.Multiply(array11, array7, array13);
		SecP128R1Field.Negate(array12, array12);
		Nat128.Mul(array8, array12, array);
		uint x = Nat128.AddBothTo(array13, array13, array12);
		SecP128R1Field.Reduce32(x, array12);
		SecP128R1FieldElement secP128R1FieldElement7 = new SecP128R1FieldElement(array4);
		SecP128R1Field.Square(array10, secP128R1FieldElement7.x);
		SecP128R1Field.Subtract(secP128R1FieldElement7.x, array12, secP128R1FieldElement7.x);
		SecP128R1FieldElement secP128R1FieldElement8 = new SecP128R1FieldElement(array12);
		SecP128R1Field.Subtract(array13, secP128R1FieldElement7.x, secP128R1FieldElement8.x);
		SecP128R1Field.MultiplyAddToExt(secP128R1FieldElement8.x, array10, array);
		SecP128R1Field.Reduce(array, secP128R1FieldElement8.x);
		SecP128R1FieldElement secP128R1FieldElement9 = new SecP128R1FieldElement(array9);
		if (!isOne)
		{
			SecP128R1Field.Multiply(secP128R1FieldElement9.x, secP128R1FieldElement5.x, secP128R1FieldElement9.x);
		}
		if (!isOne2)
		{
			SecP128R1Field.Multiply(secP128R1FieldElement9.x, secP128R1FieldElement6.x, secP128R1FieldElement9.x);
		}
		ECFieldElement[] zs = new ECFieldElement[1] { secP128R1FieldElement9 };
		return new SecP128R1Point(curve, secP128R1FieldElement7, secP128R1FieldElement8, zs, base.IsCompressed);
	}

	public override ECPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		ECCurve curve = Curve;
		SecP128R1FieldElement secP128R1FieldElement = (SecP128R1FieldElement)base.RawYCoord;
		if (secP128R1FieldElement.IsZero)
		{
			return curve.Infinity;
		}
		SecP128R1FieldElement secP128R1FieldElement2 = (SecP128R1FieldElement)base.RawXCoord;
		SecP128R1FieldElement secP128R1FieldElement3 = (SecP128R1FieldElement)base.RawZCoords[0];
		uint[] array = Nat128.Create();
		uint[] array2 = Nat128.Create();
		uint[] array3 = Nat128.Create();
		SecP128R1Field.Square(secP128R1FieldElement.x, array3);
		uint[] array4 = Nat128.Create();
		SecP128R1Field.Square(array3, array4);
		bool isOne = secP128R1FieldElement3.IsOne;
		uint[] array5 = secP128R1FieldElement3.x;
		if (!isOne)
		{
			array5 = array2;
			SecP128R1Field.Square(secP128R1FieldElement3.x, array5);
		}
		SecP128R1Field.Subtract(secP128R1FieldElement2.x, array5, array);
		uint[] array6 = array2;
		SecP128R1Field.Add(secP128R1FieldElement2.x, array5, array6);
		SecP128R1Field.Multiply(array6, array, array6);
		uint x = Nat128.AddBothTo(array6, array6, array6);
		SecP128R1Field.Reduce32(x, array6);
		uint[] array7 = array3;
		SecP128R1Field.Multiply(array3, secP128R1FieldElement2.x, array7);
		x = Nat.ShiftUpBits(4, array7, 2, 0u);
		SecP128R1Field.Reduce32(x, array7);
		x = Nat.ShiftUpBits(4, array4, 3, 0u, array);
		SecP128R1Field.Reduce32(x, array);
		SecP128R1FieldElement secP128R1FieldElement4 = new SecP128R1FieldElement(array4);
		SecP128R1Field.Square(array6, secP128R1FieldElement4.x);
		SecP128R1Field.Subtract(secP128R1FieldElement4.x, array7, secP128R1FieldElement4.x);
		SecP128R1Field.Subtract(secP128R1FieldElement4.x, array7, secP128R1FieldElement4.x);
		SecP128R1FieldElement secP128R1FieldElement5 = new SecP128R1FieldElement(array7);
		SecP128R1Field.Subtract(array7, secP128R1FieldElement4.x, secP128R1FieldElement5.x);
		SecP128R1Field.Multiply(secP128R1FieldElement5.x, array6, secP128R1FieldElement5.x);
		SecP128R1Field.Subtract(secP128R1FieldElement5.x, array, secP128R1FieldElement5.x);
		SecP128R1FieldElement secP128R1FieldElement6 = new SecP128R1FieldElement(array6);
		SecP128R1Field.Twice(secP128R1FieldElement.x, secP128R1FieldElement6.x);
		if (!isOne)
		{
			SecP128R1Field.Multiply(secP128R1FieldElement6.x, secP128R1FieldElement3.x, secP128R1FieldElement6.x);
		}
		return new SecP128R1Point(curve, secP128R1FieldElement4, secP128R1FieldElement5, new ECFieldElement[1] { secP128R1FieldElement6 }, base.IsCompressed);
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
		return new SecP128R1Point(Curve, base.RawXCoord, base.RawYCoord.Negate(), base.RawZCoords, base.IsCompressed);
	}
}
