using System;
using Org.BouncyCastle.Math.EC.Abc;
using Org.BouncyCastle.Math.Field;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC;

public abstract class AbstractF2mCurve : ECCurve
{
	private BigInteger[] si = null;

	public virtual bool IsKoblitz
	{
		get
		{
			if (m_order != null && m_cofactor != null && m_b.IsOne)
			{
				if (!m_a.IsZero)
				{
					return m_a.IsOne;
				}
				return true;
			}
			return false;
		}
	}

	public static BigInteger Inverse(int m, int[] ks, BigInteger x)
	{
		return new LongArray(x).ModInverse(m, ks).ToBigInteger();
	}

	private static IFiniteField BuildField(int m, int k1, int k2, int k3)
	{
		if (k1 == 0)
		{
			throw new ArgumentException("k1 must be > 0");
		}
		if (k2 == 0)
		{
			if (k3 != 0)
			{
				throw new ArgumentException("k3 must be 0 if k2 == 0");
			}
			return FiniteFields.GetBinaryExtensionField(new int[3] { 0, k1, m });
		}
		if (k2 <= k1)
		{
			throw new ArgumentException("k2 must be > k1");
		}
		if (k3 <= k2)
		{
			throw new ArgumentException("k3 must be > k2");
		}
		return FiniteFields.GetBinaryExtensionField(new int[5] { 0, k1, k2, k3, m });
	}

	protected AbstractF2mCurve(int m, int k1, int k2, int k3)
		: base(BuildField(m, k1, k2, k3))
	{
	}

	[Obsolete("Per-point compression property will be removed")]
	public override ECPoint CreatePoint(BigInteger x, BigInteger y, bool withCompression)
	{
		ECFieldElement eCFieldElement = FromBigInteger(x);
		ECFieldElement eCFieldElement2 = FromBigInteger(y);
		switch (CoordinateSystem)
		{
		case 5:
		case 6:
			if (eCFieldElement.IsZero)
			{
				if (!eCFieldElement2.Square().Equals(B))
				{
					throw new ArgumentException();
				}
			}
			else
			{
				eCFieldElement2 = eCFieldElement2.Divide(eCFieldElement).Add(eCFieldElement);
			}
			break;
		}
		return CreateRawPoint(eCFieldElement, eCFieldElement2, withCompression);
	}

	public override bool IsValidFieldElement(BigInteger x)
	{
		if (x != null && x.SignValue >= 0)
		{
			return x.BitLength <= FieldSize;
		}
		return false;
	}

	public override ECFieldElement RandomFieldElement(SecureRandom r)
	{
		int fieldSize = FieldSize;
		return FromBigInteger(BigIntegers.CreateRandomBigInteger(fieldSize, r));
	}

	public override ECFieldElement RandomFieldElementMult(SecureRandom r)
	{
		int fieldSize = FieldSize;
		ECFieldElement eCFieldElement = FromBigInteger(ImplRandomFieldElementMult(r, fieldSize));
		ECFieldElement b = FromBigInteger(ImplRandomFieldElementMult(r, fieldSize));
		return eCFieldElement.Multiply(b);
	}

	protected override ECPoint DecompressPoint(int yTilde, BigInteger X1)
	{
		ECFieldElement eCFieldElement = FromBigInteger(X1);
		ECFieldElement eCFieldElement2 = null;
		if (eCFieldElement.IsZero)
		{
			eCFieldElement2 = B.Sqrt();
		}
		else
		{
			ECFieldElement beta = eCFieldElement.Square().Invert().Multiply(B)
				.Add(A)
				.Add(eCFieldElement);
			ECFieldElement eCFieldElement3 = SolveQuadraticEquation(beta);
			if (eCFieldElement3 != null)
			{
				if (eCFieldElement3.TestBitZero() != (yTilde == 1))
				{
					eCFieldElement3 = eCFieldElement3.AddOne();
				}
				switch (CoordinateSystem)
				{
				case 5:
				case 6:
					eCFieldElement2 = eCFieldElement3.Add(eCFieldElement);
					break;
				default:
					eCFieldElement2 = eCFieldElement3.Multiply(eCFieldElement);
					break;
				}
			}
		}
		if (eCFieldElement2 == null)
		{
			throw new ArgumentException("Invalid point compression");
		}
		return CreateRawPoint(eCFieldElement, eCFieldElement2, withCompression: true);
	}

	internal ECFieldElement SolveQuadraticEquation(ECFieldElement beta)
	{
		AbstractF2mFieldElement abstractF2mFieldElement = (AbstractF2mFieldElement)beta;
		bool hasFastTrace = abstractF2mFieldElement.HasFastTrace;
		if (hasFastTrace && abstractF2mFieldElement.Trace() != 0)
		{
			return null;
		}
		int fieldSize = FieldSize;
		if (((uint)fieldSize & (true ? 1u : 0u)) != 0)
		{
			ECFieldElement eCFieldElement = abstractF2mFieldElement.HalfTrace();
			if (hasFastTrace || eCFieldElement.Square().Add(eCFieldElement).Add(beta)
				.IsZero)
			{
				return eCFieldElement;
			}
			return null;
		}
		if (beta.IsZero)
		{
			return beta;
		}
		ECFieldElement eCFieldElement2 = FromBigInteger(BigInteger.Zero);
		ECFieldElement eCFieldElement3;
		ECFieldElement eCFieldElement6;
		do
		{
			ECFieldElement b = FromBigInteger(BigInteger.Arbitrary(fieldSize));
			eCFieldElement3 = eCFieldElement2;
			ECFieldElement eCFieldElement4 = beta;
			for (int i = 1; i < fieldSize; i++)
			{
				ECFieldElement eCFieldElement5 = eCFieldElement4.Square();
				eCFieldElement3 = eCFieldElement3.Square().Add(eCFieldElement5.Multiply(b));
				eCFieldElement4 = eCFieldElement5.Add(beta);
			}
			if (!eCFieldElement4.IsZero)
			{
				return null;
			}
			eCFieldElement6 = eCFieldElement3.Square().Add(eCFieldElement3);
		}
		while (eCFieldElement6.IsZero);
		return eCFieldElement3;
	}

	internal virtual BigInteger[] GetSi()
	{
		if (si == null)
		{
			lock (this)
			{
				if (si == null)
				{
					si = Tnaf.GetSi(this);
				}
			}
		}
		return si;
	}

	private static BigInteger ImplRandomFieldElementMult(SecureRandom r, int m)
	{
		BigInteger bigInteger;
		do
		{
			bigInteger = BigIntegers.CreateRandomBigInteger(m, r);
		}
		while (bigInteger.SignValue <= 0);
		return bigInteger;
	}
}
