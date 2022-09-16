using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ReasonValue : SIP_t_ValueWithParams
{
	private string m_Protocol = "";

	public string Protocol
	{
		get
		{
			return m_Protocol;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Protocol");
			}
			m_Protocol = value;
		}
	}

	public int Cause
	{
		get
		{
			if (base.Parameters["cause"] == null)
			{
				return -1;
			}
			return Convert.ToInt32(base.Parameters["cause"].Value);
		}
		set
		{
			if (value < 0)
			{
				base.Parameters.Remove("cause");
			}
			else
			{
				base.Parameters.Set("cause", value.ToString());
			}
		}
	}

	public string Text
	{
		get
		{
			return base.Parameters["text"]?.Value;
		}
		set
		{
			if (value == null)
			{
				base.Parameters.Remove("text");
			}
			else
			{
				base.Parameters.Set("text", value);
			}
		}
	}

	public SIP_t_ReasonValue()
	{
	}

	public SIP_t_ReasonValue(string value)
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
			throw new SIP_ParseException("SIP reason-value 'protocol' value is missing !");
		}
		m_Protocol = text;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_Protocol);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
