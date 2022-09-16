using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_RetryAfter : SIP_t_ValueWithParams
{
	private int m_Time;

	public int Time
	{
		get
		{
			return m_Time;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Property Time value must be >= 1 !");
			}
			m_Time = value;
		}
	}

	public int Duration
	{
		get
		{
			SIP_Parameter sIP_Parameter = base.Parameters["duration"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value == -1)
			{
				base.Parameters.Remove("duration");
				return;
			}
			if (value < 1)
			{
				throw new ArgumentException("Property Duration value must be >= 1 !");
			}
			base.Parameters.Set("duration", value.ToString());
		}
	}

	public SIP_t_RetryAfter(string value)
	{
		Parse(value);
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
			throw new SIP_ParseException("SIP Retry-After 'delta-seconds' value is missing !");
		}
		try
		{
			m_Time = Convert.ToInt32(text);
		}
		catch
		{
			throw new SIP_ParseException("Invalid SIP Retry-After 'delta-seconds' value !");
		}
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_Time);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
