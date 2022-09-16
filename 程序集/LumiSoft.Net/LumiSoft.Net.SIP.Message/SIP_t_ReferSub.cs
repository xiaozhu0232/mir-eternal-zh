using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ReferSub : SIP_t_ValueWithParams
{
	private bool m_Value;

	public bool Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public SIP_t_ReferSub()
	{
	}

	public SIP_t_ReferSub(string value)
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
		string text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("Refer-Sub refer-sub-value value is missing !");
		}
		try
		{
			m_Value = Convert.ToBoolean(text);
		}
		catch
		{
			throw new SIP_ParseException("Invalid Refer-Sub refer-sub-value value !");
		}
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_Value.ToString());
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
