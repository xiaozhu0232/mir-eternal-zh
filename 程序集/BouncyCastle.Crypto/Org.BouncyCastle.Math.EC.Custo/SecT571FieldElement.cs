using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecT571FieldElement : AbstractF2mFieldElement
{
	protected internal readonly ulong[] x;

	public override bool IsOne => Nat576.IsOne64(x);

	public override bool IsZero => Nat576.IsZero64(x);

	public override string FieldName => "SecT571Field";

	public override int FieldSize => 571;

	public override bool HasFastTrace => true;

	public virtual int Representation => 3;

	public virtual int M => 571;

	public virtual int K1 => 2;

	public virtual int K2 => 5;

	public virtual int K3 => 10;

	public SecT571FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.BitLength > 571)
		{
			throw new ArgumentException("value invalid for SecT571FieldElement", "x");
		}
		this.x = SecT571Field.FromBigInteger(x);
	}

	public SecT571FieldElement()
	{
		x = Nat576.Create64();
	}

	protected internal SecT571FieldElement(ulong[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return (x[0] & 1) != 0;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat576.ToBigInteger64(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		ulong[] z = Nat576.Create64();
		SecT571Field.Add(x, ((SecT571FieldElement)b).x, z);
		return new SecT571FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		ulong[] z = Nat576.Create64();
		SecT571Field.AddOne(x, z);
		return new SecT571FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		return Add(b);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		ulong[] z = Nat576.Create64();
		SecT571Field.Multiply(x, ((SecT571FieldElement)b).x, z);
		return new SecT571FieldElement(z);
	}

	public override ECFieldElement MultiplyMinusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		return MultiplyPlusProduct(b, x, y);
	}

	public override ECFieldElement MultiplyPlusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] y2 = ((SecT571FieldElement)b).x;
		ulong[] array2 = ((SecT571FieldElement)x).x;
		ulong[] y3 = ((SecT571FieldElement)y).x;
		ulong[] array3 = Nat576.CreateExt64();
		SecT571Field.MultiplyAddToExt(array, y2, array3);
		SecT571Field.MultiplyAddToExt(array2, y3, array3);
		ulong[] z = Nat576.Create64();
		SecT571Field.Reduce(array3, z);
		return new SecT571FieldElement(z);
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
		ulong[] z = Nat576.Create64();
		SecT571Field.Square(x, z);
		return new SecT571FieldElement(z);
	}

	public override ECFieldElement SquareMinusProduct(ECFieldElement x, ECFieldElement y)
	{
		return SquarePlusProduct(x, y);
	}

	public override ECFieldElement SquarePlusProduct(ECFieldElement x, ECFieldElement y)
	{
		ulong[] array = this.x;
		ulong[] array2 = ((SecT571FieldElement)x).x;
		ulong[] y2 = ((SecT571FieldElement)y).x;
		ulong[] array3 = Nat576.CreateExt64();
		SecT571Field.SquareAddToExt(array, array3);
		SecT571Field.MultiplyAddToExt(array2, y2, array3);
		ulong[] z = Nat576.Create64();
		SecT571Field.Reduce(array3, z);
		return new SecT571FieldElement(z);
	}

	public override ECFieldElement SquarePow(int pow)
	{
		if (pow < 1)
		{
			return this;
		}
		ulong[] z = Nat576.Create64();
		SecT571Field.SquareN(x, pow, z);
		return new SecT571FieldElement(z);
	}

	public override ECFieldElement HalfTrace()
	{
		ulong[] z = Nat576.Create64();
		SecT571Field.HalfTrace(x, z);
		return new SecT571FieldElement(z);
	}

	public override int Trace()
	{
		return (int)SecT571Field.Trace(x);
	}

	public override ECFieldElement Invert()
	{
		ulong[] z = Nat576.Create64();
		SecT571Field.Invert(x, z);
		return new SecT571FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		ulong[] z = Nat576.Create64();
		SecT571Field.Sqrt(x, z);
		return new SecT571FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecT571FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecT571FieldElement);
	}

	public virtual bool Equals(SecT571FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat576.Eq64(x, other.x);
	}

	public override int GetHashCode()
	{
		return 0x5724CC ^ Arrays.GetHashCode(x, 0, 9);
	}
}
