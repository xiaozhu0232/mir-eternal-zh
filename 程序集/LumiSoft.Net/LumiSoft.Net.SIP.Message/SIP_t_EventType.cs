using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_EventType : SIP_t_Value
{
	private string m_EventType = "";

	public string EventType
	{
		get
		{
			return m_EventType;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("EventType");
			}
			m_EventType = value;
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
			throw new SIP_ParseException("Invalid 'event-type' value, event-type is missing !");
		}
		m_EventType = text;
	}

	public override string ToStringValue()
	{
		return m_EventType;
	}
}
