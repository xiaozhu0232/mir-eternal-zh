using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Replaces : SIP_t_ValueWithParams
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
			if (value == null)
			{
				throw new ArgumentNullException("CallID");
			}
			m_CallID = value;
		}
	}

	public string ToTag
	{
		get
		{
			return base.Parameters["to-tag"]?.Value;
		}
		set
		{
			if (value == null)
			{
				base.Parameters.Remove("to-tag");
			}
			else
			{
				base.Parameters.Set("to-tag", value);
			}
		}
	}

	public string FromTag
	{
		get
		{
			return base.Parameters["from-tag"]?.Value;
		}
		set
		{
			if (value == null)
			{
				base.Parameters.Remove("from-tag");
			}
			else
			{
				base.Parameters.Set("from-tag", value);
			}
		}
	}

	public bool EarlyFlag
	{
		get
		{
			if (base.Parameters.Contains("early-only"))
			{
				return true;
			}
			return false;
		}
		set
		{
			if (!value)
			{
				base.Parameters.Remove("early-only");
			}
			else
			{
				base.Parameters.Set("early-only", null);
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
			throw new SIP_ParseException("Replaces 'callid' value is missing !");
		}
		m_CallID = text;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_CallID);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
