namespace Org.BouncyCastle.Math.Field;

public interface IPolynomialExtensionField : IExtensionField, IFiniteField
{
	IPolynomial MinimalPolynomial { get; }
}
