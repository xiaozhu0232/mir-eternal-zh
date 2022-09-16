using System;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class HeaderFieldParameter
{
	private string m_Name = "";

	private string m_Value = "";

	public string Name => m_Name;

	public string Value => m_Value;

	public HeaderFieldParameter(string parameterName, string parameterValue)
	{
		m_Name = parameterName;
		m_Value = parameterValue;
	}
}
