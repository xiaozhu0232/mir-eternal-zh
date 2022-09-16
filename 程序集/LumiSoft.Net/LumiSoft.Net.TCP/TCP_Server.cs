using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using LumiSoft.Net.Log;

namespace LumiSoft.Net.TCP;

public class TCP_Server<T> : IDisposable where T : TCP_ServerSession, new()
{
	private class ListeningPoint
	{
		private Socket m_pSocket;

		private IPBindInfo m_pBindInfo;

		public Socket Socket => m_pSocket;

		public IPBindInfo BindInfo => m_pBindInfo;

		public ListeningPoint(Socket socket, IPBindInfo bind)
		{
			if (socket == null)
			{
				throw new ArgumentNullException("socket");
			}
			if (bind == null)
			{
				throw new ArgumentNullException("socket");
			}
			m_pSocket = socket;
			m_pBindInfo = bind;
		}
	}

	private class TCP_Acceptor : IDisposable
	{
		private bool m_IsDisposed;

		private bool m_IsRunning;

		private Socket m_pSocket;

		private SocketAsyncEventArgs m_pSocketArgs;

		private Dictionary<string, object> m_pTags;

		public Dictionary<string, object> Tags => m_pTags;

		public event EventHandler<EventArgs<Socket>> ConnectionAccepted;

		public event EventHandler<ExceptionEventArgs> Error;

		public TCP_Acceptor(Socket socket)
		{
			if (socket == null)
			{
				throw new ArgumentNullException("socket");
			}
			m_pSocket = socket;
			m_pTags = new Dictionary<string, object>();
		}

		public void Dispose()
		{
			if (!m_IsDisposed)
			{
				m_IsDisposed = true;
				m_pSocket = null;
				m_pSocketArgs = null;
				m_pTags = null;
				this.ConnectionAccepted = null;
				this.Error = null;
			}
		}

