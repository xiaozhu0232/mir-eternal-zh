namespace Org.BouncyCastle.Math.Field;

internal class PrimeField : IFiniteField
{
	protected readonly BigInteger characteristic;

	public virtual BigInteger Characteristic => characteristic;

	public virtual int Dimension => 1;

	internal PrimeField(BigInteger characteristic)
	{
		this.characteristic = characteristic;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is PrimeField primeField))
		{
			return false;
		}
		return characteristic.Equals(primeField.characteristic);
	}

	public override int GetHashCode()
	{
		return characteristic.GetHashCode();
	}
}
