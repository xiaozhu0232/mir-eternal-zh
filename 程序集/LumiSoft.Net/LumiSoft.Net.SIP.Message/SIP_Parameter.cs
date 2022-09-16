using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_Parameter
{
	private string m_Name = "";

	private string m_Value = "";

	public string Name => m_Name;

	public string Value
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

	public SIP_Parameter(string name)
		: this(name, "")
	{
	}

	public SIP_Parameter(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == "")
		{
			throw new ArgumentException("Parameter 'name' value may no be empty string !");
		}
		m_Name = name;
		m_Value = value;
	}
}
