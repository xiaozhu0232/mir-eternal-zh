namespace Org.BouncyCastle.Math.EC.Endo;

public class GlvTypeBEndomorphism : GlvEndomorphism, ECEndomorphism
{
	protected readonly GlvTypeBParameters m_parameters;

	protected readonly ECPointMap m_pointMap;

	public virtual ECPointMap PointMap => m_pointMap;

	public virtual bool HasEfficientPointMap => true;

	public GlvTypeBEndomorphism(ECCurve curve, GlvTypeBParameters parameters)
	{
		m_parameters = parameters;
		m_pointMap = new ScaleXPointMap(curve.FromBigInteger(parameters.Beta));
	}

	public virtual BigInteger[] DecomposeScalar(BigInteger k)
	{
		return EndoUtilities.DecomposeScalar(m_parameters.SplitParams, k);
	}
}
