using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ErrorUri : SIP_t_ValueWithParams
{
	private string m_Uri = "";

	public string Uri
	{
		get
		{
			return m_Uri;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Uri value can't be null or empty !");
			}
			m_Uri = value;
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
		reader.QuotedReadToDelimiter('<');
		if (!reader.StartsWith("<"))
		{
			throw new SIP_ParseException("Invalid 'error-uri' value, Uri not between <> !");
		}
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<" + m_Uri + ">");
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
