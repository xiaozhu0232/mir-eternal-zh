namespace Org.BouncyCastle.Math.EC.Endo;

public class GlvTypeAParameters
{
	protected readonly BigInteger m_i;

	protected readonly BigInteger m_lambda;

	protected readonly ScalarSplitParameters m_splitParams;

	public virtual BigInteger I => m_i;

	public virtual BigInteger Lambda => m_lambda;

	public virtual ScalarSplitParameters SplitParams => m_splitParams;

	public GlvTypeAParameters(BigInteger i, BigInteger lambda, ScalarSplitParameters splitParams)
	{
		m_i = i;
		m_lambda = lambda;
		m_splitParams = splitParams;
	}
}
