using System;
using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Math.EC.Custom.GM;

internal class SM2P256V1FieldElement : AbstractFpFieldElement
{
	public static readonly BigInteger Q = new BigInteger(1, Hex.DecodeStrict("FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFF"));

	protected internal readonly uint[] x;

	public override bool IsZero => Nat256.IsZero(x);

	public override bool IsOne => Nat256.IsOne(x);

	public override string FieldName => "SM2P256V1Field";

	public override int FieldSize => Q.BitLength;

	public SM2P256V1FieldElement(BigInteger x)
	{
		if (x == null || x.SignValue < 0 || x.CompareTo(Q) >= 0)
		{
			throw new ArgumentException("value invalid for SM2P256V1FieldElement", "x");
		}
		this.x = SM2P256V1Field.FromBigInteger(x);
	}

	public SM2P256V1FieldElement()
	{
		x = Nat256.Create();
	}

	protected internal SM2P256V1FieldElement(uint[] x)
	{
		this.x = x;
	}

	public override bool TestBitZero()
	{
		return Nat256.GetBit(x, 0) == 1;
	}

	public override BigInteger ToBigInteger()
	{
		return Nat256.ToBigInteger(x);
	}

	public override ECFieldElement Add(ECFieldElement b)
	{
		uint[] z = Nat256.Create();
		SM2P256V1Field.Add(x, ((SM2P256V1FieldElement)b).x, z);
		return new SM2P256V1FieldElement(z);
	}

	public override ECFieldElement AddOne()
	{
		uint[] z = Nat256.Create();
		SM2P256V1Field.AddOne(x, z);
		return new SM2P256V1FieldElement(z);
	}

	public override ECFieldElement Subtract(ECFieldElement b)
	{
		uint[] z = Nat256.Create();
		SM2P256V1Field.Subtract(x, ((SM2P256V1FieldElement)b).x, z);
		return new SM2P256V1FieldElement(z);
	}

	public override ECFieldElement Multiply(ECFieldElement b)
	{
		uint[] z = Nat256.Create();
		SM2P256V1Field.Multiply(x, ((SM2P256V1FieldElement)b).x, z);
		return new SM2P256V1FieldElement(z);
	}

	public override ECFieldElement Divide(ECFieldElement b)
	{
		uint[] z = Nat256.Create();
		SM2P256V1Field.Inv(((SM2P256V1FieldElement)b).x, z);
		SM2P256V1Field.Multiply(z, x, z);
		return new SM2P256V1FieldElement(z);
	}

	public override ECFieldElement Negate()
	{
		uint[] z = Nat256.Create();
		SM2P256V1Field.Negate(x, z);
		return new SM2P256V1FieldElement(z);
	}

	public override ECFieldElement Square()
	{
		uint[] z = Nat256.Create();
		SM2P256V1Field.Square(x, z);
		return new SM2P256V1FieldElement(z);
	}

	public override ECFieldElement Invert()
	{
		uint[] z = Nat256.Create();
		SM2P256V1Field.Inv(x, z);
		return new SM2P256V1FieldElement(z);
	}

	public override ECFieldElement Sqrt()
	{
		uint[] y = x;
		if (Nat256.IsZero(y) || Nat256.IsOne(y))
		{
			return this;
		}
		uint[] array = Nat256.Create();
		SM2P256V1Field.Square(y, array);
		SM2P256V1Field.Multiply(array, y, array);
		uint[] array2 = Nat256.Create();
		SM2P256V1Field.SquareN(array, 2, array2);
		SM2P256V1Field.Multiply(array2, array, array2);
		uint[] array3 = Nat256.Create();
		SM2P256V1Field.SquareN(array2, 2, array3);
		SM2P256V1Field.Multiply(array3, array, array3);
		uint[] array4 = array;
		SM2P256V1Field.SquareN(array3, 6, array4);
		SM2P256V1Field.Multiply(array4, array3, array4);
		uint[] array5 = Nat256.Create();
		SM2P256V1Field.SquareN(array4, 12, array5);
		SM2P256V1Field.Multiply(array5, array4, array5);
		uint[] array6 = array4;
		SM2P256V1Field.SquareN(array5, 6, array6);
		SM2P256V1Field.Multiply(array6, array3, array6);
		uint[] array7 = array3;
		SM2P256V1Field.Square(array6, array7);
		SM2P256V1Field.Multiply(array7, y, array7);
		uint[] z = array5;
		SM2P256V1Field.SquareN(array7, 31, z);
		uint[] array8 = array6;
		SM2P256V1Field.Multiply(z, array7, array8);
		SM2P256V1Field.SquareN(z, 32, z);
		SM2P256V1Field.Multiply(z, array8, z);
		SM2P256V1Field.SquareN(z, 62, z);
		SM2P256V1Field.Multiply(z, array8, z);
		SM2P256V1Field.SquareN(z, 4, z);
		SM2P256V1Field.Multiply(z, array2, z);
		SM2P256V1Field.SquareN(z, 32, z);
		SM2P256V1Field.Multiply(z, y, z);
		SM2P256V1Field.SquareN(z, 62, z);
		uint[] array9 = array2;
		SM2P256V1Field.Square(z, array9);
		if (!Nat256.Eq(y, array9))
		{
			return null;
		}
		return new SM2P256V1FieldElement(z);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SM2P256V1FieldElement);
	}

	public override bool Equals(ECFieldElement other)
	{
		return Equals(other as SM2P256V1FieldElement);
	}

	public virtual bool Equals(SM2P256V1FieldElement other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Nat256.Eq(x, other.x);
	}

	public override int GetHashCode()
	{
		return Q.GetHashCode() ^ Arrays.GetHashCode(x, 0, 8);
	}
}
