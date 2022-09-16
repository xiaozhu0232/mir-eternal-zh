using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT131FieldElement : AbstractF2mFieldElement
{
	protected internal readonly ulong[] x;

	public override bool IsOne => Nat192.IsOne64(x);

	public override bool IsZero => Nat192.IsZero64(x);

	public override string FieldName => "SecT131Field";

	public override int FieldSize => 131;

	public override bool HasFastTrace => true;

	public virtual int Representation => 3;

	public virtual int M => 131;

	public virtual int K1 => 2;

	public virtual int K2 => 3;

	public virtual int K3 => 8;

	public SecT131FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.BitLength > 131)
		{
			throw new ArgumentException("value invalid for SecT131FieldElement", "x");
		}
		this.x = SecT131Field.FromBigInteger(x);
	}

	public SecT131FieldElement()
	{
		x = Nat192.Create64();
	}

	protected internal SecT131FieldElement(ulong[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return (x[0] & 1) != 0;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat192.ToBigInteger64(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		ulong[] z = Nat192.Create64();
		SecT131Field.Add(x, ((SecT131FieldElement)b).x, z);
		return new SecT131FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		ulong[] z = Nat192.Create64();
		SecT131Field.AddOne(x, z);
		return new SecT131FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		return Add(b);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		ulong[] z = Nat192.Create64();
		SecT131Field.Multiply(x, ((SecT131FieldElement)b).x, z);
		return new SecT131FieldElement(z);
	}

	public override ECFieldElement MultiplyMinusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		return MultiplyPlusProduct(b, x, y);
	}

	public override ECFieldElement MultiplyPlusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] y2 = ((SecT131FieldElement)b).x;
		ulong[] array2 = ((SecT131FieldElement)x).x;
		ulong[] y3 = ((SecT131FieldElement)y).x;
		ulong[] array3 = Nat.Create64(5);
		SecT131Field.MultiplyAddToExt(array, y2, array3);
		SecT131Field.MultiplyAddToExt(array2, y3, array3);
		ulong[] z = Nat192.Create64();
		SecT131Field.Reduce(array3, z);
		return new SecT131FieldElement(z);
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
		ulong[] z = Nat192.Create64();
		SecT131Field.Square(x, z);
		return new SecT131FieldElement(z);
	}

	public override ECFieldElement SquareMinusProduct(ECFieldElement x, ECFieldElement y)
	{
		return SquarePlusProduct(x, y);
	}

	public override ECFieldElement SquarePlusProduct(ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] array2 = ((SecT131FieldElement)x).x;
		ulong[] y2 = ((SecT131FieldElement)y).x;
		ulong[] array3 = Nat.Create64(5);
		SecT131Field.SquareAddToExt(array, array3);
		SecT131Field.MultiplyAddToExt(array2, y2, array3);
		ulong[] z = Nat192.Create64();
		SecT131Field.Reduce(array3, z);
		return new SecT131FieldElement(z);
	}

	public override ECFieldElement SquarePow(int pow)
	{
		if (pow < 1)
		{
			return this;
		}
		ulong[] z = Nat192.Create64();
		SecT131Field.SquareN(x, pow, z);
		return new SecT131FieldElement(z);
	}

	public override ECFieldElement HalfTrace()
	{
		ulong[] z = Nat192.Create64();
		SecT131Field.HalfTrace(x, z);
		return new SecT131FieldElement(z);
	}

	public override int Trace()
	{
		return (int)SecT131Field.Trace(x);
	}

	public override ECFieldElement Invert()
	{
		ulong[] z = Nat192.Create64();
		SecT131Field.Invert(x, z);
		return new SecT131FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		ulong[] z = Nat192.Create64();
		SecT131Field.Sqrt(x, z);
		return new SecT131FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecT131FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecT131FieldElement);
	}

	public virtual bool Equals(SecT131FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat192.Eq64(x, other.x);
	}

	public override int GetHashCode()
	{
		return 0x202F8 ^ Arrays.GetHashCode(x, 0, 3);
	}
}
