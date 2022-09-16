using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using LumiSoft.Net.DNS.Client;
using LumiSoft.Net.Log;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.SMTP.Relay;

public class Relay_Server : IDisposable
{
	private bool m_IsDisposed;

	private bool m_IsRunning;

	private IPBindInfo[] m_pBindings = new IPBindInfo[0];

	private bool m_HasBindingsChanged;

	private Relay_Mode m_RelayMode;

	private List<Relay_Queue> m_pQueues;

	private BalanceMode m_SmartHostsBalanceMode;

	private CircleCollection<Relay_SmartHost> m_pSmartHosts;

	private CircleCollection<IPBindInfo> m_pLocalEndPointIPv4;

	private CircleCollection<IPBindInfo> m_pLocalEndPointIPv6;

	private long m_MaxConnections;

	private long m_MaxConnectionsPerIP;

	private bool m_UseTlsIfPossible;

	private Dns_Client m_pDsnClient;

	private TCP_SessionCollection<Relay_Session> m_pSessions;

	private Dictionary<IPAddress, long> m_pConnectionsPerIP;

	private int m_SessionIdleTimeout = 30;

	private TimerEx m_pTimerTimeout;

	private Logger m_pLogger;

	public bool IsDisposed => m_IsDisposed;

	public bool IsRunning => m_IsRunning;

