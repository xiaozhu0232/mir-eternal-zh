using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_TargetDialog : SIP_t_ValueWithParams
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
			if (m_CallID == null)
			{
				throw new ArgumentNullException("CallID");
			}
			if (m_CallID == "")
			{
				throw new ArgumentException("Property 'CallID' may not be '' !");
			}
			m_CallID = value;
		}
	}

	public string RemoteTag
	{
		get
		{
			return base.Parameters["remote-tag"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("remote-tag");
			}
			else
			{
				base.Parameters.Set("remote-tag", value);
			}
		}
	}

	public string LocalTag
	{
		get
		{
			return base.Parameters["local-tag"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("local-tag");
			}
			else
			{
				base.Parameters.Set("local-tag", value);
			}
		}
	}

	public SIP_t_TargetDialog(string value)
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
			throw new SIP_ParseException("SIP Target-Dialog 'callid' value is missing !");
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
