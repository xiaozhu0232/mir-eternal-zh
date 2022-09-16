using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Info : SIP_t_ValueWithParams
{
	private string m_Uri = "";

	public string Purpose
	{
		get
		{
			return base.Parameters["purpose"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("purpose");
			}
			else
			{
				base.Parameters.Set("purpose", value);
			}
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
			throw new SIP_ParseException("Invalid Alert-Info value, Uri not between <> !");
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