		public void Start()
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_IsRunning)
			{
				return;
			}
			m_IsRunning = true;
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					if (Net_Utils.IsSocketAsyncSupported())
					{
						m_pSocketArgs = new SocketAsyncEventArgs();
						m_pSocketArgs.Completed += delegate
						{
							if (m_IsDisposed)
							{
								return;
							}
							try
							{
								if (m_pSocketArgs.SocketError == SocketError.Success)
								{
									OnConnectionAccepted(m_pSocketArgs.AcceptSocket);
								}
								else
								{
									OnError(new Exception("Socket error '" + m_pSocketArgs.SocketError.ToString() + "'."));
								}
								IOCompletionAccept();
							}
							catch (Exception x2)
							{
								OnError(x2);
							}
						};
						IOCompletionAccept();
					}
					else
					{
						m_pSocket.BeginAccept(AsyncSocketAccept, null);
					}
				}
				catch (Exception x)
				{
					OnError(x);
				}
			});
		}

		private void IOCompletionAccept()
		{
			try
			{
				m_pSocketArgs.AcceptSocket = null;
				while (!m_IsDisposed && !m_pSocket.AcceptAsync(m_pSocketArgs))
				{
					if (m_pSocketArgs.SocketError == SocketError.Success)
					{
						try
						{
							OnConnectionAccepted(m_pSocketArgs.AcceptSocket);
							m_pSocketArgs.AcceptSocket = null;
						}
						catch (Exception x)
						{
							OnError(x);
						}
					}
					else
					{
						OnError(new Exception("Socket error '" + m_pSocketArgs.SocketError.ToString() + "'."));
					}
				}
			}
			catch (Exception x2)
			{
				OnError(x2);
			}
		}

		private void AsyncSocketAccept(IAsyncResult ar)
		{
			if (m_IsDisposed)
			{
				return;
			}
			try
			{
				OnConnectionAccepted(m_pSocket.EndAccept(ar));
			}
			catch (Exception x)
			{
				OnError(x);
			}
			try
			{
				m_pSocket.BeginAccept(AsyncSocketAccept, null);
			}
			catch (Exception x2)
			{
				OnError(x2);
			}
		}

		private void OnConnectionAccepted(Socket socket)
		{
			if (this.ConnectionAccepted != null)
			{
				this.ConnectionAccepted(this, new EventArgs<Socket>(socket));
			}
		}

		private void OnError(Exception x)
		{
			if (this.Error != null)
			{
				this.Error(this, new ExceptionEventArgs(x));
			}
		}
	}

	private bool m_IsDisposed;

	private bool m_IsRunning;

	private IPBindInfo[] m_pBindings = new IPBindInfo[0];

	private long m_MaxConnections;

	private long m_MaxConnectionsPerIP;

	private int m_SessionIdleTimeout = 100;

	private Logger m_pLogger;

	private DateTime m_StartTime;

	private long m_ConnectionsProcessed;

	private List<TCP_Acceptor> m_pConnectionAcceptors;

	private List<ListeningPoint> m_pListeningPoints;

	private TCP_SessionCollection<TCP_ServerSession> m_pSessions;

	private TimerEx m_pTimer_IdleTimeout;

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
				if (m_IsRunning)
				{
					StartListen();
				}
			}
		}
	}

	public IPEndPoint[] LocalEndPoints
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			List<IPEndPoint> list = new List<IPEndPoint>();
			IPBindInfo[] bindings = Bindings;
			foreach (IPBindInfo iPBindInfo in bindings)
			{
				if (iPBindInfo.IP.Equals(IPAddress.Any))
				{
					IPAddress[] hostAddresses = Dns.GetHostAddresses("");
					foreach (IPAddress iPAddress in hostAddresses)
					{
						if (iPAddress.AddressFamily == AddressFamily.InterNetwork && !list.Contains(new IPEndPoint(iPAddress, iPBindInfo.Port)))
						{
							list.Add(new IPEndPoint(iPAddress, iPBindInfo.Port));
						}
					}
				}
				else if (iPBindInfo.IP.Equals(IPAddress.IPv6Any))
				{
					IPAddress[] hostAddresses = Dns.GetHostAddresses("");
					foreach (IPAddress iPAddress2 in hostAddresses)
					{
						if (iPAddress2.AddressFamily == AddressFamily.InterNetworkV6 && !list.Contains(new IPEndPoint(iPAddress2, iPBindInfo.Port)))
						{
							list.Add(new IPEndPoint(iPAddress2, iPBindInfo.Port));
						}
					}
				}
				else if (!list.Contains(iPBindInfo.EndPoint))
				{
					list.Add(iPBindInfo.EndPoint);
				}
			}
			return list.ToArray();
		}
	}

	public long MaxConnections
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Server");
			}
			return m_MaxConnections;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Server");
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
				throw new ObjectDisposedException("TCP_Server");
			}
			return m_MaxConnectionsPerIP;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Server");
			}
			if (m_MaxConnectionsPerIP < 0)
			{
				throw new ArgumentException("Property 'MaxConnectionsPerIP' value must be >= 0.");
			}
			m_MaxConnectionsPerIP = value;
		}
	}

	public int SessionIdleTimeout
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Server");
			}
			return m_SessionIdleTimeout;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Server");
			}
			if (value < 0)
			{
				throw new ArgumentException("Property 'SessionIdleTimeout' value must be >= 0.");
			}
			m_SessionIdleTimeout = value;
		}
	}

	public Logger Logger
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pLogger;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_pLogger = value;
		}
	}

	public DateTime StartTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Server");
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("TCP server is not running.");
			}
			return m_StartTime;
		}
	}

	public long ConnectionsProcessed
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Server");
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("TCP server is not running.");
			}
			return m_ConnectionsProcessed;
		}
	}

	public TCP_SessionCollection<TCP_ServerSession> Sessions
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Server");
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("TCP server is not running.");
			}
			return m_pSessions;
		}
	}

	public event EventHandler Started;

	public event EventHandler Stopped;

	public event EventHandler Disposed;

	public event EventHandler<TCP_ServerSessionEventArgs<T>> SessionCreated;

	public event ErrorEventHandler Error;

	public TCP_Server()
	{
		m_pConnectionAcceptors = new List<TCP_Acceptor>();
		m_pListeningPoints = new List<ListeningPoint>();
		m_pSessions = new TCP_SessionCollection<TCP_ServerSession>();
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		if (m_IsRunning)
		{
			try
			{
				Stop();
			}
			catch
			{
			}
		}
		m_IsDisposed = true;
		try
		{
			OnDisposed();
		}
		catch
		{
		}
		m_pSessions = null;
		this.Started = null;
		this.Stopped = null;
		this.Disposed = null;
		this.Error = null;
	}

	private void m_pTimer_IdleTimeout_Elapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			TCP_ServerSession[] array = Sessions.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				T val = (T)array[i];
				try
				{
					if (DateTime.Now > val.TcpStream.LastActivity.AddSeconds(m_SessionIdleTimeout))
					{
						val.OnTimeoutI();
						if (!val.IsDisposed)
						{
							val.Disconnect();
							val.Dispose();
						}
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

	public void Start()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("TCP_Server");
		}
		if (!m_IsRunning)
		{
			m_IsRunning = true;
			m_StartTime = DateTime.Now;
			m_ConnectionsProcessed = 0L;
			ThreadPool.QueueUserWorkItem(delegate
			{
				StartListen();
			});
			m_pTimer_IdleTimeout = new TimerEx(30000.0, autoReset: true);
			m_pTimer_IdleTimeout.Elapsed += m_pTimer_IdleTimeout_Elapsed;
			m_pTimer_IdleTimeout.Enabled = true;
			OnStarted();
		}
	}

	public void Stop()
	{
		if (!m_IsRunning)
		{
			return;
		}
		m_IsRunning = false;
		TCP_Acceptor[] array = m_pConnectionAcceptors.ToArray();
		foreach (TCP_Acceptor tCP_Acceptor in array)
		{
			try
			{
				tCP_Acceptor.Dispose();
			}
			catch (Exception x)
			{
				OnError(x);
			}
		}
		m_pConnectionAcceptors.Clear();
		ListeningPoint[] array2 = m_pListeningPoints.ToArray();
		foreach (ListeningPoint listeningPoint in array2)
		{
			try
			{
				listeningPoint.Socket.Close();
			}
			catch (Exception x2)
			{
				OnError(x2);
			}
		}
		m_pListeningPoints.Clear();
		m_pTimer_IdleTimeout.Dispose();
		m_pTimer_IdleTimeout = null;
		OnStopped();
	}

	public void Restart()
	{
		Stop();
		Start();
	}

	protected virtual void OnMaxConnectionsExceeded(T session)
	{
	}

	protected virtual void OnMaxConnectionsPerIPExceeded(T session)
	{
	}

	private void StartListen()
	{
		try
		{
			ListeningPoint[] array = m_pListeningPoints.ToArray();
			foreach (ListeningPoint listeningPoint in array)
			{
				try
				{
					listeningPoint.Socket.Close();
				}
				catch (Exception x)
				{
					OnError(x);
				}
			}
			m_pListeningPoints.Clear();
			IPBindInfo[] pBindings = m_pBindings;
			foreach (IPBindInfo iPBindInfo in pBindings)
			{
				try
				{
					Socket socket = null;
					if (iPBindInfo.IP.AddressFamily == AddressFamily.InterNetwork)
					{
						socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						goto IL_0095;
					}
					if (iPBindInfo.IP.AddressFamily == AddressFamily.InterNetworkV6)
					{
						socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
						goto IL_0095;
					}
					goto end_IL_0055;
					IL_0095:
					socket.Bind(new IPEndPoint(iPBindInfo.IP, iPBindInfo.Port));
					socket.Listen(100);
					ListeningPoint item = new ListeningPoint(socket, iPBindInfo);
					m_pListeningPoints.Add(item);
					for (int j = 0; j < 10; j++)
					{
						TCP_Acceptor acceptor = new TCP_Acceptor(socket);
						acceptor.Tags["bind"] = iPBindInfo;
						acceptor.ConnectionAccepted += delegate(object s1, EventArgs<Socket> e1)
						{
							ProcessConnection(e1.Value, (IPBindInfo)acceptor.Tags["bind"]);
						};
						acceptor.Error += delegate(object s1, ExceptionEventArgs e1)
						{
							OnError(e1.Exception);
						};
						m_pConnectionAcceptors.Add(acceptor);
						acceptor.Start();
					}
					end_IL_0055:;
				}
				catch (Exception x2)
				{
					OnError(x2);
				}
			}
		}
		catch (Exception x3)
		{
			OnError(x3);
		}
	}

	private void ProcessConnection(Socket socket, IPBindInfo bindInfo)
	{
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		if (bindInfo == null)
		{
			throw new ArgumentNullException("bindInfo");
		}
		m_ConnectionsProcessed++;
		try
		{
			T val = new T();
			val.Init(this, socket, bindInfo.HostName, bindInfo.SslMode == SslMode.SSL, bindInfo.Certificate);
			if (m_MaxConnections != 0L && m_pSessions.Count > m_MaxConnections)
			{
				OnMaxConnectionsExceeded(val);
				val.Dispose();
				return;
			}
			if (m_MaxConnectionsPerIP != 0L && m_pSessions.GetConnectionsPerIP(val.RemoteEndPoint.Address) > m_MaxConnectionsPerIP)
			{
				OnMaxConnectionsPerIPExceeded(val);
				val.Dispose();
				return;
			}
			val.Disonnected += delegate(object sender, EventArgs e)
			{
				m_pSessions.Remove((TCP_ServerSession)sender);
			};
			m_pSessions.Add(val);
			OnSessionCreated(val);
			val.StartI();
		}
		catch (Exception x)
		{
			OnError(x);
		}
	}

	protected void OnStarted()
	{
		if (this.Started != null)
		{
			this.Started(this, new EventArgs());
		}
	}

	protected void OnStopped()
	{
		if (this.Stopped != null)
		{
			this.Stopped(this, new EventArgs());
		}
	}

	protected void OnDisposed()
	{
		if (this.Disposed != null)
		{
			this.Disposed(this, new EventArgs());
		}
	}

	private void OnSessionCreated(T session)
	{
		if (this.SessionCreated != null)
		{
			this.SessionCreated(this, new TCP_ServerSessionEventArgs<T>(this, session));
		}
	}

	private void OnError(Exception x)
	{
		if (this.Error != null)
		{
			this.Error(this, new Error_EventArgs(x, new StackTrace()));
		}
	}
}
