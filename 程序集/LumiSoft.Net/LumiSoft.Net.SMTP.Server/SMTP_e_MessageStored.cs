using System;
using System.IO;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_e_MessageStored : EventArgs
{
	private SMTP_Session m_pSession;

	private Stream m_pStream;

	private SMTP_Reply m_pReply;

	public SMTP_Session Session => m_pSession;

	public Stream Stream => m_pStream;

	public SMTP_Reply Reply
	{
		get
		{
			return m_pReply;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Reply");
			}
			m_pReply = value;
		}
	}

	public SMTP_e_MessageStored(SMTP_Session session, Stream stream, SMTP_Reply reply)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (reply == null)
		{
			throw new ArgumentNullException("reply");
		}
		m_pSession = session;
		m_pStream = stream;
		m_pReply = reply;
	}
}
