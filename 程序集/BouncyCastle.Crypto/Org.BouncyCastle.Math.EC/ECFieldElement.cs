using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC;

public abstract class ECFieldElement
{
	public abstract string FieldName { get; }

	public abstract int FieldSize { get; }

	public virtual int BitLength => ToBigInteger().BitLength;

	public virtual bool IsOne => BitLength == 1;

	public virtual bool IsZero => 0 == ToBigInteger().SignValue;

	public abstract BigInteger ToBigInteger();

	public abstract ECFieldElement Add(ECFieldElement b);

	public abstract ECFieldElement AddOne();

	public abstract ECFieldElement Subtract(ECFieldElement b);

	public abstract ECFieldElement Multiply(ECFieldElement b);

	public abstract ECFieldElement Divide(ECFieldElement b);

	public abstract ECFieldElement Negate();

	public abstract ECFieldElement Square();

	public abstract ECFieldElement Invert();

	public abstract ECFieldElement Sqrt();

	public virtual ECFieldElement MultiplyMinusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		return Multiply(b).Subtract(x.Multiply(y));
	}

	public virtual ECFieldElement MultiplyPlusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		return Multiply(b).Add(x.Multiply(y));
	}

	public virtual ECFieldElement SquareMinusProduct(ECFieldElement x, ECFieldElement y)
	{
		return Square().Subtract(x.Multiply(y));
	}

	public virtual ECFieldElement SquarePlusProduct(ECFieldElement x, ECFieldElement y)
	{
		return Square().Add(x.Multiply(y));
	}

	public virtual ECFieldElement SquarePow(int pow)
	{
		ECFieldElement eCFieldElement = this;
		for (int i = 0; i < pow; i++)
		{
			eCFieldElement = eCFieldElement.Square();
		}
		return eCFieldElement;
	}

	public virtual bool TestBitZero()
	{
		return ToBigInteger().TestBit(0);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ECFieldElement);
	}

	public virtual bool Equals(ECFieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return ToBigInteger().Equals(other.ToBigInteger());
	}

	public override int GetHashCode()
	{
		return ToBigInteger().GetHashCode();
	}

	public override string ToString()
	{
		return ToBigInteger().ToString(16);
	}

	public virtual byte[] GetEncoded()
	{
		return BigIntegers.AsUnsignedByteArray((FieldSize + 7) / 8, ToBigInteger());
	}
}
