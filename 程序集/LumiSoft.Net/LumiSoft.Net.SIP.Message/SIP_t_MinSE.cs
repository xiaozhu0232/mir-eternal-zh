using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_MinSE : SIP_t_ValueWithParams
{
	private int m_Time = 90;

	public int Time
	{
		get
		{
			return m_Time;
		}
		set
		{
			if (m_Time < 1)
			{
				throw new ArgumentException("Time value must be > 0 !");
			}
			m_Time = value;
		}
	}

	public SIP_t_MinSE(string value)
	{
		Parse(value);
	}

	public SIP_t_MinSE(int minExpires)
	{
		m_Time = minExpires;
	}

	public void Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
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
			throw new SIP_ParseException("Min-SE delta-seconds value is missing !");
		}
		try
		{
			m_Time = Convert.ToInt32(text);
		}
		catch
		{
			throw new SIP_ParseException("Invalid Min-SE delta-seconds value !");
		}
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_Time.ToString());
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
