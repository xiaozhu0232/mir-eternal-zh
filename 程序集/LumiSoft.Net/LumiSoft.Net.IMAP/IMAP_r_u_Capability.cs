using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Capability : IMAP_r_u
{
	private string[] m_pCapabilities;

	public string[] Capabilities => m_pCapabilities;

	public IMAP_r_u_Capability(string[] capabilities)
	{
		if (capabilities == null)
		{
			throw new ArgumentNullException("capabilities");
		}
		m_pCapabilities = capabilities;
	}

	public static IMAP_r_u_Capability Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		StringReader stringReader = new StringReader(response);
		stringReader.ReadWord();
		stringReader.ReadWord();
		return new IMAP_r_u_Capability(stringReader.ReadToEnd().Split(' '));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* CAPABILITY");
		string[] pCapabilities = m_pCapabilities;
		foreach (string text in pCapabilities)
		{
			stringBuilder.Append(" " + text);
		}
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
