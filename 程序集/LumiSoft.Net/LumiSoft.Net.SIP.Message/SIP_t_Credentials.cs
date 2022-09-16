using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Credentials : SIP_t_Value
{
	private string m_Method = "";

	private string m_AuthData = "";

	public string Method
	{
		get
		{
			return m_Method;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Method value cant be null or mepty !");
			}
			m_Method = value;
		}
	}

	public string AuthData
	{
		get
		{
			return m_AuthData;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property AuthData value cant be null or mepty !");
			}
			m_AuthData = value;
		}
	}

	public SIP_t_Credentials(string value)
	{
		Parse(new StringReader(value));
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
			throw new SIP_ParseException("Invalid 'credentials' value, authentication method is missing !");
		}
		m_Method = text;
		text = reader.ReadToEnd();
		if (text == null)
		{
			throw new SIP_ParseException("Invalid 'credentials' value, authentication parameters are missing !");
		}
		m_AuthData = text.Trim();
	}

	public override string ToStringValue()
	{
		return m_Method + " " + m_AuthData;
	}
}
