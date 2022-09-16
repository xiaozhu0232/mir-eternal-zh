using System;
using System.Collections.Generic;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_Server : TCP_Server<SMTP_Session>
{
	private List<string> m_pServiceExtentions;

	private string m_GreetingText = "";

	private int m_MaxBadCommands = 30;

	private int m_MaxTransactions = 10;

	private int m_MaxMessageSize = 10000000;

	private int m_MaxRecipients = 100;

	public string[] ServiceExtentions
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pServiceExtentions.ToArray();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ServiceExtentions");
			}
			m_pServiceExtentions.Clear();
			foreach (string text in value)
			{
				if (text.ToUpper() == SMTP_ServiceExtensions.PIPELINING)
				{
					m_pServiceExtentions.Add(SMTP_ServiceExtensions.PIPELINING);
				}
				else if (text.ToUpper() == SMTP_ServiceExtensions.SIZE)
				{
					m_pServiceExtentions.Add(SMTP_ServiceExtensions.SIZE);
				}
				else if (text.ToUpper() == SMTP_ServiceExtensions.STARTTLS)
				{
					m_pServiceExtentions.Add(SMTP_ServiceExtensions.STARTTLS);
				}
				else if (text.ToUpper() == SMTP_ServiceExtensions._8BITMIME)
				{
					m_pServiceExtentions.Add(SMTP_ServiceExtensions._8BITMIME);
				}
				else if (text.ToUpper() == SMTP_ServiceExtensions.BINARYMIME)
				{
					m_pServiceExtentions.Add(SMTP_ServiceExtensions.BINARYMIME);
				}
				else if (text.ToUpper() == SMTP_ServiceExtensions.CHUNKING)
				{
					m_pServiceExtentions.Add(SMTP_ServiceExtensions.CHUNKING);
				}
				else if (text.ToUpper() == SMTP_ServiceExtensions.DSN)
				{
					m_pServiceExtentions.Add(SMTP_ServiceExtensions.DSN);
				}
			}
		}
	}

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

	public int MaxTransactions
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaxTransactions;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 0)
			{
				throw new ArgumentException("Property 'MaxTransactions' value must be >= 0.");
			}
			m_MaxTransactions = value;
		}
	}

	public int MaxMessageSize
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaxMessageSize;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 500)
			{
				throw new ArgumentException("Property 'MaxMessageSize' value must be >= 500.");
			}
			m_MaxMessageSize = value;
		}
	}

	public int MaxRecipients
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaxRecipients;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 1)
			{
				throw new ArgumentException("Property 'MaxRecipients' value must be >= 1.");
			}
			m_MaxRecipients = value;
		}
	}

	internal List<string> Extentions => m_pServiceExtentions;

	public SMTP_Server()
	{
		m_pServiceExtentions = new List<string>();
		m_pServiceExtentions.Add(SMTP_ServiceExtensions.PIPELINING);
		m_pServiceExtentions.Add(SMTP_ServiceExtensions.SIZE);
		m_pServiceExtentions.Add(SMTP_ServiceExtensions.STARTTLS);
		m_pServiceExtentions.Add(SMTP_ServiceExtensions._8BITMIME);
		m_pServiceExtentions.Add(SMTP_ServiceExtensions.BINARYMIME);
		m_pServiceExtentions.Add(SMTP_ServiceExtensions.CHUNKING);
	}

	protected override void OnMaxConnectionsExceeded(SMTP_Session session)
	{
		session.TcpStream.WriteLine("421 Client host rejected: too many connections, please try again later.");
	}

	protected override void OnMaxConnectionsPerIPExceeded(SMTP_Session session)
	{
		session.TcpStream.WriteLine("421 Client host rejected: too many connections from your IP(" + session.RemoteEndPoint.Address?.ToString() + "), please try again later.");
	}
}
