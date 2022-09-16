using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP192R1FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFFFFFFFFFF"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat192.IsZero(x);

	public override bool IsOne => Nat192.IsOne(x);

	public override string FieldName => "SecP192R1Field";

	public override int FieldSize => Q.BitLength;

	public SecP192R1FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SecP192R1FieldElement", "x");
		}
		this.x = SecP192R1Field.FromBigInteger(x);
	}

	public SecP192R1FieldElement()
	{
		x = Nat192.Create();
	}

	protected internal SecP192R1FieldElement(uint[] x)
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
		SecP192R1Field.Add(x, ((SecP192R1FieldElement)b).x, z);
		return new SecP192R1FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat192.Create();
		SecP192R1Field.AddOne(x, z);
		return new SecP192R1FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat192.Create();
		SecP192R1Field.Subtract(x, ((SecP192R1FieldElement)b).x, z);
		return new SecP192R1FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat192.Create();
		SecP192R1Field.Multiply(x, ((SecP192R1FieldElement)b).x, z);
		return new SecP192R1FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat192.Create();
		SecP192R1Field.Inv(((SecP192R1FieldElement)b).x, z);
		SecP192R1Field.Multiply(z, x, z);
		return new SecP192R1FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat192.Create();
		SecP192R1Field.Negate(x, z);
		return new SecP192R1FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat192.Create();
		SecP192R1Field.Square(x, z);
		return new SecP192R1FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat192.Create();
		SecP192R1Field.Inv(x, z);
		return new SecP192R1FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] y = x;
		if (Nat192.IsZero(y) || Nat192.IsOne(y))
		{
			return this;
		}
		uint[] array = Nat192.Create();
		uint[] array2 = Nat192.Create();
		SecP192R1Field.Square(y, array);
		SecP192R1Field.Multiply(array, y, array);
		SecP192R1Field.SquareN(array, 2, array2);
		SecP192R1Field.Multiply(array2, array, array2);
		SecP192R1Field.SquareN(array2, 4, array);
		SecP192R1Field.Multiply(array, array2, array);
		SecP192R1Field.SquareN(array, 8, array2);
		SecP192R1Field.Multiply(array2, array, array2);
		SecP192R1Field.SquareN(array2, 16, array);
		SecP192R1Field.Multiply(array, array2, array);
		SecP192R1Field.SquareN(array, 32, array2);
		SecP192R1Field.Multiply(array2, array, array2);
		SecP192R1Field.SquareN(array2, 64, array);
		SecP192R1Field.Multiply(array, array2, array);
		SecP192R1Field.SquareN(array, 62, array);
		SecP192R1Field.Square(array, array2);
		if (!Nat192.Eq(y, array2))
		{
			return null;
		}
		return new SecP192R1FieldElement(array);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecP192R1FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecP192R1FieldElement);
	}

	public virtual bool Equals(SecP192R1FieldElement other)
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
