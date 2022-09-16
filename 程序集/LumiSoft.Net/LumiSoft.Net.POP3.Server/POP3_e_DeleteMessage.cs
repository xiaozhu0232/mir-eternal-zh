using System;

namespace LumiSoft.Net.POP3.Server;

public class POP3_e_DeleteMessage : EventArgs
{
	private POP3_ServerMessage m_pMessage;

	public POP3_ServerMessage Message => m_pMessage;

	internal POP3_e_DeleteMessage(POP3_ServerMessage message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		m_pMessage = message;
	}
}
