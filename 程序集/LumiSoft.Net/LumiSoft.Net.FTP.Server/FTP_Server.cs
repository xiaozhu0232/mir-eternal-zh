using System;
using System.Net;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.FTP.Server;

public class FTP_Server : TCP_Server<FTP_Session>
{
	private string m_GreetingText = "";

	private int m_MaxBadCommands = 30;

	private IPAddress m_pPassivePublicIP;

	private int m_PassiveStartPort = 20000;

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

	public IPAddress PassivePublicIP
	{
		get
		{
			return m_pPassivePublicIP;
		}
		set
		{
			m_pPassivePublicIP = value;
		}
	}

	public int PassiveStartPort
	{
		get
		{
			return m_PassiveStartPort;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Valu must be > 0 !");
			}
			m_PassiveStartPort = value;
		}
	}

	public FTP_Server()
	{
		base.SessionIdleTimeout = 3600;
	}

	protected override void OnMaxConnectionsExceeded(FTP_Session session)
	{
		session.TcpStream.WriteLine("500 Client host rejected: too many connections, please try again later.");
	}

	protected override void OnMaxConnectionsPerIPExceeded(FTP_Session session)
	{
		session.TcpStream.WriteLine("500 Client host rejected: too many connections from your IP(" + session.RemoteEndPoint.Address?.ToString() + "), please try again later.");
	}
}
