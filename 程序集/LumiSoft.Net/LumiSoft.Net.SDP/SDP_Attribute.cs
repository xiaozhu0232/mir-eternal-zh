using System;

namespace LumiSoft.Net.SDP;

public class SDP_Attribute
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

	public SDP_Attribute(string name, string value)
	{
		m_Name = name;
		Value = value;
	}

	public static SDP_Attribute Parse(string aValue)
	{
		StringReader stringReader = new StringReader(aValue);
		stringReader.QuotedReadToDelimiter('=');
		string text = stringReader.QuotedReadToDelimiter(':');
		if (text == null)
		{
			throw new Exception("SDP message \"a\" field <attribute> name is missing !");
		}
		string name = text;
		string value = "";
		text = stringReader.ReadToEnd();
		if (text != null)
		{
			value = text;
		}
		return new SDP_Attribute(name, value);
	}

	public string ToValue()
	{
		if (string.IsNullOrEmpty(m_Value))
		{
			return "a=" + m_Name + "\r\n";
		}
		return "a=" + m_Name + ":" + m_Value + "\r\n";
	}
}
