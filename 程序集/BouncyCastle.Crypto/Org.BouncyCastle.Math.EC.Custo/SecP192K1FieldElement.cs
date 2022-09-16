using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP192K1FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFEE37"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat192.IsZero(x);

	public override bool IsOne => Nat192.IsOne(x);

	public override string FieldName => "SecP192K1Field";

	public override int FieldSize => Q.BitLength;

	public SecP192K1FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SecP192K1FieldElement", "x");
		}
		this.x = SecP192K1Field.FromBigInteger(x);
	}

	public SecP192K1FieldElement()
	{
		x = Nat192.Create();
	}

	protected internal SecP192K1FieldElement(uint[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return Nat192.GetBit(x, 0) == 1;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat192.ToBigInteger(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		uint[] z = Nat192.Create();
		SecP192K1Field.Add(x, ((SecP192K1FieldElement)b).x, z);
		return new SecP192K1FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat192.Create();
		SecP192K1Field.AddOne(x, z);
		return new SecP192K1FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat192.Create();
		SecP192K1Field.Subtract(x, ((SecP192K1FieldElement)b).x, z);
		return new SecP192K1FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat192.Create();
		SecP192K1Field.Multiply(x, ((SecP192K1FieldElement)b).x, z);
		return new SecP192K1FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat192.Create();
		SecP192K1Field.Inv(((SecP192K1FieldElement)b).x, z);
		SecP192K1Field.Multiply(z, x, z);
		return new SecP192K1FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat192.Create();
		SecP192K1Field.Negate(x, z);
		return new SecP192K1FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat192.Create();
		SecP192K1Field.Square(x, z);
		return new SecP192K1FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat192.Create();
		SecP192K1Field.Inv(x, z);
		return new SecP192K1FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] y = x;
		if (Nat192.IsZero(y) || Nat192.IsOne(y))
		{
			return this;
		}
		uint[] array = Nat192.Create();
		SecP192K1Field.Square(y, array);
		SecP192K1Field.Multiply(array, y, array);
		uint[] array2 = Nat192.Create();
		SecP192K1Field.Square(array, array2);
		SecP192K1Field.Multiply(array2, y, array2);
		uint[] array3 = Nat192.Create();
		SecP192K1Field.SquareN(array2, 3, array3);
		SecP192K1Field.Multiply(array3, array2, array3);
		uint[] array4 = array3;
		SecP192K1Field.SquareN(array3, 2, array4);
		SecP192K1Field.Multiply(array4, array, array4);
		uint[] array5 = array;
		SecP192K1Field.SquareN(array4, 8, array5);
		SecP192K1Field.Multiply(array5, array4, array5);
		uint[] array6 = array4;
		SecP192K1Field.SquareN(array5, 3, array6);
		SecP192K1Field.Multiply(array6, array2, array6);
		uint[] array7 = Nat192.Create();
		SecP192K1Field.SquareN(array6, 16, array7);
		SecP192K1Field.Multiply(array7, array5, array7);
		uint[] array8 = array5;
		SecP192K1Field.SquareN(array7, 35, array8);
		SecP192K1Field.Multiply(array8, array7, array8);
		uint[] z = array7;
		SecP192K1Field.SquareN(array8, 70, z);
		SecP192K1Field.Multiply(z, array8, z);
		uint[] array9 = array8;
		SecP192K1Field.SquareN(z, 19, array9);
		SecP192K1Field.Multiply(array9, array6, array9);
		uint[] z2 = array9;
		SecP192K1Field.SquareN(z2, 20, z2);
		SecP192K1Field.Multiply(z2, array6, z2);
		SecP192K1Field.SquareN(z2, 4, z2);
		SecP192K1Field.Multiply(z2, array2, z2);
		SecP192K1Field.SquareN(z2, 6, z2);
		SecP192K1Field.Multiply(z2, array2, z2);
		SecP192K1Field.Square(z2, z2);
		uint[] array10 = array2;
		SecP192K1Field.Square(z2, array10);
		if (!Nat192.Eq(y, array10))
		{
			return null;
		}
		return new SecP192K1FieldElement(z2);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecP192K1FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecP192K1FieldElement);
	}

	public virtual bool Equals(SecP192K1FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat192.Eq(x, other.x);
	}

	public override int GetHashCode()
	{
		return Q.GetHashCode() ^ Arrays.GetHashCode(x, 0, 6);
	}
}
