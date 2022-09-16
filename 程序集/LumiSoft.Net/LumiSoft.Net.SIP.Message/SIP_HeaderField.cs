namespace LumiSoft.Net.SIP.Message;

public class SIP_HeaderField
{
	private string m_Name = "";

	private string m_Value = "";

	private bool m_IsMultiValue;

	public string Name => m_Name;

	public virtual string Value
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

	public bool IsMultiValue => m_IsMultiValue;

	internal SIP_HeaderField(string name, string value)
	{
		m_Name = name;
		m_Value = value;
	}

	internal void SetMultiValue(bool value)
	{
		m_IsMultiValue = value;
	}
}
