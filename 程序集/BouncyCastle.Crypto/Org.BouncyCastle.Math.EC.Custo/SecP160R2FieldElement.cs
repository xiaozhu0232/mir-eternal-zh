using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP160R2FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFAC73"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat160.IsZero(x);

	public override bool IsOne => Nat160.IsOne(x);

	public override string FieldName => "SecP160R2Field";

	public override int FieldSize => Q.BitLength;

	public SecP160R2FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SecP160R2FieldElement", "x");
		}
		this.x = SecP160R2Field.FromBigInteger(x);
	}

	public SecP160R2FieldElement()
	{
		x = Nat160.Create();
	}

	protected internal SecP160R2FieldElement(uint[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return Nat160.GetBit(x, 0) == 1;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat160.ToBigInteger(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		uint[] z = Nat160.Create();
		SecP160R2Field.Add(x, ((SecP160R2FieldElement)b).x, z);
		return new SecP160R2FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat160.Create();
		SecP160R2Field.AddOne(x, z);
		return new SecP160R2FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat160.Create();
		SecP160R2Field.Subtract(x, ((SecP160R2FieldElement)b).x, z);
		return new SecP160R2FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat160.Create();
		SecP160R2Field.Multiply(x, ((SecP160R2FieldElement)b).x, z);
		return new SecP160R2FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat160.Create();
		SecP160R2Field.Inv(((SecP160R2FieldElement)b).x, z);
		SecP160R2Field.Multiply(z, x, z);
		return new SecP160R2FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat160.Create();
		SecP160R2Field.Negate(x, z);
		return new SecP160R2FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat160.Create();
		SecP160R2Field.Square(x, z);
		return new SecP160R2FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat160.Create();
		SecP160R2Field.Inv(x, z);
		return new SecP160R2FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] y = x;
		if (Nat160.IsZero(y) || Nat160.IsOne(y))
		{
			return this;
		}
		uint[] array = Nat160.Create();
		SecP160R2Field.Square(y, array);
		SecP160R2Field.Multiply(array, y, array);
		uint[] array2 = Nat160.Create();
		SecP160R2Field.Square(array, array2);
		SecP160R2Field.Multiply(array2, y, array2);
		uint[] array3 = Nat160.Create();
		SecP160R2Field.Square(array2, array3);
		SecP160R2Field.Multiply(array3, y, array3);
		uint[] array4 = Nat160.Create();
		SecP160R2Field.SquareN(array3, 3, array4);
		SecP160R2Field.Multiply(array4, array2, array4);
		uint[] array5 = array3;
		SecP160R2Field.SquareN(array4, 7, array5);
		SecP160R2Field.Multiply(array5, array4, array5);
		uint[] array6 = array4;
		SecP160R2Field.SquareN(array5, 3, array6);
		SecP160R2Field.Multiply(array6, array2, array6);
		uint[] array7 = Nat160.Create();
		SecP160R2Field.SquareN(array6, 14, array7);
		SecP160R2Field.Multiply(array7, array5, array7);
		uint[] array8 = array5;
		SecP160R2Field.SquareN(array7, 31, array8);
		SecP160R2Field.Multiply(array8, array7, array8);
		uint[] z = array7;
		SecP160R2Field.SquareN(array8, 62, z);
		SecP160R2Field.Multiply(z, array8, z);
		uint[] array9 = array8;
		SecP160R2Field.SquareN(z, 3, array9);
		SecP160R2Field.Multiply(array9, array2, array9);
		uint[] z2 = array9;
		SecP160R2Field.SquareN(z2, 18, z2);
		SecP160R2Field.Multiply(z2, array6, z2);
		SecP160R2Field.SquareN(z2, 2, z2);
		SecP160R2Field.Multiply(z2, y, z2);
		SecP160R2Field.SquareN(z2, 3, z2);
		SecP160R2Field.Multiply(z2, array, z2);
		SecP160R2Field.SquareN(z2, 6, z2);
		SecP160R2Field.Multiply(z2, array2, z2);
		SecP160R2Field.SquareN(z2, 2, z2);
		SecP160R2Field.Multiply(z2, y, z2);
		uint[] array10 = array;
		SecP160R2Field.Square(z2, array10);
		if (!Nat160.Eq(y, array10))
		{
			return null;
		}
		return new SecP160R2FieldElement(z2);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecP160R2FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecP160R2FieldElement);
	}

	public virtual bool Equals(SecP160R2FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat160.Eq(x, other.x);
	}

	public override int GetHashCode()
	{
		return Q.GetHashCode() ^ Arrays.GetHashCode(x, 0, 5);
	}
}
