using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_Join : SIP_t_ValueWithParams
{
	private SIP_t_CallID m_pCallID;

	public SIP_t_CallID CallID
	{
		get
		{
			return m_pCallID;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("CallID");
			}
			m_pCallID = value;
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
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("ToTag is mandatory and cant be null or empty !");
			}
			base.Parameters.Set("to-tag", value);
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
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("FromTag is mandatory and cant be null or empty !");
			}
			base.Parameters.Set("from-tag", value);
		}
	}

	public SIP_t_Join(string value)
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
		SIP_t_CallID sIP_t_CallID = new SIP_t_CallID();
		sIP_t_CallID.Parse(reader);
		m_pCallID = sIP_t_CallID;
		ParseParameters(reader);
		if (base.Parameters["to-tag"] == null)
		{
			throw new SIP_ParseException("Join value mandatory to-tag value is missing !");
		}
		if (base.Parameters["from-tag"] == null)
		{
			throw new SIP_ParseException("Join value mandatory from-tag value is missing !");
		}
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_pCallID.ToStringValue());
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
