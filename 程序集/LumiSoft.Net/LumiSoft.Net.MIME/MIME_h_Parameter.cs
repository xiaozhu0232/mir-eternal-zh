using System;

namespace LumiSoft.Net.MIME;

public class MIME_h_Parameter
{
	private bool m_IsModified;

	private string m_Name = "";

	private string m_Value = "";

	public bool IsModified => m_IsModified;

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
			m_IsModified = true;
		}
	}

	public MIME_h_Parameter(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		m_Name = name;
		m_Value = value;
	}
}
