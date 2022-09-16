namespace Org.BouncyCastle.Math.EC.Multiplier;

public interface ECMultiplier
{
	ECPoint Multiply(ECPoint p, BigInteger k);
}
