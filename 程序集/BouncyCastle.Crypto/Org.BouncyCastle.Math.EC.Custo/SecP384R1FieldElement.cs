using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP384R1FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFF0000000000000000FFFFFFFF"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat.IsZero(12, x);

	public override bool IsOne => Nat.IsOne(12, x);

	public override string FieldName => "SecP384R1Field";

	public override int FieldSize => Q.BitLength;

	public SecP384R1FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SecP384R1FieldElement", "x");
		}
		this.x = SecP384R1Field.FromBigInteger(x);
	}

	public SecP384R1FieldElement()
	{
		x = Nat.Create(12);
	}

	protected internal SecP384R1FieldElement(uint[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return Nat.GetBit(x, 0) == 1;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat.ToBigInteger(12, x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		uint[] z = Nat.Create(12);
		SecP384R1Field.Add(x, ((SecP384R1FieldElement)b).x, z);
		return new SecP384R1FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat.Create(12);
		SecP384R1Field.AddOne(x, z);
		return new SecP384R1FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat.Create(12);
		SecP384R1Field.Subtract(x, ((SecP384R1FieldElement)b).x, z);
		return new SecP384R1FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat.Create(12);
		SecP384R1Field.Multiply(x, ((SecP384R1FieldElement)b).x, z);
		return new SecP384R1FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat.Create(12);
		SecP384R1Field.Inv(((SecP384R1FieldElement)b).x, z);
		SecP384R1Field.Multiply(z, x, z);
		return new SecP384R1FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat.Create(12);
		SecP384R1Field.Negate(x, z);
		return new SecP384R1FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat.Create(12);
		SecP384R1Field.Square(x, z);
		return new SecP384R1FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat.Create(12);
		SecP384R1Field.Inv(x, z);
		return new SecP384R1FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] y = x;
		if (Nat.IsZero(12, y) || Nat.IsOne(12, y))
		{
			return this;
		}
		uint[] array = Nat.Create(12);
		uint[] array2 = Nat.Create(12);
		uint[] array3 = Nat.Create(12);
		uint[] array4 = Nat.Create(12);
		SecP384R1Field.Square(y, array);
		SecP384R1Field.Multiply(array, y, array);
		SecP384R1Field.SquareN(array, 2, array2);
		SecP384R1Field.Multiply(array2, array, array2);
		SecP384R1Field.Square(array2, array2);
		SecP384R1Field.Multiply(array2, y, array2);
		SecP384R1Field.SquareN(array2, 5, array3);
		SecP384R1Field.Multiply(array3, array2, array3);
		SecP384R1Field.SquareN(array3, 5, array4);
		SecP384R1Field.Multiply(array4, array2, array4);
		SecP384R1Field.SquareN(array4, 15, array2);
		SecP384R1Field.Multiply(array2, array4, array2);
		SecP384R1Field.SquareN(array2, 2, array3);
		SecP384R1Field.Multiply(array, array3, array);
		SecP384R1Field.SquareN(array3, 28, array3);
		SecP384R1Field.Multiply(array2, array3, array2);
		SecP384R1Field.SquareN(array2, 60, array3);
		SecP384R1Field.Multiply(array3, array2, array3);
		uint[] z = array2;
		SecP384R1Field.SquareN(array3, 120, z);
		SecP384R1Field.Multiply(z, array3, z);
		SecP384R1Field.SquareN(z, 15, z);
		SecP384R1Field.Multiply(z, array4, z);
		SecP384R1Field.SquareN(z, 33, z);
		SecP384R1Field.Multiply(z, array, z);
		SecP384R1Field.SquareN(z, 64, z);
		SecP384R1Field.Multiply(z, y, z);
		SecP384R1Field.SquareN(z, 30, array);
		SecP384R1Field.Square(array, array2);
		if (!Nat.Eq(12, y, array2))
		{
			return null;
		}
		return new SecP384R1FieldElement(array);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecP384R1FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecP384R1FieldElement);
	}

	public virtual bool Equals(SecP384R1FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat.Eq(12, x, other.x);
	}

	public override int GetHashCode()
	{
		return Q.GetHashCode() ^ Arrays.GetHashCode(x, 0, 12);
	}
}
