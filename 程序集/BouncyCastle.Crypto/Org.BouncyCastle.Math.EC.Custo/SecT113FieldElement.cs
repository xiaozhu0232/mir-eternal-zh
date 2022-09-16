using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT113FieldElement : AbstractF2mFieldElement
{
	protected internal readonly ulong[] x;

	public override bool IsOne => Nat128.IsOne64(x);

	public override bool IsZero => Nat128.IsZero64(x);

	public override string FieldName => "SecT113Field";

	public override int FieldSize => 113;

	public override bool HasFastTrace => true;

	public virtual int Representation => 2;

	public virtual int M => 113;

	public virtual int K1 => 9;

	public virtual int K2 => 0;

	public virtual int K3 => 0;

	public SecT113FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.BitLength > 113)
		{
			throw new ArgumentException("value invalid for SecT113FieldElement", "x");
		}
		this.x = SecT113Field.FromBigInteger(x);
	}

	public SecT113FieldElement()
	{
		x = Nat128.Create64();
	}

	protected internal SecT113FieldElement(ulong[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return (x[0] & 1) != 0;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat128.ToBigInteger64(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		ulong[] z = Nat128.Create64();
		SecT113Field.Add(x, ((SecT113FieldElement)b).x, z);
		return new SecT113FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		ulong[] z = Nat128.Create64();
		SecT113Field.AddOne(x, z);
		return new SecT113FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		return Add(b);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		ulong[] z = Nat128.Create64();
		SecT113Field.Multiply(x, ((SecT113FieldElement)b).x, z);
		return new SecT113FieldElement(z);
	}

	public override ECFieldElement MultiplyMinusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		return MultiplyPlusProduct(b, x, y);
	}

	public override ECFieldElement MultiplyPlusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] y2 = ((SecT113FieldElement)b).x;
		ulong[] array2 = ((SecT113FieldElement)x).x;
		ulong[] y3 = ((SecT113FieldElement)y).x;
		ulong[] array3 = Nat128.CreateExt64();
		SecT113Field.MultiplyAddToExt(array, y2, array3);
		SecT113Field.MultiplyAddToExt(array2, y3, array3);
		ulong[] z = Nat128.Create64();
		SecT113Field.Reduce(array3, z);
		return new SecT113FieldElement(z);
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
		ulong[] z = Nat128.Create64();
		SecT113Field.Square(x, z);
		return new SecT113FieldElement(z);
	}

	public override ECFieldElement SquareMinusProduct(ECFieldElement x, ECFieldElement y)
	{
		return SquarePlusProduct(x, y);
	}

	public override ECFieldElement SquarePlusProduct(ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] array2 = ((SecT113FieldElement)x).x;
		ulong[] y2 = ((SecT113FieldElement)y).x;
		ulong[] array3 = Nat128.CreateExt64();
		SecT113Field.SquareAddToExt(array, array3);
		SecT113Field.MultiplyAddToExt(array2, y2, array3);
		ulong[] z = Nat128.Create64();
		SecT113Field.Reduce(array3, z);
		return new SecT113FieldElement(z);
	}

	public override ECFieldElement SquarePow(int pow)
	{
		if (pow < 1)
		{
			return this;
		}
		ulong[] z = Nat128.Create64();
		SecT113Field.SquareN(x, pow, z);
		return new SecT113FieldElement(z);
	}

	public override ECFieldElement HalfTrace()
	{
		ulong[] z = Nat128.Create64();
		SecT113Field.HalfTrace(x, z);
		return new SecT113FieldElement(z);
	}

	public override int Trace()
	{
		return (int)SecT113Field.Trace(x);
	}

	public override ECFieldElement Invert()
	{
		ulong[] z = Nat128.Create64();
		SecT113Field.Invert(x, z);
		return new SecT113FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		ulong[] z = Nat128.Create64();
		SecT113Field.Sqrt(x, z);
		return new SecT113FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecT113FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecT113FieldElement);
	}

	public virtual bool Equals(SecT113FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat128.Eq64(x, other.x);
	}

	public override int GetHashCode()
	{
		return 0x1B971 ^ Arrays.GetHashCode(x, 0, 2);
	}
}
