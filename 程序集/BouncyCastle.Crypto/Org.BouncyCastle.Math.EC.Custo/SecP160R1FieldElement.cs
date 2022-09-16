using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP160R1FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF7FFFFFFF"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat160.IsZero(x);

	public override bool IsOne => Nat160.IsOne(x);

	public override string FieldName => "SecP160R1Field";

	public override int FieldSize => Q.BitLength;

	public SecP160R1FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SecP160R1FieldElement", "x");
		}
		this.x = SecP160R1Field.FromBigInteger(x);
	}

	public SecP160R1FieldElement()
	{
		x = Nat160.Create();
	}

	protected internal SecP160R1FieldElement(uint[] x)
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
		SecP160R1Field.Add(x, ((SecP160R1FieldElement)b).x, z);
		return new SecP160R1FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat160.Create();
		SecP160R1Field.AddOne(x, z);
		return new SecP160R1FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat160.Create();
		SecP160R1Field.Subtract(x, ((SecP160R1FieldElement)b).x, z);
		return new SecP160R1FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat160.Create();
		SecP160R1Field.Multiply(x, ((SecP160R1FieldElement)b).x, z);
		return new SecP160R1FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat160.Create();
		SecP160R1Field.Inv(((SecP160R1FieldElement)b).x, z);
		SecP160R1Field.Multiply(z, x, z);
		return new SecP160R1FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat160.Create();
		SecP160R1Field.Negate(x, z);
		return new SecP160R1FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat160.Create();
		SecP160R1Field.Square(x, z);
		return new SecP160R1FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat160.Create();
		SecP160R1Field.Inv(x, z);
		return new SecP160R1FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] y = x;
		if (Nat160.IsZero(y) || Nat160.IsOne(y))
		{
			return this;
		}
		uint[] array = Nat160.Create();
		SecP160R1Field.Square(y, array);
		SecP160R1Field.Multiply(array, y, array);
		uint[] array2 = Nat160.Create();
		SecP160R1Field.SquareN(array, 2, array2);
		SecP160R1Field.Multiply(array2, array, array2);
		uint[] array3 = array;
		SecP160R1Field.SquareN(array2, 4, array3);
		SecP160R1Field.Multiply(array3, array2, array3);
		uint[] array4 = array2;
		SecP160R1Field.SquareN(array3, 8, array4);
		SecP160R1Field.Multiply(array4, array3, array4);
		uint[] array5 = array3;
		SecP160R1Field.SquareN(array4, 16, array5);
		SecP160R1Field.Multiply(array5, array4, array5);
		uint[] array6 = array4;
		SecP160R1Field.SquareN(array5, 32, array6);
		SecP160R1Field.Multiply(array6, array5, array6);
		uint[] array7 = array5;
		SecP160R1Field.SquareN(array6, 64, array7);
		SecP160R1Field.Multiply(array7, array6, array7);
		uint[] array8 = array6;
		SecP160R1Field.Square(array7, array8);
		SecP160R1Field.Multiply(array8, y, array8);
		uint[] z = array8;
		SecP160R1Field.SquareN(z, 29, z);
		uint[] array9 = array7;
		SecP160R1Field.Square(z, array9);
		if (!Nat160.Eq(y, array9))
		{
			return null;
		}
		return new SecP160R1FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecP160R1FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecP160R1FieldElement);
	}

	public virtual bool Equals(SecP160R1FieldElement other)
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
