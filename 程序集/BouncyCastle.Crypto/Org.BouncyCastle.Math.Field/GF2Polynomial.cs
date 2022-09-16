using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.Field;

internal class GF2Polynomial : IPolynomial
{
	protected readonly int[] exponents;

	public virtual int Degree => exponents[exponents.Length - 1];

	internal GF2Polynomial(int[] exponents)
	{
		this.exponents = Arrays.Clone(exponents);
	}

	public virtual int[] GetExponentsPresent()
	{
		return Arrays.Clone(exponents);
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is GF2Polynomial gF2Polynomial))
		{
			return false;
		}
		return Arrays.AreEqual(exponents, gF2Polynomial.exponents);
	}

	public override int GetHashCode()
	{
		return Arrays.GetHashCode(exponents);
	}
}
