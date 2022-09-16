using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using LumiSoft.Net.DNS.Client;
using LumiSoft.Net.IO;
using LumiSoft.Net.Log;
using LumiSoft.Net.SMTP.Client;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.SMTP.Relay;

public class Relay_Session : TCP_Session
{
	private class Relay_Target
	{
		private string m_HostName = "";

		private IPEndPoint m_pTarget;

		private SslMode m_SslMode;

		private string m_UserName;

		private string m_Password;

		public string HostName => m_HostName;

		public IPEndPoint Target => m_pTarget;

		public SslMode SslMode => m_SslMode;

		public string UserName => m_UserName;

		public string Password => m_Password;

		public Relay_Target(string hostName, IPEndPoint target)
		{
			m_HostName = hostName;
			m_pTarget = target;
		}

		public Relay_Target(string hostName, IPEndPoint target, SslMode sslMode, string userName, string password)
		{
			m_HostName = hostName;
			m_pTarget = target;
			m_SslMode = sslMode;
			m_UserName = userName;
			m_Password = password;
		}
	}

	private bool m_IsDisposed;

	private Relay_Server m_pServer;

	private IPBindInfo m_pLocalBindInfo;

	private Relay_QueueItem m_pRelayItem;

	private Relay_SmartHost[] m_pSmartHosts;

	private Relay_Mode m_RelayMode;

	private string m_SessionID = "";

	private DateTime m_SessionCreateTime;

	private SMTP_Client m_pSmtpClient;

	private List<Relay_Target> m_pTargets;

	private Relay_Target m_pActiveTarget;

	public bool IsDisposed => m_IsDisposed;

