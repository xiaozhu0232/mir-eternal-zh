using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_CallID : SIP_t_Value
{
	private string m_CallID = "";

	public string CallID
	{
		get
		{
			return m_CallID;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property CallID value may not be null or empty !");
			}
			m_CallID = value;
		}
	}

	public static SIP_t_CallID CreateCallID()
	{
		return new SIP_t_CallID
		{
			CallID = Guid.NewGuid().ToString().Replace("-", "")
		};
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
			throw new SIP_ParseException("Invalid 'callid' value, callid is missing !");
		}
		m_CallID = text;
	}

	public override string ToStringValue()
	{
		return m_CallID;
	}
}
