using System;

namespace LumiSoft.Net.SMTP.Relay;

public class Relay_SessionCompletedEventArgs
{
	private Relay_Session m_pSession;

	private Exception m_pException;

	public Relay_Session Session => m_pSession;

	public Exception Exception => m_pException;

	public Relay_SessionCompletedEventArgs(Relay_Session session, Exception exception)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		m_pSession = session;
		m_pException = exception;
	}
}
