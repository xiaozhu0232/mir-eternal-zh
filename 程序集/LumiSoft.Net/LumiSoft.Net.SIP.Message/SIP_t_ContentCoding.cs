using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ContentCoding : SIP_t_Value
{
	private string m_Encoding = "";

	public string Encoding
	{
		get
		{
			return m_Encoding;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Encoding value may not be null or empty !");
			}
			if (!TextUtils.IsToken(value))
			{
				throw new ArgumentException("Encoding value may be 'token' only !");
			}
			m_Encoding = value;
		}
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
			throw new SIP_ParseException("Invalid 'content-coding' value, value is missing !");
		}
		m_Encoding = text;
	}

	public override string ToStringValue()
	{
		return m_Encoding;
	}
}
