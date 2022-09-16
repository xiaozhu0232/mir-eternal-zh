using System;
using System.IO;

namespace LumiSoft.Net.POP3.Server;

public class POP3_e_GetMessageStream : EventArgs
{
	private POP3_ServerMessage m_pMessage;

	private bool m_CloseStream = true;

	private Stream m_pStream;

	public POP3_ServerMessage Message => m_pMessage;

	public bool CloseMessageStream
	{
		get
		{
			return m_CloseStream;
		}
		set
		{
			m_CloseStream = value;
		}
	}

	public Stream MessageStream
	{
		get
		{
			return m_pStream;
		}
		set
		{
			m_pStream = value;
		}
	}

	internal POP3_e_GetMessageStream(POP3_ServerMessage message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		m_pMessage = message;
	}
}
