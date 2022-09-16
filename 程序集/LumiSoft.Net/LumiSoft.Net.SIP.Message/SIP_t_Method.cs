using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Method : SIP_t_Value
{
	private string m_Method = "";

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
				throw new ArgumentException("Property Method value can't be null or empty !");
			}
			if (TextUtils.IsToken(value))
			{
				throw new ArgumentException("Property Method value must be 'token' !");
			}
			m_Method = value;
		}
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
			throw new SIP_ParseException("Invalid 'Method' value, value is missing !");
		}
		m_Method = text;
	}

	public override string ToStringValue()
	{
		return m_Method;
	}
}
