using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_RValue : SIP_t_Value
{
	private string m_Namespace = "";

	private string m_Priority = "";

	public string Namespace
	{
		get
		{
			return m_Namespace;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Namespace");
			}
			if (value == "")
			{
				throw new ArgumentException("Property Namespace value may not be '' !");
			}
			if (!TextUtils.IsToken(value))
			{
				throw new ArgumentException("Property Namespace value must be 'token' !");
			}
			m_Namespace = value;
		}
	}

	public string Priority
	{
		get
		{
			return m_Priority;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Priority");
			}
			if (value == "")
			{
				throw new ArgumentException("Property Priority value may not be '' !");
			}
			if (!TextUtils.IsToken(value))
			{
				throw new ArgumentException("Property Priority value must be 'token' !");
			}
			m_Priority = value;
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
		string[] array = (reader.ReadWord() ?? throw new SIP_ParseException("Invalid 'r-value' value, 'namespace \".\" r-priority' is missing !")).Split('.');
		if (array.Length != 2)
		{
			throw new SIP_ParseException("Invalid r-value !");
		}
		m_Namespace = array[0];
		m_Priority = array[1];
	}

	public override string ToStringValue()
	{
		return m_Namespace + "." + m_Priority;
	}
}
