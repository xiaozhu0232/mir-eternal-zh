using System;
using Org.BouncyCastle.Math.Field;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC;

public abstract class AbstractFpCurve : ECCurve
{
	protected AbstractFpCurve(BigInteger q)
		: base(FiniteFields.GetPrimeField(q))
	{
	}

	public override bool IsValidFieldElement(BigInteger x)
	{
		if (x != null && x.SignValue >= 0)
		{
			return x.CompareTo(Field.Characteristic) < 0;
		}
		return false;
	}

	public override ECFieldElement RandomFieldElement(SecureRandom r)
	{
		BigInteger characteristic = Field.Characteristic;
		ECFieldElement eCFieldElement = FromBigInteger(ImplRandomFieldElement(r, characteristic));
		ECFieldElement b = FromBigInteger(ImplRandomFieldElement(r, characteristic));
		return eCFieldElement.Multiply(b);
	}

	public override ECFieldElement RandomFieldElementMult(SecureRandom r)
	{
		BigInteger characteristic = Field.Characteristic;
		ECFieldElement eCFieldElement = FromBigInteger(ImplRandomFieldElementMult(r, characteristic));
		ECFieldElement b = FromBigInteger(ImplRandomFieldElementMult(r, characteristic));
		return eCFieldElement.Multiply(b);
	}

	protected override ECPoint DecompressPoint(int yTilde, BigInteger X1)
	{
		ECFieldElement eCFieldElement = FromBigInteger(X1);
		ECFieldElement eCFieldElement2 = eCFieldElement.Square().Add(A).Multiply(eCFieldElement)
			.Add(B);
		ECFieldElement eCFieldElement3 = eCFieldElement2.Sqrt();
		if (eCFieldElement3 == null)
		{
			throw new ArgumentException("Invalid point compression");
		}
		if (eCFieldElement3.TestBitZero() != (yTilde == 1))
		{
			eCFieldElement3 = eCFieldElement3.Negate();
		}
		return CreateRawPoint(eCFieldElement, eCFieldElement3, withCompression: true);
	}

	private static BigInteger ImplRandomFieldElement(SecureRandom r, BigInteger p)
	{
		BigInteger bigInteger;
		do
		{
			bigInteger = BigIntegers.CreateRandomBigInteger(p.BitLength, r);
		}
		while (bigInteger.CompareTo(p) >= 0);
		return bigInteger;
	}

	private static BigInteger ImplRandomFieldElementMult(SecureRandom r, BigInteger p)
	{
		BigInteger bigInteger;
		do
		{
			bigInteger = BigIntegers.CreateRandomBigInteger(p.BitLength, r);
		}
		while (bigInteger.SignValue <= 0 || bigInteger.CompareTo(p) >= 0);
		return bigInteger;
	}
}
