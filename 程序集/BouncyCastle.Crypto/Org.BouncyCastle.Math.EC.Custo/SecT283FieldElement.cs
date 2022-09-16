using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT283FieldElement : AbstractF2mFieldElement
{
	protected internal readonly ulong[] x;

	public override bool IsOne => Nat320.IsOne64(x);

	public override bool IsZero => Nat320.IsZero64(x);

	public override string FieldName => "SecT283Field";

	public override int FieldSize => 283;

	public override bool HasFastTrace => true;

	public virtual int Representation => 3;

	public virtual int M => 283;

	public virtual int K1 => 5;

	public virtual int K2 => 7;

	public virtual int K3 => 12;

	public SecT283FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.BitLength > 283)
		{
			throw new ArgumentException("value invalid for SecT283FieldElement", "x");
		}
		this.x = SecT283Field.FromBigInteger(x);
	}

	public SecT283FieldElement()
	{
		x = Nat320.Create64();
	}

	protected internal SecT283FieldElement(ulong[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return (x[0] & 1) != 0;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat320.ToBigInteger64(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		ulong[] z = Nat320.Create64();
		SecT283Field.Add(x, ((SecT283FieldElement)b).x, z);
		return new SecT283FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		ulong[] z = Nat320.Create64();
		SecT283Field.AddOne(x, z);
		return new SecT283FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		return Add(b);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		ulong[] z = Nat320.Create64();
		SecT283Field.Multiply(x, ((SecT283FieldElement)b).x, z);
		return new SecT283FieldElement(z);
	}

	public override ECFieldElement MultiplyMinusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		return MultiplyPlusProduct(b, x, y);
	}

	public override ECFieldElement MultiplyPlusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] y2 = ((SecT283FieldElement)b).x;
		ulong[] array2 = ((SecT283FieldElement)x).x;
		ulong[] y3 = ((SecT283FieldElement)y).x;
		ulong[] array3 = Nat.Create64(9);
		SecT283Field.MultiplyAddToExt(array, y2, array3);
		SecT283Field.MultiplyAddToExt(array2, y3, array3);
		ulong[] z = Nat320.Create64();
		SecT283Field.Reduce(array3, z);
		return new SecT283FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		return Multiply(b.Invert());
	}

	public override ECFieldElement Negate()
	{
		return this;
	}

	public override ECFieldElement Square()
	{
		ulong[] z = Nat320.Create64();
		SecT283Field.Square(x, z);
		return new SecT283FieldElement(z);
	}

	public override ECFieldElement SquareMinusProduct(ECFieldElement x, ECFieldElement y)
	{
		return SquarePlusProduct(x, y);
	}

	public override ECFieldElement SquarePlusProduct(ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] array2 = ((SecT283FieldElement)x).x;
		ulong[] y2 = ((SecT283FieldElement)y).x;
		ulong[] array3 = Nat.Create64(9);
		SecT283Field.SquareAddToExt(array, array3);
		SecT283Field.MultiplyAddToExt(array2, y2, array3);
		ulong[] z = Nat320.Create64();
		SecT283Field.Reduce(array3, z);
		return new SecT283FieldElement(z);
	}

	public override ECFieldElement SquarePow(int pow)
	{
		if (pow < 1)
		{
			return this;
		}
		ulong[] z = Nat320.Create64();
		SecT283Field.SquareN(x, pow, z);
		return new SecT283FieldElement(z);
	}

	public override ECFieldElement HalfTrace()
	{
		ulong[] z = Nat320.Create64();
		SecT283Field.HalfTrace(x, z);
		return new SecT283FieldElement(z);
	}

	public override int Trace()
	{
		return (int)SecT283Field.Trace(x);
	}

	public override ECFieldElement Invert()
	{
		ulong[] z = Nat320.Create64();
		SecT283Field.Invert(x, z);
		return new SecT283FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		ulong[] z = Nat320.Create64();
		SecT283Field.Sqrt(x, z);
		return new SecT283FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecT283FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecT283FieldElement);
	}

	public virtual bool Equals(SecT283FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat320.Eq64(x, other.x);
	}

	public override int GetHashCode()
	{
		return 0x2B33AB ^ Arrays.GetHashCode(x, 0, 5);
	}
}
