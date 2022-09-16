using System;

namespace Org.BouncyCastle.Math.EC.Endo;

public class ScalarSplitParameters
{
	protected readonly BigInteger m_v1A;

	protected readonly BigInteger m_v1B;

	protected readonly BigInteger m_v2A;

	protected readonly BigInteger m_v2B;

	protected readonly BigInteger m_g1;

	protected readonly BigInteger m_g2;

	protected readonly int m_bits;

	public virtual BigInteger V1A => m_v1A;

	public virtual BigInteger V1B => m_v1B;

	public virtual BigInteger V2A => m_v2A;

	public virtual BigInteger V2B => m_v2B;

	public virtual BigInteger G1 => m_g1;

	public virtual BigInteger G2 => m_g2;

	public virtual int Bits => m_bits;

	private static void CheckVector(BigInteger[] v, string name)
	{
		if (v == null || v.Length != 2 || v[0] == null || v[1] == null)
		{
			throw new ArgumentException("Must consist of exactly 2 (non-null) values", name);
		}
	}

	public ScalarSplitParameters(BigInteger[] v1, BigInteger[] v2, BigInteger g1, BigInteger g2, int bits)
	{
		CheckVector(v1, "v1");
		CheckVector(v2, "v2");
		m_v1A = v1[0];
		m_v1B = v1[1];
		m_v2A = v2[0];
		m_v2B = v2[1];
		m_g1 = g1;
		m_g2 = g2;
		m_bits = bits;
	}
}