	public IPBindInfo[] Bindings
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pBindings;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				value = new IPBindInfo[0];
			}
			bool flag = false;
			if (m_pBindings.Length != value.Length)
			{
				flag = true;
			}
			else
			{
				for (int i = 0; i < m_pBindings.Length; i++)
				{
					if (!m_pBindings[i].Equals(value[i]))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				m_pBindings = value;
				m_HasBindingsChanged = true;
			}
		}
	}

	public Relay_Mode RelayMode
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RelayMode;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_RelayMode = value;
		}
	}

	public List<Relay_Queue> Queues
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pQueues;
		}
	}

	public BalanceMode SmartHostsBalanceMode
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SmartHostsBalanceMode;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_SmartHostsBalanceMode = value;
		}
	}

	public Relay_SmartHost[] SmartHosts
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSmartHosts.ToArray();
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				throw new ArgumentNullException("SmartHosts");
			}
			m_pSmartHosts.Add(value);
		}
	}

	public long MaxConnections
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaxConnections;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 0)
			{
				throw new ArgumentException("Property 'MaxConnections' value must be >= 0.");
			}
			m_MaxConnections = value;
		}
	}

	public long MaxConnectionsPerIP
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaxConnectionsPerIP;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_MaxConnectionsPerIP < 0)
			{
				throw new ArgumentException("Property 'MaxConnectionsPerIP' value must be >= 0.");
			}
			m_MaxConnectionsPerIP = value;
		}
	}

	public bool UseTlsIfPossible
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_UseTlsIfPossible;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_UseTlsIfPossible = value;
		}
	}

	public int SessionIdleTimeout
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SessionIdleTimeout;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_SessionIdleTimeout < 0)
			{
				throw new ArgumentException("Property 'SessionIdleTimeout' value must be >= 0.");
			}
			m_SessionIdleTimeout = value;
		}
	}

	public TCP_SessionCollection<Relay_Session> Sessions
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("Relay server not running.");
			}
			return m_pSessions;
		}
	}

	public Logger Logger
	{
		get
		{
			return m_pLogger;
		}
		set
		{
			m_pLogger = value;
		}
	}

	public Dns_Client DnsClient
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pDsnClient;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				throw new ArgumentNullException("DnsClient");
			}
			m_pDsnClient = value;
		}
	}

	public event Relay_SessionCompletedEventHandler SessionCompleted;

	public event ErrorEventHandler Error;

	public Relay_Server()
	{
		m_pQueues = new List<Relay_Queue>();
		m_pSmartHosts = new CircleCollection<Relay_SmartHost>();
		m_pDsnClient = new Dns_Client();
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		try
		{
			if (m_IsRunning)
			{
				Stop();
			}
		}
		catch
		{
		}
		m_IsDisposed = true;
		this.Error = null;
		this.SessionCompleted = null;
		m_pQueues = null;
		m_pSmartHosts = null;
		m_pDsnClient.Dispose();
		m_pDsnClient = null;
	}

	private void m_pTimerTimeout_Elapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			Relay_Session[] array = Sessions.ToArray();
			foreach (Relay_Session relay_Session in array)
			{
				try
				{
					if (relay_Session.LastActivity.AddSeconds(m_SessionIdleTimeout) < DateTime.Now)
					{
						relay_Session.Dispose(new Exception("Session idle timeout."));
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception x)
		{
			OnError(x);
		}
	}

	public virtual void Start()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!m_IsRunning)
		{
			m_IsRunning = true;
			m_pLocalEndPointIPv4 = new CircleCollection<IPBindInfo>();
			m_pLocalEndPointIPv6 = new CircleCollection<IPBindInfo>();
			m_pSessions = new TCP_SessionCollection<Relay_Session>();
			m_pConnectionsPerIP = new Dictionary<IPAddress, long>();
			new Thread(Run).Start();
			m_pTimerTimeout = new TimerEx(30000.0);
			m_pTimerTimeout.Elapsed += m_pTimerTimeout_Elapsed;
			m_pTimerTimeout.Start();
		}
	}

	public virtual void Stop()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_IsRunning)
		{
			m_IsRunning = false;
			m_pLocalEndPointIPv4 = null;
			m_pLocalEndPointIPv6 = null;
			m_pSessions = null;
			m_pConnectionsPerIP = null;
			m_pTimerTimeout.Dispose();
			m_pTimerTimeout = null;
		}
	}

	private void Run()
	{
		while (m_IsRunning)
		{
			try
			{
				if (m_HasBindingsChanged)
				{
					m_pLocalEndPointIPv4.Clear();
					m_pLocalEndPointIPv6.Clear();
					IPBindInfo[] pBindings = m_pBindings;
					foreach (IPBindInfo iPBindInfo in pBindings)
					{
						if (iPBindInfo.IP == IPAddress.Any)
						{
							IPAddress[] hostAddresses = Dns.GetHostAddresses("");
							foreach (IPAddress iPAddress in hostAddresses)
							{
								if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
								{
									IPBindInfo item = new IPBindInfo(iPBindInfo.HostName, iPBindInfo.Protocol, iPAddress, 25);
									if (!m_pLocalEndPointIPv4.Contains(item))
									{
										m_pLocalEndPointIPv4.Add(item);
									}
								}
							}
							continue;
						}
						if (iPBindInfo.IP == IPAddress.IPv6Any)
						{
							IPAddress[] hostAddresses = Dns.GetHostAddresses("");
							foreach (IPAddress iPAddress2 in hostAddresses)
							{
								if (iPAddress2.AddressFamily == AddressFamily.InterNetworkV6)
								{
									IPBindInfo item2 = new IPBindInfo(iPBindInfo.HostName, iPBindInfo.Protocol, iPAddress2, 25);
									if (!m_pLocalEndPointIPv6.Contains(item2))
									{
										m_pLocalEndPointIPv6.Add(item2);
									}
								}
							}
							continue;
						}
						IPBindInfo item3 = new IPBindInfo(iPBindInfo.HostName, iPBindInfo.Protocol, iPBindInfo.IP, 25);
						if (iPBindInfo.IP.AddressFamily == AddressFamily.InterNetwork)
						{
							if (!m_pLocalEndPointIPv4.Contains(item3))
							{
								m_pLocalEndPointIPv4.Add(item3);
							}
						}
						else if (!m_pLocalEndPointIPv6.Contains(item3))
						{
							m_pLocalEndPointIPv6.Add(item3);
						}
					}
					m_HasBindingsChanged = false;
				}
				if (m_pLocalEndPointIPv4.Count == 0 && m_pLocalEndPointIPv6.Count == 0)
				{
					Thread.Sleep(10);
					continue;
				}
				if (m_MaxConnections != 0L && m_pSessions.Count >= m_MaxConnections)
				{
					Thread.Sleep(10);
					continue;
				}
				Relay_QueueItem relay_QueueItem = null;
				foreach (Relay_Queue pQueue in m_pQueues)
				{
					relay_QueueItem = pQueue.DequeueMessage();
					if (relay_QueueItem != null)
					{
						break;
					}
				}
				if (relay_QueueItem == null)
				{
					Thread.Sleep(10);
				}
				else if (relay_QueueItem.TargetServer != null)
				{
					Relay_Session relay_Session = new Relay_Session(this, relay_QueueItem, new Relay_SmartHost[1] { relay_QueueItem.TargetServer });
					m_pSessions.Add(relay_Session);
					ThreadPool.QueueUserWorkItem(relay_Session.Start);
				}
				else if (m_RelayMode == Relay_Mode.Dns)
				{
					Relay_Session relay_Session2 = new Relay_Session(this, relay_QueueItem);
					m_pSessions.Add(relay_Session2);
					ThreadPool.QueueUserWorkItem(relay_Session2.Start);
				}
				else if (m_RelayMode == Relay_Mode.SmartHost)
				{
					Relay_SmartHost[] array = null;
					array = ((m_SmartHostsBalanceMode != BalanceMode.FailOver) ? m_pSmartHosts.ToCurrentOrderArray() : m_pSmartHosts.ToArray());
					Relay_Session relay_Session3 = new Relay_Session(this, relay_QueueItem, array);
					m_pSessions.Add(relay_Session3);
					ThreadPool.QueueUserWorkItem(relay_Session3.Start);
				}
			}
			catch (Exception x)
			{
				OnError(x);
			}
		}
	}

	internal IPBindInfo GetLocalBinding(IPAddress remoteIP)
	{
		if (remoteIP == null)
		{
			throw new ArgumentNullException("remoteIP");
		}
		if (remoteIP.AddressFamily == AddressFamily.InterNetworkV6)
		{
			if (m_pLocalEndPointIPv6.Count == 0)
			{
				return null;
			}
			return m_pLocalEndPointIPv6.Next();
		}
		if (m_pLocalEndPointIPv4.Count == 0)
		{
			return null;
		}
		return m_pLocalEndPointIPv4.Next();
	}

	internal bool TryAddIpUsage(IPAddress ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		lock (m_pConnectionsPerIP)
		{
			long value = 0L;
			if (m_pConnectionsPerIP.TryGetValue(ip, out value))
			{
				if (m_MaxConnectionsPerIP > 0 && value >= m_MaxConnectionsPerIP)
				{
					return false;
				}
				m_pConnectionsPerIP[ip] = value + 1;
			}
			else
			{
				m_pConnectionsPerIP.Add(ip, 1L);
			}
			return true;
		}
	}

	internal void RemoveIpUsage(IPAddress ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		lock (m_pConnectionsPerIP)
		{
			long value = 0L;
			if (m_pConnectionsPerIP.TryGetValue(ip, out value))
			{
				if (value == 1)
				{
					m_pConnectionsPerIP.Remove(ip);
				}
				else
				{
					m_pConnectionsPerIP[ip] = value - 1;
				}
			}
		}
	}

	internal long GetIpUsage(IPAddress ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		lock (m_pConnectionsPerIP)
		{
			long value = 0L;
			if (m_pConnectionsPerIP.TryGetValue(ip, out value))
			{
				return value;
			}
			return 0L;
		}
	}

	protected internal virtual void OnSessionCompleted(Relay_Session session, Exception exception)
	{
		if (this.SessionCompleted != null)
		{
			this.SessionCompleted(new Relay_SessionCompletedEventArgs(session, exception));
		}
	}

	protected internal virtual void OnError(Exception x)
	{
		if (this.Error != null)
		{
			this.Error(this, new Error_EventArgs(x, new StackTrace()));
		}
	}
}
