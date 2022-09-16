using System;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_e_Ehlo : EventArgs
{
	private SMTP_Session m_pSession;

	private string m_Domain = "";

	private SMTP_Reply m_pReply;

	public SMTP_Session Session => m_pSession;

	public string Domain => m_Domain;

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

	public SMTP_e_Ehlo(SMTP_Session session, string domain, SMTP_Reply reply)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (domain == string.Empty)
		{
			throw new ArgumentException("Argument 'domain' value must be sepcified.", "domain");
		}
		if (reply == null)
		{
			throw new ArgumentNullException("reply");
		}
		m_pSession = session;
		m_Domain = domain;
		m_pReply = reply;
	}
}
