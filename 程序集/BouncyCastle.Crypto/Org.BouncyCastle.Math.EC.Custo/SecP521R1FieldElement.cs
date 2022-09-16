using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP521R1FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat.IsZero(17, x);

	public override bool IsOne => Nat.IsOne(17, x);

	public override string FieldName => "SecP521R1Field";

	public override int FieldSize => Q.BitLength;

	public SecP521R1FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SecP521R1FieldElement", "x");
		}
		this.x = SecP521R1Field.FromBigInteger(x);
	}

	public SecP521R1FieldElement()
	{
		x = Nat.Create(17);
	}

	protected internal SecP521R1FieldElement(uint[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return Nat.GetBit(x, 0) == 1;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat.ToBigInteger(17, x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		uint[] z = Nat.Create(17);
		SecP521R1Field.Add(x, ((SecP521R1FieldElement)b).x, z);
		return new SecP521R1FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat.Create(17);
		SecP521R1Field.AddOne(x, z);
		return new SecP521R1FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat.Create(17);
		SecP521R1Field.Subtract(x, ((SecP521R1FieldElement)b).x, z);
		return new SecP521R1FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat.Create(17);
		SecP521R1Field.Multiply(x, ((SecP521R1FieldElement)b).x, z);
		return new SecP521R1FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat.Create(17);
		SecP521R1Field.Inv(((SecP521R1FieldElement)b).x, z);
		SecP521R1Field.Multiply(z, x, z);
		return new SecP521R1FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat.Create(17);
		SecP521R1Field.Negate(x, z);
		return new SecP521R1FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat.Create(17);
		SecP521R1Field.Square(x, z);
		return new SecP521R1FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat.Create(17);
		SecP521R1Field.Inv(x, z);
		return new SecP521R1FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] array = x;
		if (Nat.IsZero(17, array) || Nat.IsOne(17, array))
		{
			return this;
		}
		uint[] z = Nat.Create(17);
		uint[] array2 = Nat.Create(17);
		SecP521R1Field.SquareN(array, 519, z);
		SecP521R1Field.Square(z, array2);
		if (!Nat.Eq(17, array, array2))
		{
			return null;
		}
		return new SecP521R1FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecP521R1FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecP521R1FieldElement);
	}

	public virtual bool Equals(SecP521R1FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat.Eq(17, x, other.x);
	}

	public override int GetHashCode()
	{
		return Q.GetHashCode() ^ Arrays.GetHashCode(x, 0, 17);
	}
}