	public string LocalHostName
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_pLocalBindInfo != null)
			{
				return m_pLocalBindInfo.HostName;
			}
			return "";
		}
	}

	public DateTime SessionCreateTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SessionCreateTime;
		}
	}

	public int ExpectedTimeout
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (int)(m_pServer.SessionIdleTimeout - (DateTime.Now.Ticks - TcpStream.LastActivity.Ticks) / 10000);
		}
	}

	public string From
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRelayItem.From;
		}
	}

	public string To
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRelayItem.To;
		}
	}

	public string MessageID
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRelayItem.MessageID;
		}
	}

	public Stream MessageStream
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRelayItem.MessageStream;
		}
	}

	public string RemoteHostName
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_pActiveTarget != null)
			{
				return m_pActiveTarget.HostName;
			}
			return null;
		}
	}

	public Relay_Queue Queue
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRelayItem.Queue;
		}
	}

	public object QueueTag
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRelayItem.Tag;
		}
	}

	public override GenericIdentity AuthenticatedUserIdentity
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!m_pSmtpClient.IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_pSmtpClient.AuthenticatedUserIdentity;
		}
	}

	public override bool IsConnected
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSmtpClient.IsConnected;
		}
	}

	public override string ID
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SessionID;
		}
	}

	public override DateTime ConnectTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSmtpClient.ConnectTime;
		}
	}

	public override DateTime LastActivity
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSmtpClient.LastActivity;
		}
	}

	public override IPEndPoint LocalEndPoint
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSmtpClient.LocalEndPoint;
		}
	}

	public override IPEndPoint RemoteEndPoint
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSmtpClient.RemoteEndPoint;
		}
	}

	public override SmartStream TcpStream
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSmtpClient.TcpStream;
		}
	}

	internal Relay_Session(Relay_Server server, Relay_QueueItem realyItem)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (realyItem == null)
		{
			throw new ArgumentNullException("realyItem");
		}
		m_pServer = server;
		m_pRelayItem = realyItem;
		m_SessionID = Guid.NewGuid().ToString();
		m_SessionCreateTime = DateTime.Now;
		m_pTargets = new List<Relay_Target>();
		m_pSmtpClient = new SMTP_Client();
	}

	internal Relay_Session(Relay_Server server, Relay_QueueItem realyItem, Relay_SmartHost[] smartHosts)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (realyItem == null)
		{
			throw new ArgumentNullException("realyItem");
		}
		if (smartHosts == null)
		{
			throw new ArgumentNullException("smartHosts");
		}
		m_pServer = server;
		m_pRelayItem = realyItem;
		m_pSmartHosts = smartHosts;
		m_RelayMode = Relay_Mode.SmartHost;
		m_SessionID = Guid.NewGuid().ToString();
		m_SessionCreateTime = DateTime.Now;
		m_pTargets = new List<Relay_Target>();
		m_pSmtpClient = new SMTP_Client();
	}

	public override void Dispose()
	{
		Dispose(new ObjectDisposedException(GetType().Name));
	}

	public void Dispose(Exception exception)
	{
		try
		{
			lock (this)
			{
				if (!m_IsDisposed)
				{
					try
					{
						m_pServer.OnSessionCompleted(this, exception);
					}
					catch
					{
					}
					m_pServer.Sessions.Remove(this);
					m_IsDisposed = true;
					m_pLocalBindInfo = null;
					m_pRelayItem = null;
					m_pSmartHosts = null;
					if (m_pSmtpClient != null)
					{
						m_pSmtpClient.Dispose();
						m_pSmtpClient = null;
					}
					m_pTargets = null;
					if (m_pActiveTarget != null)
					{
						m_pServer.RemoveIpUsage(m_pActiveTarget.Target.Address);
						m_pActiveTarget = null;
					}
					m_pServer = null;
				}
			}
		}
		catch (Exception x)
		{
			if (m_pServer != null)
			{
				m_pServer.OnError(x);
			}
		}
	}

	internal void Start(object state)
	{
		try
		{
			if (m_pServer.Logger != null)
			{
				m_pSmtpClient.Logger = new Logger();
				m_pSmtpClient.Logger.WriteLog += SmtpClient_WriteLog;
			}
			LogText("Starting to relay message '" + m_pRelayItem.MessageID + "' from '" + m_pRelayItem.From + "' to '" + m_pRelayItem.To + "'.");
			if (m_RelayMode == Relay_Mode.Dns)
			{
				Dns_Client.GetEmailHostsAsyncOP op2 = new Dns_Client.GetEmailHostsAsyncOP(m_pRelayItem.To);
				op2.CompletedAsync += delegate
				{
					EmailHostsResolveCompleted(m_pRelayItem.To, op2);
				};
				if (!m_pServer.DnsClient.GetEmailHostsAsync(op2))
				{
					EmailHostsResolveCompleted(m_pRelayItem.To, op2);
				}
			}
			else if (m_RelayMode == Relay_Mode.SmartHost)
			{
				string[] array = new string[m_pSmartHosts.Length];
				for (int i = 0; i < m_pSmartHosts.Length; i++)
				{
					array[i] = m_pSmartHosts[i].Host;
				}
				Dns_Client.GetHostsAddressesAsyncOP op = new Dns_Client.GetHostsAddressesAsyncOP(array);
				op.CompletedAsync += delegate
				{
					SmartHostsResolveCompleted(op);
				};
				if (!m_pServer.DnsClient.GetHostsAddressesAsync(op))
				{
					SmartHostsResolveCompleted(op);
				}
			}
		}
		catch (Exception exception)
		{
			Dispose(exception);
		}
	}

	public override void Disconnect()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsConnected)
		{
			m_pSmtpClient.Disconnect();
		}
	}

	public void Disconnect(string text)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsConnected)
		{
			m_pSmtpClient.TcpStream.WriteLine(text);
			Disconnect();
		}
	}

	private void EmailHostsResolveCompleted(string to, Dns_Client.GetEmailHostsAsyncOP op)
	{
		if (to == null)
		{
			throw new ArgumentNullException("to");
		}
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		if (op.Error != null)
		{
			LogText("Failed to resolve email domain for email address '" + to + "' with error: " + op.Error.Message + ".");
			Dispose(op.Error);
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			HostEntry[] hosts = op.Hosts;
			foreach (HostEntry hostEntry in hosts)
			{
				IPAddress[] addresses = hostEntry.Addresses;
				foreach (IPAddress address in addresses)
				{
					m_pTargets.Add(new Relay_Target(hostEntry.HostName, new IPEndPoint(address, 25)));
				}
				stringBuilder.Append(hostEntry.HostName + " ");
			}
			LogText("Resolved to following email hosts: (" + stringBuilder.ToString().TrimEnd() + ").");
			BeginConnect();
		}
		op.Dispose();
	}

	private void SmartHostsResolveCompleted(Dns_Client.GetHostsAddressesAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		if (op.Error != null)
		{
			LogText("Failed to resolve relay smart host(s) ip addresses with error: " + op.Error.Message + ".");
			Dispose(op.Error);
		}
		else
		{
			for (int i = 0; i < op.HostEntries.Length; i++)
			{
				Relay_SmartHost relay_SmartHost = m_pSmartHosts[i];
				IPAddress[] addresses = op.HostEntries[i].Addresses;
				foreach (IPAddress address in addresses)
				{
					m_pTargets.Add(new Relay_Target(relay_SmartHost.Host, new IPEndPoint(address, relay_SmartHost.Port), relay_SmartHost.SslMode, relay_SmartHost.UserName, relay_SmartHost.Password));
				}
			}
			BeginConnect();
		}
		op.Dispose();
	}

	private void BeginConnect()
	{
		if (m_pTargets.Count == 0)
		{
			LogText("No relay target(s) for '" + m_pRelayItem.To + "', aborting.");
			Dispose(new Exception("No relay target(s) for '" + m_pRelayItem.To + "', aborting."));
			return;
		}
		if (m_pServer.MaxConnectionsPerIP > 0)
		{
			if (m_pServer.RelayMode == Relay_Mode.Dns || m_pServer.SmartHostsBalanceMode == BalanceMode.LoadBalance)
			{
				foreach (Relay_Target pTarget in m_pTargets)
				{
					m_pLocalBindInfo = m_pServer.GetLocalBinding(pTarget.Target.Address);
					if (m_pLocalBindInfo != null)
					{
						if (m_pServer.TryAddIpUsage(pTarget.Target.Address))
						{
							m_pActiveTarget = pTarget;
							m_pTargets.Remove(pTarget);
							break;
						}
						LogText("Skipping relay target (" + pTarget.HostName + "->" + pTarget.Target.Address?.ToString() + "), maximum connections to the specified IP has reached.");
					}
					else
					{
						LogText("Skipping relay target (" + pTarget.HostName + "->" + pTarget.Target.Address?.ToString() + "), no suitable local IPv4/IPv6 binding.");
					}
				}
			}
			else
			{
				m_pLocalBindInfo = m_pServer.GetLocalBinding(m_pTargets[0].Target.Address);
				if (m_pLocalBindInfo != null)
				{
					if (m_pServer.TryAddIpUsage(m_pTargets[0].Target.Address))
					{
						m_pActiveTarget = m_pTargets[0];
						m_pTargets.RemoveAt(0);
					}
					else
					{
						LogText("Skipping relay target (" + m_pTargets[0].HostName + "->" + m_pTargets[0].Target.Address?.ToString() + "), maximum connections to the specified IP has reached.");
					}
				}
				else
				{
					LogText("Skipping relay target (" + m_pTargets[0].HostName + "->" + m_pTargets[0].Target.Address?.ToString() + "), no suitable local IPv4/IPv6 binding.");
				}
			}
		}
		else
		{
			m_pLocalBindInfo = m_pServer.GetLocalBinding(m_pTargets[0].Target.Address);
			if (m_pLocalBindInfo != null)
			{
				m_pActiveTarget = m_pTargets[0];
				m_pTargets.RemoveAt(0);
			}
			else
			{
				LogText("Skipping relay target (" + m_pTargets[0].HostName + "->" + m_pTargets[0].Target.Address?.ToString() + "), no suitable local IPv4/IPv6 binding.");
			}
		}
		if (m_pLocalBindInfo == null)
		{
			LogText("No suitable IPv4/IPv6 local IP endpoint for relay target.");
			Dispose(new Exception("No suitable IPv4/IPv6 local IP endpoint for relay target."));
			return;
		}
		if (m_pActiveTarget == null)
		{
			LogText("All targets has exeeded maximum allowed connection per IP address, skip relay.");
			Dispose(new Exception("All targets has exeeded maximum allowed connection per IP address, skip relay."));
			return;
		}
		m_pSmtpClient.LocalHostName = m_pLocalBindInfo.HostName;
		TCP_Client.ConnectAsyncOP connectOP = new TCP_Client.ConnectAsyncOP(new IPEndPoint(m_pLocalBindInfo.IP, 0), m_pActiveTarget.Target, ssl: false, null);
		connectOP.CompletedAsync += delegate
		{
			ConnectCompleted(connectOP);
		};
		if (!m_pSmtpClient.ConnectAsync(connectOP))
		{
			ConnectCompleted(connectOP);
		}
	}

	private void ConnectCompleted(TCP_Client.ConnectAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		try
		{
			if (op.Error != null)
			{
				try
				{
					m_pServer.RemoveIpUsage(m_pActiveTarget.Target.Address);
					m_pActiveTarget = null;
					if (!IsDisposed && !IsConnected && m_pTargets.Count > 0)
					{
						BeginConnect();
					}
					else
					{
						Dispose(op.Error);
					}
					return;
				}
				catch (Exception exception)
				{
					Dispose(exception);
					return;
				}
			}
			string hostName = (string.IsNullOrEmpty(m_pLocalBindInfo.HostName) ? Dns.GetHostName() : m_pLocalBindInfo.HostName);
			SMTP_Client.EhloHeloAsyncOP ehloOP = new SMTP_Client.EhloHeloAsyncOP(hostName);
			ehloOP.CompletedAsync += delegate
			{
				EhloCommandCompleted(ehloOP);
			};
			if (!m_pSmtpClient.EhloHeloAsync(ehloOP))
			{
				EhloCommandCompleted(ehloOP);
			}
		}
		catch (Exception exception2)
		{
			Dispose(exception2);
		}
	}

	private void EhloCommandCompleted(SMTP_Client.EhloHeloAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		try
		{
			if (op.Error != null)
			{
				Dispose(op.Error);
				return;
			}
			if (!m_pSmtpClient.IsSecureConnection && ((m_pServer.UseTlsIfPossible && IsTlsSupported()) || m_pActiveTarget.SslMode == SslMode.TLS))
			{
				SMTP_Client.StartTlsAsyncOP startTlsOP = new SMTP_Client.StartTlsAsyncOP(null);
				startTlsOP.CompletedAsync += delegate
				{
					StartTlsCommandCompleted(startTlsOP);
				};
				if (!m_pSmtpClient.StartTlsAsync(startTlsOP))
				{
					StartTlsCommandCompleted(startTlsOP);
				}
				return;
			}
			if (!string.IsNullOrEmpty(m_pActiveTarget.UserName))
			{
				SMTP_Client.AuthAsyncOP authOP = new SMTP_Client.AuthAsyncOP(m_pSmtpClient.AuthGetStrongestMethod(m_pActiveTarget.UserName, m_pActiveTarget.Password));
				authOP.CompletedAsync += delegate
				{
					AuthCommandCompleted(authOP);
				};
				if (!m_pSmtpClient.AuthAsync(authOP))
				{
					AuthCommandCompleted(authOP);
				}
				return;
			}
			long messageSize = -1L;
			try
			{
				messageSize = m_pRelayItem.MessageStream.Length - m_pRelayItem.MessageStream.Position;
			}
			catch
			{
			}
			SMTP_Client.MailFromAsyncOP mailOP = new SMTP_Client.MailFromAsyncOP(From, messageSize, IsDsnSupported() ? m_pRelayItem.DSN_Ret : SMTP_DSN_Ret.NotSpecified, IsDsnSupported() ? m_pRelayItem.EnvelopeID : null);
			mailOP.CompletedAsync += delegate
			{
				MailCommandCompleted(mailOP);
			};
			if (!m_pSmtpClient.MailFromAsync(mailOP))
			{
				MailCommandCompleted(mailOP);
			}
		}
		catch (Exception exception)
		{
			Dispose(exception);
		}
	}

	private void StartTlsCommandCompleted(SMTP_Client.StartTlsAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		try
		{
			if (op.Error != null)
			{
				Dispose(op.Error);
				return;
			}
			string hostName = (string.IsNullOrEmpty(m_pLocalBindInfo.HostName) ? Dns.GetHostName() : m_pLocalBindInfo.HostName);
			SMTP_Client.EhloHeloAsyncOP ehloOP = new SMTP_Client.EhloHeloAsyncOP(hostName);
			ehloOP.CompletedAsync += delegate
			{
				EhloCommandCompleted(ehloOP);
			};
			if (!m_pSmtpClient.EhloHeloAsync(ehloOP))
			{
				EhloCommandCompleted(ehloOP);
			}
		}
		catch (Exception exception)
		{
			Dispose(exception);
		}
	}

	private void AuthCommandCompleted(SMTP_Client.AuthAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		try
		{
			if (op.Error != null)
			{
				Dispose(op.Error);
				return;
			}
			long messageSize = -1L;
			try
			{
				messageSize = m_pRelayItem.MessageStream.Length - m_pRelayItem.MessageStream.Position;
			}
			catch
			{
			}
			SMTP_Client.MailFromAsyncOP mailOP = new SMTP_Client.MailFromAsyncOP(From, messageSize, IsDsnSupported() ? m_pRelayItem.DSN_Ret : SMTP_DSN_Ret.NotSpecified, IsDsnSupported() ? m_pRelayItem.EnvelopeID : null);
			mailOP.CompletedAsync += delegate
			{
				MailCommandCompleted(mailOP);
			};
			if (!m_pSmtpClient.MailFromAsync(mailOP))
			{
				MailCommandCompleted(mailOP);
			}
		}
		catch (Exception exception)
		{
			Dispose(exception);
		}
	}

	private void MailCommandCompleted(SMTP_Client.MailFromAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		try
		{
			if (op.Error != null)
			{
				Dispose(op.Error);
				return;
			}
			SMTP_Client.RcptToAsyncOP rcptOP = new SMTP_Client.RcptToAsyncOP(To, IsDsnSupported() ? m_pRelayItem.DSN_Notify : SMTP_DSN_Notify.NotSpecified, IsDsnSupported() ? m_pRelayItem.OriginalRecipient : null);
			rcptOP.CompletedAsync += delegate
			{
				RcptCommandCompleted(rcptOP);
			};
			if (!m_pSmtpClient.RcptToAsync(rcptOP))
			{
				RcptCommandCompleted(rcptOP);
			}
		}
		catch (Exception exception)
		{
			Dispose(exception);
		}
	}

	private void RcptCommandCompleted(SMTP_Client.RcptToAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		try
		{
			if (op.Error != null)
			{
				Dispose(op.Error);
				return;
			}
			SMTP_Client.SendMessageAsyncOP sendMsgOP = new SMTP_Client.SendMessageAsyncOP(m_pRelayItem.MessageStream, useBdatIfPossibe: false);
			sendMsgOP.CompletedAsync += delegate
			{
				MessageSendingCompleted(sendMsgOP);
			};
			if (!m_pSmtpClient.SendMessageAsync(sendMsgOP))
			{
				MessageSendingCompleted(sendMsgOP);
			}
		}
		catch (Exception exception)
		{
			Dispose(exception);
		}
	}

	private void MessageSendingCompleted(SMTP_Client.SendMessageAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		try
		{
			if (op.Error != null)
			{
				Dispose(op.Error);
			}
			else
			{
				Dispose(null);
			}
		}
		catch (Exception exception)
		{
			Dispose(exception);
		}
		op.Dispose();
	}

	private void SmtpClient_WriteLog(object sender, WriteLogEventArgs e)
	{
		try
		{
			if (m_pServer.Logger != null)
			{
				if (e.LogEntry.EntryType == LogEntryType.Read)
				{
					m_pServer.Logger.AddRead(m_SessionID, e.LogEntry.UserIdentity, e.LogEntry.Size, e.LogEntry.Text, e.LogEntry.LocalEndPoint, e.LogEntry.RemoteEndPoint);
				}
				else if (e.LogEntry.EntryType == LogEntryType.Text)
				{
					m_pServer.Logger.AddText(m_SessionID, e.LogEntry.UserIdentity, e.LogEntry.Text, e.LogEntry.LocalEndPoint, e.LogEntry.RemoteEndPoint);
				}
				else if (e.LogEntry.EntryType == LogEntryType.Write)
				{
					m_pServer.Logger.AddWrite(m_SessionID, e.LogEntry.UserIdentity, e.LogEntry.Size, e.LogEntry.Text, e.LogEntry.LocalEndPoint, e.LogEntry.RemoteEndPoint);
				}
				else if (e.LogEntry.EntryType == LogEntryType.Exception)
				{
					m_pServer.Logger.AddException(m_SessionID, e.LogEntry.UserIdentity, e.LogEntry.Text, e.LogEntry.LocalEndPoint, e.LogEntry.RemoteEndPoint, e.LogEntry.Exception);
				}
			}
		}
		catch
		{
		}
	}

	private void LogText(string text)
	{
		if (m_pServer.Logger != null)
		{
			GenericIdentity userIdentity = null;
			try
			{
				userIdentity = AuthenticatedUserIdentity;
			}
			catch
			{
			}
			IPEndPoint localEP = null;
			IPEndPoint remoteEP = null;
			try
			{
				localEP = m_pSmtpClient.LocalEndPoint;
				remoteEP = m_pSmtpClient.RemoteEndPoint;
			}
			catch
			{
			}
			m_pServer.Logger.AddText(m_SessionID, userIdentity, text, localEP, remoteEP);
		}
	}

	private bool IsDsnSupported()
	{
		string[] esmtpFeatures = m_pSmtpClient.EsmtpFeatures;
		for (int i = 0; i < esmtpFeatures.Length; i++)
		{
			if (string.Equals(esmtpFeatures[i], SMTP_ServiceExtensions.DSN, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsTlsSupported()
	{
		string[] esmtpFeatures = m_pSmtpClient.EsmtpFeatures;
		for (int i = 0; i < esmtpFeatures.Length; i++)
		{
			if (string.Equals(esmtpFeatures[i], SMTP_ServiceExtensions.STARTTLS, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
