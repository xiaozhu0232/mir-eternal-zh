using System;

namespace LumiSoft.Net.IMAP.Client;

internal class IMAP_Client_CmdPart
{
	private IMAP_Client_CmdPart_Type m_Type;

	private string m_Value;

	public IMAP_Client_CmdPart_Type Type => m_Type;

	public string Value => m_Value;

	public IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type type, string data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_Type = type;
		m_Value = data;
	}
}
