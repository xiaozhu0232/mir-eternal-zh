using System;

namespace Org.BouncyCastle.Math.EC.Endo;

public class GlvTypeBParameters
{
	protected readonly BigInteger m_beta;

	protected readonly BigInteger m_lambda;

	protected readonly ScalarSplitParameters m_splitParams;

	public virtual BigInteger Beta => m_beta;

	public virtual BigInteger Lambda => m_lambda;

	public virtual ScalarSplitParameters SplitParams => m_splitParams;

	[Obsolete("Access via SplitParams instead")]
	public virtual BigInteger[] V1 => new BigInteger[2] { m_splitParams.V1A, m_splitParams.V1B };

	[Obsolete("Access via SplitParams instead")]
	public virtual BigInteger[] V2 => new BigInteger[2] { m_splitParams.V2A, m_splitParams.V2B };

	[Obsolete("Access via SplitParams instead")]
	public virtual BigInteger G1 => m_splitParams.G1;

	[Obsolete("Access via SplitParams instead")]
	public virtual BigInteger G2 => m_splitParams.G2;

	[Obsolete("Access via SplitParams instead")]
	public virtual int Bits => m_splitParams.Bits;

	[Obsolete("Use constructor taking a ScalarSplitParameters instead")]
	public GlvTypeBParameters(BigInteger beta, BigInteger lambda, BigInteger[] v1, BigInteger[] v2, BigInteger g1, BigInteger g2, int bits)
	{
		m_beta = beta;
		m_lambda = lambda;
		m_splitParams = new ScalarSplitParameters(v1, v2, g1, g2, bits);
	}

	public GlvTypeBParameters(BigInteger beta, BigInteger lambda, ScalarSplitParameters splitParams)
	{
		m_beta = beta;
		m_lambda = lambda;
		m_splitParams = splitParams;
	}
}
