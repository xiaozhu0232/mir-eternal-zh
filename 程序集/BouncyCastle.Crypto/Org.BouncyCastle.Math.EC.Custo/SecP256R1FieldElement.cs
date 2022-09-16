using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP256R1FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat256.IsZero(x);

	public override bool IsOne => Nat256.IsOne(x);

	public override string FieldName => "SecP256R1Field";

	public override int FieldSize => Q.BitLength;

	public SecP256R1FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SecP256R1FieldElement", "x");
		}
		this.x = SecP256R1Field.FromBigInteger(x);
	}

	public SecP256R1FieldElement()
	{
		x = Nat256.Create();
	}

	protected internal SecP256R1FieldElement(uint[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return Nat256.GetBit(x, 0) == 1;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat256.ToBigInteger(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		uint[] z = Nat256.Create();
		SecP256R1Field.Add(x, ((SecP256R1FieldElement)b).x, z);
		return new SecP256R1FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat256.Create();
		SecP256R1Field.AddOne(x, z);
		return new SecP256R1FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat256.Create();
		SecP256R1Field.Subtract(x, ((SecP256R1FieldElement)b).x, z);
		return new SecP256R1FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat256.Create();
		SecP256R1Field.Multiply(x, ((SecP256R1FieldElement)b).x, z);
		return new SecP256R1FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat256.Create();
		SecP256R1Field.Inv(((SecP256R1FieldElement)b).x, z);
		SecP256R1Field.Multiply(z, x, z);
		return new SecP256R1FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat256.Create();
		SecP256R1Field.Negate(x, z);
		return new SecP256R1FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat256.Create();
		SecP256R1Field.Square(x, z);
		return new SecP256R1FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat256.Create();
		SecP256R1Field.Inv(x, z);
		return new SecP256R1FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] y = x;
		if (Nat256.IsZero(y) || Nat256.IsOne(y))
		{
			return this;
		}
		uint[] array = Nat256.Create();
		uint[] array2 = Nat256.Create();
		SecP256R1Field.Square(y, array);
		SecP256R1Field.Multiply(array, y, array);
		SecP256R1Field.SquareN(array, 2, array2);
		SecP256R1Field.Multiply(array2, array, array2);
		SecP256R1Field.SquareN(array2, 4, array);
		SecP256R1Field.Multiply(array, array2, array);
		SecP256R1Field.SquareN(array, 8, array2);
		SecP256R1Field.Multiply(array2, array, array2);
		SecP256R1Field.SquareN(array2, 16, array);
		SecP256R1Field.Multiply(array, array2, array);
		SecP256R1Field.SquareN(array, 32, array);
		SecP256R1Field.Multiply(array, y, array);
		SecP256R1Field.SquareN(array, 96, array);
		SecP256R1Field.Multiply(array, y, array);
		SecP256R1Field.SquareN(array, 94, array);
		SecP256R1Field.Multiply(array, array, array2);
		if (!Nat256.Eq(y, array2))
		{
			return null;
		}
		return new SecP256R1FieldElement(array);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecP256R1FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecP256R1FieldElement);
	}

	public virtual bool Equals(SecP256R1FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat256.Eq(x, other.x);
	}

	public override int GetHashCode()
	{
		return Q.GetHashCode() ^ Arrays.GetHashCode(x, 0, 8);
	}
}
