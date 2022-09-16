using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Enable : IMAP_r_u
{
	private string[] m_Capabilities;

	public string[] Capabilities => m_Capabilities;

	public IMAP_r_u_Enable(string[] capabilities)
	{
		if (capabilities == null)
		{
			throw new ArgumentNullException("capabilities");
		}
		m_Capabilities = capabilities;
	}

	public static IMAP_r_u_Enable Parse(string enableResponse)
	{
		if (enableResponse == null)
		{
			throw new ArgumentNullException("enableResponse");
		}
		StringReader stringReader = new StringReader(enableResponse);
		stringReader.ReadWord();
		stringReader.ReadWord();
		return new IMAP_r_u_Enable(stringReader.ReadToEnd().Split(' '));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* ENABLED");
		string[] capabilities = m_Capabilities;
		foreach (string text in capabilities)
		{
			stringBuilder.Append(" " + text);
		}
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
