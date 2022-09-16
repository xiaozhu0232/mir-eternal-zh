using System;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_Server : TCP_Server<IMAP_Session>
{
	private string m_GreetingText = "";

	private int m_MaxBadCommands = 30;

	public string GreetingText
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_GreetingText;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_GreetingText = value;
		}
	}

	public int MaxBadCommands
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaxBadCommands;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 0)
			{
				throw new ArgumentException("Property 'MaxBadCommands' value must be >= 0.");
			}
			m_MaxBadCommands = value;
		}
	}

	protected override void OnMaxConnectionsExceeded(IMAP_Session session)
	{
		session.TcpStream.WriteLine("* NO Client host rejected: too many connections, please try again later.");
	}

	protected override void OnMaxConnectionsPerIPExceeded(IMAP_Session session)
	{
		session.TcpStream.WriteLine("* NO Client host rejected: too many connections from your IP(" + session.RemoteEndPoint.Address?.ToString() + "), please try again later.");
	}
}
