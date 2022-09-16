using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Event : SIP_t_ValueWithParams
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
			if (value == "")
			{
				throw new ArgumentException("Property EventType value can't be '' !");
			}
			m_EventType = value;
		}
	}

	public string ID
	{
		get
		{
			return base.Parameters["id"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("id");
			}
			else
			{
				base.Parameters.Set("id", value);
			}
		}
	}

	public SIP_t_Event(string value)
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
			throw new SIP_ParseException("SIP Event 'event-type' value is missing !");
		}
		m_EventType = text;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_EventType);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
