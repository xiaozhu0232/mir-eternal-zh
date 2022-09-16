namespace Org.BouncyCastle.Math.Field;

public interface IPolynomial
{
	int Degree { get; }

	int[] GetExponentsPresent();
}
