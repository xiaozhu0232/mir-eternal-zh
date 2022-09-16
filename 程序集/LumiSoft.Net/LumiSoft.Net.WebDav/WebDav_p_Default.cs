using System;

namespace LumiSoft.Net.WebDav;

public class WebDav_p_Default : WebDav_p
{
	private string m_Namespace = "";

	private string m_Name;

	private string m_Value;

	public override string Namespace => m_Namespace;

	public override string Name => m_Name;

	public override string Value => m_Value;

	public WebDav_p_Default(string nameSpace, string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("Argument 'name' value must be specified.");
		}
		m_Namespace = nameSpace;
		m_Name = name;
		m_Value = value;
	}
}
