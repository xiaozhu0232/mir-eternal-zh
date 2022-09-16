namespace Org.BouncyCastle.Math.EC.Multiplier;

public class WNafPreCompInfo : PreCompInfo
{
	internal volatile int m_promotionCountdown = 4;

	protected int m_confWidth = -1;

	protected ECPoint[] m_preComp = null;

	protected ECPoint[] m_preCompNeg = null;

	protected ECPoint m_twice = null;

	protected int m_width = -1;

	internal int PromotionCountdown
	{
		get
		{
			return m_promotionCountdown;
		}
		set
		{
			m_promotionCountdown = value;
		}
	}

	public virtual bool IsPromoted => m_promotionCountdown <= 0;

	public virtual int ConfWidth
	{
		get
		{
			return m_confWidth;
		}
		set
		{
			m_confWidth = value;
		}
	}

	public virtual ECPoint[] PreComp
	{
		get
		{
			return m_preComp;
		}
		set
		{
			m_preComp = value;
		}
	}

	public virtual ECPoint[] PreCompNeg
	{
		get
		{
			return m_preCompNeg;
		}
		set
		{
			m_preCompNeg = value;
		}
	}

	public virtual ECPoint Twice
	{
		get
		{
			return m_twice;
		}
		set
		{
			m_twice = value;
		}
	}

	public virtual int Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	internal int DecrementPromotionCountdown()
	{
		int num = m_promotionCountdown;
		if (num > 0)
		{
			num = (m_promotionCountdown = num - 1);
		}
		return num;
	}
}
