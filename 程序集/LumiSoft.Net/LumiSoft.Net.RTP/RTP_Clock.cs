using System;

namespace LumiSoft.Net.RTP;

public class RTP_Clock
{
	private int m_BaseValue;

	private int m_Rate = 1;

	private DateTime m_CreateTime;

	public int BaseValue => m_BaseValue;

	public int Rate => m_Rate;

	public uint RtpTimestamp
	{
		get
		{
			long num = (long)(DateTime.Now - m_CreateTime).TotalMilliseconds;
			return (uint)(m_BaseValue + m_Rate * num / 1000);
		}
	}

	public RTP_Clock(int baseValue, int rate)
	{
		if (rate < 1)
		{
			throw new ArgumentException("Argument 'rate' value must be between 1 and 100 000.", "rate");
		}
		m_BaseValue = baseValue;
		m_Rate = rate;
		m_CreateTime = DateTime.Now;
	}

	public int MillisecondsToRtpTicks(int milliseconds)
	{
		return m_Rate * milliseconds / 1000;
	}
}
