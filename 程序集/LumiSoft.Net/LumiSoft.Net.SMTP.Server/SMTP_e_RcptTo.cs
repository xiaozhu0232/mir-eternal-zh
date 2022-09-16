using System;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_e_RcptTo : EventArgs
{
	private SMTP_Session m_pSession;

	private SMTP_RcptTo m_pRcptTo;

	private SMTP_Reply m_pReply;

	public SMTP_Session Session => m_pSession;

	public SMTP_RcptTo RcptTo => m_pRcptTo;

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

	public SMTP_e_RcptTo(SMTP_Session session, SMTP_RcptTo to, SMTP_Reply reply)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (to == null)
		{
			throw new ArgumentNullException("from");
		}
		if (reply == null)
		{
			throw new ArgumentNullException("reply");
		}
		m_pSession = session;
		m_pRcptTo = to;
		m_pReply = reply;
	}
}
