using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_SessionExpires : SIP_t_ValueWithParams
{
	private int m_Expires = 90;

	public int Expires
	{
		get
		{
			return m_Expires;
		}
		set
		{
			if (m_Expires < 90)
			{
				throw new ArgumentException("Property Expires value must be >= 90 !");
			}
			m_Expires = value;
		}
	}

	public string Refresher
	{
		get
		{
			return base.Parameters["refresher"]?.Value;
		}
		set
		{
			if (value == null)
			{
				base.Parameters.Remove("refresher");
			}
			else
			{
				base.Parameters.Set("refresher", value);
			}
		}
	}

	public SIP_t_SessionExpires(string value)
	{
		Parse(value);
	}

	public SIP_t_SessionExpires(int expires, string refresher)
	{
		if (m_Expires < 90)
		{
			throw new ArgumentException("Argument 'expires' value must be >= 90 !");
		}
		m_Expires = expires;
		Refresher = refresher;
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
			throw new SIP_ParseException("Session-Expires delta-seconds value is missing !");
		}
		try
		{
			m_Expires = Convert.ToInt32(text);
		}
		catch
		{
			throw new SIP_ParseException("Invalid Session-Expires delta-seconds value !");
		}
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_Expires.ToString());
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
