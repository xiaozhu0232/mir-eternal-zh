using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT163FieldElement : AbstractF2mFieldElement
{
	protected internal readonly ulong[] x;

	public override bool IsOne => Nat192.IsOne64(x);

	public override bool IsZero => Nat192.IsZero64(x);

	public override string FieldName => "SecT163Field";

	public override int FieldSize => 163;

	public override bool HasFastTrace => true;

	public virtual int Representation => 3;

	public virtual int M => 163;

	public virtual int K1 => 3;

	public virtual int K2 => 6;

	public virtual int K3 => 7;

	public SecT163FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.BitLength > 163)
		{
			throw new ArgumentException("value invalid for SecT163FieldElement", "x");
		}
		this.x = SecT163Field.FromBigInteger(x);
	}

	public SecT163FieldElement()
	{
		x = Nat192.Create64();
	}

	protected internal SecT163FieldElement(ulong[] x)
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
		SecT163Field.Add(x, ((SecT163FieldElement)b).x, z);
		return new SecT163FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		ulong[] z = Nat192.Create64();
		SecT163Field.AddOne(x, z);
		return new SecT163FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		return Add(b);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		ulong[] z = Nat192.Create64();
		SecT163Field.Multiply(x, ((SecT163FieldElement)b).x, z);
		return new SecT163FieldElement(z);
	}

	public override ECFieldElement MultiplyMinusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		return MultiplyPlusProduct(b, x, y);
	}

	public override ECFieldElement MultiplyPlusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] y2 = ((SecT163FieldElement)b).x;
		ulong[] array2 = ((SecT163FieldElement)x).x;
		ulong[] y3 = ((SecT163FieldElement)y).x;
		ulong[] array3 = Nat192.CreateExt64();
		SecT163Field.MultiplyAddToExt(array, y2, array3);
		SecT163Field.MultiplyAddToExt(array2, y3, array3);
		ulong[] z = Nat192.Create64();
		SecT163Field.Reduce(array3, z);
		return new SecT163FieldElement(z);
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
		SecT163Field.Square(x, z);
		return new SecT163FieldElement(z);
	}

	public override ECFieldElement SquareMinusProduct(ECFieldElement x, ECFieldElement y)
	{
		return SquarePlusProduct(x, y);
	}

	public override ECFieldElement SquarePlusProduct(ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] array2 = ((SecT163FieldElement)x).x;
		ulong[] y2 = ((SecT163FieldElement)y).x;
		ulong[] array3 = Nat192.CreateExt64();
		SecT163Field.SquareAddToExt(array, array3);
		SecT163Field.MultiplyAddToExt(array2, y2, array3);
		ulong[] z = Nat192.Create64();
		SecT163Field.Reduce(array3, z);
		return new SecT163FieldElement(z);
	}

	public override ECFieldElement SquarePow(int pow)
	{
		if (pow < 1)
		{
			return this;
		}
		ulong[] z = Nat192.Create64();
		SecT163Field.SquareN(x, pow, z);
		return new SecT163FieldElement(z);
	}

	public override ECFieldElement HalfTrace()
	{
		ulong[] z = Nat192.Create64();
		SecT163Field.HalfTrace(x, z);
		return new SecT163FieldElement(z);
	}

	public override int Trace()
	{
		return (int)SecT163Field.Trace(x);
	}

	public override ECFieldElement Invert()
	{
		ulong[] z = Nat192.Create64();
		SecT163Field.Invert(x, z);
		return new SecT163FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		ulong[] z = Nat192.Create64();
		SecT163Field.Sqrt(x, z);
		return new SecT163FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecT163FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecT163FieldElement);
	}

	public virtual bool Equals(SecT163FieldElement other)
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
		return 0x27FB3 ^ Arrays.GetHashCode(x, 0, 3);
	}
}
