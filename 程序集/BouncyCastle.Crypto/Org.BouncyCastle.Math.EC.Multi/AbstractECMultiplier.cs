namespace Org.BouncyCastle.Math.EC.Multiplier;

public abstract class AbstractECMultiplier : ECMultiplier
{
	public virtual ECPoint Multiply(ECPoint p, BigInteger k)
	{
		int signValue = k.SignValue;
		if (signValue == 0 || p.IsInfinity)
		{
			return p.Curve.Infinity;
		}
		ECPoint eCPoint = MultiplyPositive(p, k.Abs());
		ECPoint p2 = ((signValue > 0) ? eCPoint : eCPoint.Negate());
		return CheckResult(p2);
	}

	protected abstract ECPoint MultiplyPositive(ECPoint p, BigInteger k);

	protected virtual ECPoint CheckResult(ECPoint p)
	{
		return ECAlgorithms.ImplCheckResult(p);
	}
}
