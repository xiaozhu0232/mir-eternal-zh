using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT409FieldElement : AbstractF2mFieldElement
{
	protected internal readonly ulong[] x;

	public override bool IsOne => Nat448.IsOne64(x);

	public override bool IsZero => Nat448.IsZero64(x);

	public override string FieldName => "SecT409Field";

	public override int FieldSize => 409;

	public override bool HasFastTrace => true;

	public virtual int Representation => 2;

	public virtual int M => 409;

	public virtual int K1 => 87;

	public virtual int K2 => 0;

	public virtual int K3 => 0;

	public SecT409FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.BitLength > 409)
		{
			throw new ArgumentException("value invalid for SecT409FieldElement", "x");
		}
		this.x = SecT409Field.FromBigInteger(x);
	}

	public SecT409FieldElement()
	{
		x = Nat448.Create64();
	}

	protected internal SecT409FieldElement(ulong[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return (x[0] & 1) != 0;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat448.ToBigInteger64(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		ulong[] z = Nat448.Create64();
		SecT409Field.Add(x, ((SecT409FieldElement)b).x, z);
		return new SecT409FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		ulong[] z = Nat448.Create64();
		SecT409Field.AddOne(x, z);
		return new SecT409FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		return Add(b);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		ulong[] z = Nat448.Create64();
		SecT409Field.Multiply(x, ((SecT409FieldElement)b).x, z);
		return new SecT409FieldElement(z);
	}

	public override ECFieldElement MultiplyMinusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		return MultiplyPlusProduct(b, x, y);
	}

	public override ECFieldElement MultiplyPlusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] y2 = ((SecT409FieldElement)b).x;
		ulong[] array2 = ((SecT409FieldElement)x).x;
		ulong[] y3 = ((SecT409FieldElement)y).x;
		ulong[] array3 = Nat.Create64(13);
		SecT409Field.MultiplyAddToExt(array, y2, array3);
		SecT409Field.MultiplyAddToExt(array2, y3, array3);
		ulong[] z = Nat448.Create64();
		SecT409Field.Reduce(array3, z);
		return new SecT409FieldElement(z);
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
		ulong[] z = Nat448.Create64();
		SecT409Field.Square(x, z);
		return new SecT409FieldElement(z);
	}

	public override ECFieldElement SquareMinusProduct(ECFieldElement x, ECFieldElement y)
	{
		return SquarePlusProduct(x, y);
	}

	public override ECFieldElement SquarePlusProduct(ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] array2 = ((SecT409FieldElement)x).x;
		ulong[] y2 = ((SecT409FieldElement)y).x;
		ulong[] array3 = Nat.Create64(13);
		SecT409Field.SquareAddToExt(array, array3);
		SecT409Field.MultiplyAddToExt(array2, y2, array3);
		ulong[] z = Nat448.Create64();
		SecT409Field.Reduce(array3, z);
		return new SecT409FieldElement(z);
	}

	public override ECFieldElement SquarePow(int pow)
	{
		if (pow < 1)
		{
			return this;
		}
		ulong[] z = Nat448.Create64();
		SecT409Field.SquareN(x, pow, z);
		return new SecT409FieldElement(z);
	}

	public override ECFieldElement HalfTrace()
	{
		ulong[] z = Nat448.Create64();
		SecT409Field.HalfTrace(x, z);
		return new SecT409FieldElement(z);
	}

	public override int Trace()
	{
		return (int)SecT409Field.Trace(x);
	}

	public override ECFieldElement Invert()
	{
		ulong[] z = Nat448.Create64();
		SecT409Field.Invert(x, z);
		return new SecT409FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		ulong[] z = Nat448.Create64();
		SecT409Field.Sqrt(x, z);
		return new SecT409FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecT409FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecT409FieldElement);
	}

	public virtual bool Equals(SecT409FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat448.Eq64(x, other.x);
	}

	public override int GetHashCode()
	{
		return 0x3E68E7 ^ Arrays.GetHashCode(x, 0, 7);
	}
}
