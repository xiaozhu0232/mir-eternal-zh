using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_WarningValue : SIP_t_Value
{
	private int m_Code;

	private string m_Agent = "";

	private string m_Text = "";

	public int Code
	{
		get
		{
			return m_Code;
		}
		set
		{
			if (value < 100 || value > 999)
			{
				throw new ArgumentException("Property Code value must be 3 digit !");
			}
			m_Code = value;
		}
	}

	public string Agent
	{
		get
		{
			return m_Agent;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Agent value may not be null or empty !");
			}
			m_Agent = value;
		}
	}

	public string Text
	{
		get
		{
			return m_Text;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Text value may not be null or empty !");
			}
			m_Text = value;
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
			throw new SIP_ParseException("Invalid 'warning-value' value, warn-code is missing !");
		}
		try
		{
			Code = Convert.ToInt32(text);
		}
		catch
		{
			throw new SIP_ParseException("Invalid 'warning-value' warn-code value, warn-code is missing !");
		}
		text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("Invalid 'warning-value' value, warn-agent is missing !");
		}
		Agent = text;
		text = reader.ReadToEnd();
		if (text == null)
		{
			throw new SIP_ParseException("Invalid 'warning-value' value, warn-text is missing !");
		}
		Text = TextUtils.UnQuoteString(text);
	}

	public override string ToStringValue()
	{
		return m_Code + " " + m_Agent + " " + TextUtils.QuoteString(m_Text);
	}
}
