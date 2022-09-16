using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_IdentityInfo : SIP_t_ValueWithParams
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
			if (value == null)
			{
				throw new ArgumentNullException("Uri");
			}
			if (value == "")
			{
				throw new ArgumentException("Invalid Identity-Info 'absoluteURI' value !");
			}
			m_Uri = value;
		}
	}

	public string Alg
	{
		get
		{
			return base.Parameters["alg"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("alg");
			}
			else
			{
				base.Parameters.Set("alg", value);
			}
		}
	}

	public SIP_t_IdentityInfo(string value)
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
		try
		{
			string text = reader.ReadParenthesized();
			if (text == null)
			{
				throw new SIP_ParseException("Invalid Identity-Info 'absoluteURI' value !");
			}
			m_Uri = text;
		}
		catch
		{
			throw new SIP_ParseException("Invalid Identity-Info 'absoluteURI' value !");
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
