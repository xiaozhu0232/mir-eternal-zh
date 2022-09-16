using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Timestamp : SIP_t_Value
{
	private decimal m_Time;

	private decimal m_Delay;

	public decimal Time
	{
		get
		{
			return m_Time;
		}
		set
		{
			m_Time = value;
		}
	}

	public decimal Delay
	{
		get
		{
			return m_Delay;
		}
		set
		{
			m_Delay = value;
		}
	}

	public SIP_t_Timestamp(string value)
	{
		Parse(new StringReader(value));
	}

	public SIP_t_Timestamp(decimal time, decimal delay)
	{
		m_Time = time;
		m_Delay = delay;
	}

	public void Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("reader");
		}
		Parse(new StringReader(value));
	}

	public override void Parse(StringReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		string text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("Invalid 'Timestamp' value, time is missing !");
		}
		m_Time = Convert.ToDecimal(text);
		text = reader.ReadWord();
		if (text != null)
		{
			m_Delay = Convert.ToDecimal(text);
		}
		else
		{
			m_Delay = default(decimal);
		}
	}

	public override string ToStringValue()
	{
		if (m_Delay > 0m)
		{
			return m_Time + " " + m_Delay;
		}
		return m_Time.ToString();
	}
}
