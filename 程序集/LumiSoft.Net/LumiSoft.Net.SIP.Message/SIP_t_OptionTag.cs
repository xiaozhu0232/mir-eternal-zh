using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_OptionTag : SIP_t_Value
{
	private string m_OptionTag = "";

	public string OptionTag
	{
		get
		{
			return m_OptionTag;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("property OptionTag value cant be null or empty !");
			}
			m_OptionTag = value;
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
			throw new ArgumentException("Invalid 'option-tag' value, value is missing !");
		}
		m_OptionTag = text;
	}

	public override string ToStringValue()
	{
		return m_OptionTag;
	}
}
