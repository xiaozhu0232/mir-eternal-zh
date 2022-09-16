using System;
using System.IO;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_e_Message : EventArgs
{
	private SMTP_Session m_pSession;

	private Stream m_pStream;

	public SMTP_Session Session => m_pSession;

	public Stream Stream
	{
		get
		{
			return m_pStream;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Stream");
			}
			m_pStream = value;
		}
	}

	public SMTP_e_Message(SMTP_Session session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		m_pSession = session;
	}
}
