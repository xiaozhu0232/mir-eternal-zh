using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC;

public class FpFieldElement : AbstractFpFieldElement
{
	private readonly BigInteger q;

	private readonly BigInteger r;

	private readonly BigInteger x;

	public override string FieldName => "Fp";

	public override int FieldSize => q.BitLength;

	public BigInteger Q => q;

	internal static BigInteger CalculateResidue(BigInteger p)
	{
		int bitLength = p.BitLength;
		if (bitLength >= 96)
		{
			BigInteger bigInteger = p.ShiftRight(bitLength - 64);
			if (bigInteger.LongValue == -1)
			{
				return BigInteger.One.ShiftLeft(bitLength).Subtract(p);
			}
			if ((bitLength & 7) == 0)
			{
				return BigInteger.One.ShiftLeft(bitLength << 1).Divide(p).Negate();
			}
		}
		return null;
	}

	[Obsolete("Use ECCurve.FromBigInteger to construct field elements")]
	public FpFieldElement(BigInteger q, BigInteger x)
		: this(q, CalculateResidue(q), x)
	{
	}

	internal FpFieldElement(BigInteger q, BigInteger r, BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(q) >= 0)
		{
			throw new ArgumentException("value invalid in Fp field element", "x");
		}
		this.q = q;
		this.r = r;
		this.x = x;
	}

	public override BigInteger ToBigInteger()
	{
		return x;
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		return new FpFieldElement(q, r, ModAdd(x, b.ToBigInteger()));
	}

	public override ECFieldElement AddOne()
	{
		BigInteger bigInteger = x.Add(BigInteger.One);
		if (bigInteger.CompareTo(q) == 0)
		{
			bigInteger = BigInteger.Zero;
		}
		return new FpFieldElement(q, r, bigInteger);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		return new FpFieldElement(q, r, ModSubtract(x, b.ToBigInteger()));
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		return new FpFieldElement(q, r, ModMult(x, b.ToBigInteger()));
	}

	public override ECFieldElement MultiplyMinusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		BigInteger bigInteger = this.x;
		BigInteger val = b.ToBigInteger();
		BigInteger bigInteger2 = x.ToBigInteger();
		BigInteger val2 = y.ToBigInteger();
		BigInteger bigInteger3 = bigInteger.Multiply(val);
		BigInteger n = bigInteger2.Multiply(val2);
		return new FpFieldElement(q, r, ModReduce(bigInteger3.Subtract(n)));
	}

	public override ECFieldElement MultiplyPlusProduct(ECFieldElement b, ECFieldElement x, ECFieldElement y)
	{
		BigInteger bigInteger = this.x;
		BigInteger val = b.ToBigInteger();
		BigInteger bigInteger2 = x.ToBigInteger();
		BigInteger val2 = y.ToBigInteger();
		BigInteger bigInteger3 = bigInteger.Multiply(val);
		BigInteger value = bigInteger2.Multiply(val2);
		BigInteger bigInteger4 = bigInteger3.Add(value);
		if (r != null && r.SignValue < 0 && bigInteger4.BitLength > q.BitLength << 1)
		{
			bigInteger4 = bigInteger4.Subtract(q.ShiftLeft(q.BitLength));
		}
		return new FpFieldElement(q, r, ModReduce(bigInteger4));
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		return new FpFieldElement(q, r, ModMult(x, ModInverse(b.ToBigInteger())));
	}

	public override ECFieldElement Negate()
	{
		if (x.SignValue != 0)
		{
			return new FpFieldElement(q, r, q.Subtract(x));
		}
		return this;
	}

	public override ECFieldElement Square()
	{
		return new FpFieldElement(q, r, ModMult(x, x));
	}

	public override ECFieldElement SquareMinusProduct(ECFieldElement x, ECFieldElement y)
	{
		BigInteger bigInteger = this.x;
		BigInteger bigInteger2 = x.ToBigInteger();
		BigInteger val = y.ToBigInteger();
		BigInteger bigInteger3 = bigInteger.Multiply(bigInteger);
		BigInteger n = bigInteger2.Multiply(val);
		return new FpFieldElement(q, r, ModReduce(bigInteger3.Subtract(n)));
	}

	public override ECFieldElement SquarePlusProduct(ECFieldElement x, ECFieldElement y)
	{
		BigInteger bigInteger = this.x;
		BigInteger bigInteger2 = x.ToBigInteger();
		BigInteger val = y.ToBigInteger();
		BigInteger bigInteger3 = bigInteger.Multiply(bigInteger);
		BigInteger value = bigInteger2.Multiply(val);
		BigInteger bigInteger4 = bigInteger3.Add(value);
		if (r != null && r.SignValue < 0 && bigInteger4.BitLength > q.BitLength << 1)
		{
			bigInteger4 = bigInteger4.Subtract(q.ShiftLeft(q.BitLength));
		}
		return new FpFieldElement(q, r, ModReduce(bigInteger4));
	}

	public override ECFieldElement Invert()
	{
		return new FpFieldElement(q, r, ModInverse(x));
	}

	public override ECFieldElement Sqrt()
	{
		if (IsZero || IsOne)
		{
			return this;
		}
		if (!q.TestBit(0))
		{
			throw Platform.CreateNotImplementedException("even value of q");
		}
		if (q.TestBit(1))
		{
			BigInteger e = q.ShiftRight(2).Add(BigInteger.One);
			return CheckSqrt(new FpFieldElement(q, r, this.x.ModPow(e, q)));
		}
		if (q.TestBit(2))
		{
			BigInteger bigInteger = this.x.ModPow(q.ShiftRight(3), q);
			BigInteger x = ModMult(bigInteger, this.x);
			BigInteger bigInteger2 = ModMult(x, bigInteger);
			if (bigInteger2.Equals(BigInteger.One))
			{
				return CheckSqrt(new FpFieldElement(q, r, x));
			}
			BigInteger x2 = BigInteger.Two.ModPow(q.ShiftRight(2), q);
			BigInteger bigInteger3 = ModMult(x, x2);
			return CheckSqrt(new FpFieldElement(q, r, bigInteger3));
		}
		BigInteger bigInteger4 = q.ShiftRight(1);
		if (!this.x.ModPow(bigInteger4, q).Equals(BigInteger.One))
		{
			return null;
		}
		BigInteger bigInteger5 = this.x;
		BigInteger bigInteger6 = ModDouble(ModDouble(bigInteger5));
		BigInteger k = bigInteger4.Add(BigInteger.One);
		BigInteger obj = q.Subtract(BigInteger.One);
		while (true)
		{
			BigInteger bigInteger7 = BigInteger.Arbitrary(q.BitLength);
			if (bigInteger7.CompareTo(q) < 0 && ModReduce(bigInteger7.Multiply(bigInteger7).Subtract(bigInteger6)).ModPow(bigInteger4, q).Equals(obj))
			{
				BigInteger[] array = LucasSequence(bigInteger7, bigInteger5, k);
				BigInteger bigInteger8 = array[0];
				BigInteger bigInteger9 = array[1];
				if (ModMult(bigInteger9, bigInteger9).Equals(bigInteger6))
				{
					return new FpFieldElement(q, r, ModHalfAbs(bigInteger9));
				}
				if (!bigInteger8.Equals(BigInteger.One) && !bigInteger8.Equals(obj))
				{
					break;
				}
			}
		}
		return null;
	}

	private ECFieldElement CheckSqrt(ECFieldElement z)
	{
		if (!z.Square().Equals(this))
		{
			return null;
		}
		return z;
	}

	private BigInteger[] LucasSequence(BigInteger P, BigInteger Q, BigInteger k)
	{
		int bitLength = k.BitLength;
		int lowestSetBit = k.GetLowestSetBit();
		BigInteger bigInteger = BigInteger.One;
		BigInteger bigInteger2 = BigInteger.Two;
		BigInteger bigInteger3 = P;
		BigInteger bigInteger4 = BigInteger.One;
		BigInteger bigInteger5 = BigInteger.One;
		for (int num = bitLength - 1; num >= lowestSetBit + 1; num--)
		{
			bigInteger4 = ModMult(bigInteger4, bigInteger5);
			if (k.TestBit(num))
			{
				bigInteger5 = ModMult(bigInteger4, Q);
				bigInteger = ModMult(bigInteger, bigInteger3);
				bigInteger2 = ModReduce(bigInteger3.Multiply(bigInteger2).Subtract(P.Multiply(bigInteger4)));
				bigInteger3 = ModReduce(bigInteger3.Multiply(bigInteger3).Subtract(bigInteger5.ShiftLeft(1)));
			}
			else
			{
				bigInteger5 = bigInteger4;
				bigInteger = ModReduce(bigInteger.Multiply(bigInteger2).Subtract(bigInteger4));
				bigInteger3 = ModReduce(bigInteger3.Multiply(bigInteger2).Subtract(P.Multiply(bigInteger4)));
				bigInteger2 = ModReduce(bigInteger2.Multiply(bigInteger2).Subtract(bigInteger4.ShiftLeft(1)));
			}
		}
		bigInteger4 = ModMult(bigInteger4, bigInteger5);
		bigInteger5 = ModMult(bigInteger4, Q);
		bigInteger = ModReduce(bigInteger.Multiply(bigInteger2).Subtract(bigInteger4));
		bigInteger2 = ModReduce(bigInteger3.Multiply(bigInteger2).Subtract(P.Multiply(bigInteger4)));
		bigInteger4 = ModMult(bigInteger4, bigInteger5);
		for (int i = 1; i <= lowestSetBit; i++)
		{
			bigInteger = ModMult(bigInteger, bigInteger2);
			bigInteger2 = ModReduce(bigInteger2.Multiply(bigInteger2).Subtract(bigInteger4.ShiftLeft(1)));
			bigInteger4 = ModMult(bigInteger4, bigInteger4);
		}
		return new BigInteger[2] { bigInteger, bigInteger2 };
	}

	protected virtual BigInteger ModAdd(BigInteger x1, BigInteger x2)
	{
		BigInteger bigInteger = x1.Add(x2);
		if (bigInteger.CompareTo(q) >= 0)
		{
			bigInteger = bigInteger.Subtract(q);
		}
		return bigInteger;
	}

	protected virtual BigInteger ModDouble(BigInteger x)
	{
		BigInteger bigInteger = x.ShiftLeft(1);
		if (bigInteger.CompareTo(q) >= 0)
		{
			bigInteger = bigInteger.Subtract(q);
		}
		return bigInteger;
	}

	protected virtual BigInteger ModHalf(BigInteger x)
	{
		if (x.TestBit(0))
		{
			x = q.Add(x);
		}
		return x.ShiftRight(1);
	}

	protected virtual BigInteger ModHalfAbs(BigInteger x)
	{
		if (x.TestBit(0))
		{
			x = q.Subtract(x);
		}
		return x.ShiftRight(1);
	}

	protected virtual BigInteger ModInverse(BigInteger x)
	{
		return BigIntegers.ModOddInverse(q, x);
	}

	protected virtual BigInteger ModMult(BigInteger x1, BigInteger x2)
	{
		return ModReduce(x1.Multiply(x2));
	}

	protected virtual BigInteger ModReduce(BigInteger x)
	{
		if (r == null)
		{
			x = x.Mod(q);
		}
		else
		{
			bool flag = x.SignValue < 0;
			if (flag)
			{
				x = x.Abs();
			}
			int bitLength = q.BitLength;
			if (r.SignValue > 0)
			{
				BigInteger n = BigInteger.One.ShiftLeft(bitLength);
				bool flag2 = r.Equals(BigInteger.One);
				while (x.BitLength > bitLength + 1)
				{
					BigInteger bigInteger = x.ShiftRight(bitLength);
					BigInteger value = x.Remainder(n);
					if (!flag2)
					{
						bigInteger = bigInteger.Multiply(r);
					}
					x = bigInteger.Add(value);
				}
			}
			else
			{
				int num = ((bitLength - 1) & 0x1F) + 1;
				BigInteger bigInteger2 = r.Negate();
				BigInteger bigInteger3 = bigInteger2.Multiply(x.ShiftRight(bitLength - num));
				BigInteger bigInteger4 = bigInteger3.ShiftRight(bitLength + num);
				BigInteger bigInteger5 = bigInteger4.Multiply(q);
				BigInteger bigInteger6 = BigInteger.One.ShiftLeft(bitLength + num);
				bigInteger5 = bigInteger5.Remainder(bigInteger6);
				x = x.Remainder(bigInteger6);
				x = x.Subtract(bigInteger5);
				if (x.SignValue < 0)
				{
					x = x.Add(bigInteger6);
				}
			}
			while (x.CompareTo(q) >= 0)
			{
				x = x.Subtract(q);
			}
			if (flag && x.SignValue != 0)
			{
				x = q.Subtract(x);
			}
		}
		return x;
	}

	protected virtual BigInteger ModSubtract(BigInteger x1, BigInteger x2)
	{
		BigInteger bigInteger = x1.Subtract(x2);
		if (bigInteger.SignValue < 0)
		{
			bigInteger = bigInteger.Add(q);
		}
		return bigInteger;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is FpFieldElement other))
		{
			return false;
		}
		return Equals(other);
	}

	public virtual bool Equals(FpFieldElement other)
	{
		if (q.Equals(other.q))
		{
			return base.Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return q.GetHashCode() ^ base.GetHashCode();
	}
}
