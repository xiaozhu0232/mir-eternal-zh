using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_LanguageTag : SIP_t_ValueWithParams
{
	private string m_LanguageTag = "";

	public string LanguageTag
	{
		get
		{
			return m_LanguageTag;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property LanguageTag value can't be null or empty !");
			}
			m_LanguageTag = value;
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
			throw new SIP_ParseException("Invalid Content-Language value, language-tag value is missing !");
		}
		m_LanguageTag = text;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_LanguageTag);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
