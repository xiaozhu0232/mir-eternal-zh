using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Language : SIP_t_ValueWithParams
{
	private string m_LanguageRange = "";

	public string LanguageRange
	{
		get
		{
			return m_LanguageRange;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property LanguageRange value can't be null or empty !");
			}
			m_LanguageRange = value;
		}
	}

	public double QValue
	{
		get
		{
			SIP_Parameter sIP_Parameter = base.Parameters["qvalue"];
			if (sIP_Parameter != null)
			{
				return Convert.ToDouble(sIP_Parameter.Value);
			}
			return -1.0;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentException("Property QValue value must be between 0.0 and 1.0 !");
			}
			if (value < 0.0)
			{
				base.Parameters.Remove("qvalue");
			}
			else
			{
				base.Parameters.Set("qvalue", value.ToString());
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
		string text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("Invalid Accept-Language value, language-range value is missing !");
		}
		m_LanguageRange = text;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_LanguageRange);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
