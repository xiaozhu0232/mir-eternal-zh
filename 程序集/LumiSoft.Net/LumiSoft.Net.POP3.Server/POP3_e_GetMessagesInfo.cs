using System;
using System.Collections.Generic;

namespace LumiSoft.Net.POP3.Server;

public class POP3_e_GetMessagesInfo : EventArgs
{
	private List<POP3_ServerMessage> m_pMessages;

	public List<POP3_ServerMessage> Messages => m_pMessages;

	internal POP3_e_GetMessagesInfo()
	{
		m_pMessages = new List<POP3_ServerMessage>();
	}
}
