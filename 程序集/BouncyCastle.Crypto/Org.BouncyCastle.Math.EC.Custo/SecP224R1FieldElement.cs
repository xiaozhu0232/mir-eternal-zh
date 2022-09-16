using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.Sec;

internal class SecP224R1FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000000000001"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat224.IsZero(x);

	public override bool IsOne => Nat224.IsOne(x);

	public override string FieldName => "SecP224R1Field";

	public override int FieldSize => Q.BitLength;

	public SecP224R1FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SecP224R1FieldElement", "x");
		}
		this.x = SecP224R1Field.FromBigInteger(x);
	}

	public SecP224R1FieldElement()
	{
		x = Nat224.Create();
	}

	protected internal SecP224R1FieldElement(uint[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return Nat224.GetBit(x, 0) == 1;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat224.ToBigInteger(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		uint[] z = Nat224.Create();
		SecP224R1Field.Add(x, ((SecP224R1FieldElement)b).x, z);
		return new SecP224R1FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat224.Create();
		SecP224R1Field.AddOne(x, z);
		return new SecP224R1FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat224.Create();
		SecP224R1Field.Subtract(x, ((SecP224R1FieldElement)b).x, z);
		return new SecP224R1FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat224.Create();
		SecP224R1Field.Multiply(x, ((SecP224R1FieldElement)b).x, z);
		return new SecP224R1FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat224.Create();
		SecP224R1Field.Inv(((SecP224R1FieldElement)b).x, z);
		SecP224R1Field.Multiply(z, x, z);
		return new SecP224R1FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat224.Create();
		SecP224R1Field.Negate(x, z);
		return new SecP224R1FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat224.Create();
		SecP224R1Field.Square(x, z);
		return new SecP224R1FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat224.Create();
		SecP224R1Field.Inv(x, z);
		return new SecP224R1FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] array = x;
		if (Nat224.IsZero(array) || Nat224.IsOne(array))
		{
			return this;
		}
		uint[] array2 = Nat224.Create();
		SecP224R1Field.Negate(array, array2);
		uint[] array3 = Mod.Random(SecP224R1Field.P);
		uint[] t = Nat224.Create();
		if (!IsSquare(array))
		{
			return null;
		}
		while (!TrySqrt(array2, array3, t))
		{
			SecP224R1Field.AddOne(array3, array3);
		}
		SecP224R1Field.Square(t, array3);
		if (!Nat224.Eq(array, array3))
		{
			return null;
		}
		return new SecP224R1FieldElement(t);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SecP224R1FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SecP224R1FieldElement);
	}

	public virtual bool Equals(SecP224R1FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat224.Eq(x, other.x);
	}

	public override int GetHashCode()
	{
		return Q.GetHashCode() ^ Arrays.GetHashCode(x, 0, 7);
	}

	private static bool IsSquare(uint[] x)
	{
		uint[] z = Nat224.Create();
		uint[] array = Nat224.Create();
		Nat224.Copy(x, z);
		for (int i = 0; i < 7; i++)
		{
			Nat224.Copy(z, array);
			SecP224R1Field.SquareN(z, 1 << i, z);
			SecP224R1Field.Multiply(z, array, z);
		}
		SecP224R1Field.SquareN(z, 95, z);
		return Nat224.IsOne(z);
	}

	private static void RM(uint[] nc, uint[] d0, uint[] e0, uint[] d1, uint[] e1, uint[] f1, uint[] t)
	{
		SecP224R1Field.Multiply(e1, e0, t);
		SecP224R1Field.Multiply(t, nc, t);
		SecP224R1Field.Multiply(d1, d0, f1);
		SecP224R1Field.Add(f1, t, f1);
		SecP224R1Field.Multiply(d1, e0, t);
		Nat224.Copy(f1, d1);
		SecP224R1Field.Multiply(e1, d0, e1);
		SecP224R1Field.Add(e1, t, e1);
		SecP224R1Field.Square(e1, f1);
		SecP224R1Field.Multiply(f1, nc, f1);
	}

	private static void RP(uint[] nc, uint[] d1, uint[] e1, uint[] f1, uint[] t)
	{
		Nat224.Copy(nc, f1);
		uint[] array = Nat224.Create();
		uint[] array2 = Nat224.Create();
		for (int i = 0; i < 7; i++)
		{
			Nat224.Copy(d1, array);
			Nat224.Copy(e1, array2);
			int num = 1 << i;
			while (--num >= 0)
			{
				RS(d1, e1, f1, t);
			}
			RM(nc, array, array2, d1, e1, f1, t);
		}
	}

	private static void RS(uint[] d, uint[] e, uint[] f, uint[] t)
	{
		SecP224R1Field.Multiply(e, d, e);
		SecP224R1Field.Twice(e, e);
		SecP224R1Field.Square(d, t);
		SecP224R1Field.Add(f, t, d);
		SecP224R1Field.Multiply(f, t, f);
		uint num = Nat.ShiftUpBits(7, f, 2, 0u);
		SecP224R1Field.Reduce32(num, f);
	}

	private static bool TrySqrt(uint[] nc, uint[] r, uint[] t)
	{
		uint[] array = Nat224.Create();
		Nat224.Copy(r, array);
		uint[] array2 = Nat224.Create();
		array2[0] = 1u;
		uint[] array3 = Nat224.Create();
		RP(nc, array, array2, array3, t);
		uint[] array4 = Nat224.Create();
		uint[] z = Nat224.Create();
		for (int i = 1; i < 96; i++)
		{
			Nat224.Copy(array, array4);
			Nat224.Copy(array2, z);
			RS(array, array2, array3, t);
			if (Nat224.IsZero(array))
			{
				SecP224R1Field.Inv(z, t);
				SecP224R1Field.Multiply(t, array4, t);
				return true;
			}
		}
		return false;
	}
}
