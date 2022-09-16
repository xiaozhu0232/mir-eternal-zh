using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Encoding : SIP_t_ValueWithParams
{
	private string m_ContentEncoding = "";

	public string ContentEncoding
	{
		get
		{
			return m_ContentEncoding;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property ContentEncoding value can't be null or empty !");
			}
			if (!TextUtils.IsToken(value))
			{
				throw new ArgumentException("Property ContentEncoding value may be 'token' only !");
			}
			m_ContentEncoding = value;
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
			throw new SIP_ParseException("Invalid 'encoding' value is missing !");
		}
		m_ContentEncoding = text;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_ContentEncoding);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
