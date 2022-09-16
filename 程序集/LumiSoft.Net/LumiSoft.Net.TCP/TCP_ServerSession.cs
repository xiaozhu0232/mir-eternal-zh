using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using LumiSoft.Net.IO;
using LumiSoft.Net.Log;

namespace LumiSoft.Net.TCP;

public class TCP_ServerSession : TCP_Session
{
	public class SwitchToSecureAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private bool m_RiseCompleted;

		private AsyncOP_State m_State;

		private Exception m_pException;

		private TCP_ServerSession m_pTcpSession;

		private SslStream m_pSslStream;

		public AsyncOP_State State => m_State;

		public Exception Error
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Error' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pException;
			}
		}

		public event EventHandler<EventArgs<SwitchToSecureAsyncOP>> CompletedAsync;

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pTcpSession = null;
				m_pSslStream = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(TCP_ServerSession owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pTcpSession = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				m_pSslStream = new SslStream(m_pTcpSession.TcpStream.SourceStream, leaveInnerStreamOpen: true);
				m_pSslStream.BeginAuthenticateAsServer(m_pTcpSession.m_pCertificate, BeginAuthenticateAsServerCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		private void SetState(AsyncOP_State state)
		{
			if (m_State == AsyncOP_State.Disposed)
			{
				return;
			}
			lock (m_pLock)
			{
				m_State = state;
				if (m_State == AsyncOP_State.Completed && m_RiseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void BeginAuthenticateAsServerCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSslStream.EndAuthenticateAsServer(ar);
				m_pTcpSession.m_pTcpStream.IsOwner = false;
				m_pTcpSession.m_pTcpStream.Dispose();
				m_pTcpSession.m_IsSecure = true;
				m_pTcpSession.m_pTcpStream = new SmartStream(m_pSslStream, owner: true);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<SwitchToSecureAsyncOP>(this));
			}
		}
	}

	private bool m_IsDisposed;

	private bool m_IsTerminated;

	private object m_pServer;

	private string m_ID = "";

	private DateTime m_ConnectTime;

	private string m_LocalHostName = "";

	private IPEndPoint m_pLocalEP;

	private IPEndPoint m_pRemoteEP;

	private bool m_IsSsl;

	private bool m_IsSecure;

	private X509Certificate m_pCertificate;

	private NetworkStream m_pRawTcpStream;

	private SmartStream m_pTcpStream;

	private object m_pTag;

	private Dictionary<string, object> m_pTags;

	public bool IsDisposed => m_IsDisposed;

	public object Server
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_pServer;
		}
	}

	public string LocalHostName
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_LocalHostName;
		}
	}

	public X509Certificate Certificate
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_pCertificate;
		}
	}

	public object Tag
	{
		get
		{
			return m_pTag;
		}
		set
		{
			m_pTag = value;
		}
	}

	public Dictionary<string, object> Tags
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_pTags;
		}
	}

	public override bool IsConnected => true;

	public override string ID
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_ID;
		}
	}

	public override DateTime ConnectTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_ConnectTime;
		}
	}

	public override DateTime LastActivity
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_pTcpStream.LastActivity;
		}
	}

	public override IPEndPoint LocalEndPoint
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_pLocalEP;
		}
	}

	public override IPEndPoint RemoteEndPoint
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_pRemoteEP;
		}
	}

	public override bool IsSecureConnection
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_IsSecure;
		}
	}

	public override SmartStream TcpStream
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_ServerSession");
			}
			return m_pTcpStream;
		}
	}

	public event EventHandler IdleTimeout;

	public event EventHandler Disonnected;

	public event EventHandler Disposed;

	public event ErrorEventHandler Error;

	public TCP_ServerSession()
	{
		m_pTags = new Dictionary<string, object>();
	}

	public override void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		if (!m_IsTerminated)
		{
			try
			{
				Disconnect();
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
		m_pLocalEP = null;
		m_pRemoteEP = null;
		m_pCertificate = null;
		if (m_pTcpStream != null)
		{
			m_pTcpStream.Dispose();
		}
		m_pTcpStream = null;
		if (m_pRawTcpStream != null)
		{
			m_pRawTcpStream.Close();
		}
		m_pRawTcpStream = null;
		m_pTags = null;
		this.IdleTimeout = null;
		this.Disonnected = null;
		this.Disposed = null;
	}

	internal void Init(object server, Socket socket, string hostName, bool ssl, X509Certificate certificate)
	{
		m_pServer = server;
		m_LocalHostName = hostName;
		m_IsSsl = ssl;
		m_ID = Guid.NewGuid().ToString();
		m_ConnectTime = DateTime.Now;
		m_pLocalEP = (IPEndPoint)socket.LocalEndPoint;
		m_pRemoteEP = (IPEndPoint)socket.RemoteEndPoint;
		m_pCertificate = certificate;
		socket.ReceiveBufferSize = 32000;
		socket.SendBufferSize = 32000;
		m_pRawTcpStream = new NetworkStream(socket, ownsSocket: true);
		m_pTcpStream = new SmartStream(m_pRawTcpStream, owner: true);
	}

	internal void StartI()
	{
		if (m_IsSsl)
		{
			LogAddText("Starting SSL negotiation now.");
			DateTime startTime = DateTime.Now;
			Action<SwitchToSecureAsyncOP> switchSecureCompleted = delegate(SwitchToSecureAsyncOP e)
			{
				try
				{
					if (e.Error != null)
					{
						LogAddException(e.Error);
						if (!IsDisposed)
						{
							Disconnect();
						}
					}
					else
					{
						LogAddText("SSL negotiation completed successfully in " + (DateTime.Now - startTime).TotalSeconds.ToString("f2") + " seconds.");
						Start();
					}
				}
				catch (Exception exception)
				{
					LogAddException(exception);
					if (!IsDisposed)
					{
						Disconnect();
					}
				}
			};
			SwitchToSecureAsyncOP op = new SwitchToSecureAsyncOP();
			op.CompletedAsync += delegate
			{
				switchSecureCompleted(op);
			};
			if (!SwitchToSecureAsync(op))
			{
				switchSecureCompleted(op);
			}
		}
		else
		{
			Start();
		}
	}

	protected virtual void Start()
	{
	}

	public void SwitchToSecure()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("TCP_ServerSession");
		}
		if (m_IsSecure)
		{
			throw new InvalidOperationException("Session is already SSL/TLS.");
		}
		if (m_pCertificate == null)
		{
			throw new InvalidOperationException("There is no certificate specified.");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using SwitchToSecureAsyncOP switchToSecureAsyncOP = new SwitchToSecureAsyncOP();
		switchToSecureAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!SwitchToSecureAsync(switchToSecureAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (switchToSecureAsyncOP.Error != null)
		{
			throw switchToSecureAsyncOP.Error;
		}
	}

	public bool SwitchToSecureAsync(SwitchToSecureAsyncOP op)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsSecureConnection)
		{
			throw new InvalidOperationException("Connection is already secure.");
		}
		if (m_pCertificate == null)
		{
			throw new InvalidOperationException("There is no certificate specified.");
		}
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		if (op.State != 0)
		{
			throw new ArgumentException("Invalid argument 'op' state, 'op' must be in 'AsyncOP_State.WaitingForStart' state.", "op");
		}
		return op.Start(this);
	}

	public override void Disconnect()
	{
		Disconnect(null);
	}

	public void Disconnect(string text)
	{
		if (m_IsDisposed || m_IsTerminated)
		{
			return;
		}
		m_IsTerminated = true;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				m_pTcpStream.Write(text);
			}
			catch (Exception x)
			{
				OnError(x);
			}
		}
		try
		{
			OnDisonnected();
		}
		catch (Exception x2)
		{
			OnError(x2);
		}
		Dispose();
	}

	protected virtual void OnTimeout()
	{
	}

	internal virtual void OnTimeoutI()
	{
		OnTimeout();
	}

	private void LogAddText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		try
		{
			object value = Server.GetType().GetProperty("Logger").GetValue(Server, null);
			if (value != null)
			{
				((Logger)value).AddText(ID, AuthenticatedUserIdentity, text, LocalEndPoint, RemoteEndPoint);
			}
		}
		catch
		{
		}
	}

	private void LogAddException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		try
		{
			object value = Server.GetType().GetProperty("Logger").GetValue(Server, null);
			if (value != null)
			{
				((Logger)value).AddException(ID, AuthenticatedUserIdentity, exception.Message, LocalEndPoint, RemoteEndPoint, exception);
			}
		}
		catch
		{
		}
	}

	private void OnIdleTimeout()
	{
		if (this.IdleTimeout != null)
		{
			this.IdleTimeout(this, new EventArgs());
		}
	}

	private void OnDisonnected()
	{
		if (this.Disonnected != null)
		{
			this.Disonnected(this, new EventArgs());
		}
	}

	private void OnDisposed()
	{
		if (this.Disposed != null)
		{
			this.Disposed(this, new EventArgs());
		}
	}

	protected virtual void OnError(Exception x)
	{
		if (this.Error != null)
		{
			this.Error(this, new Error_EventArgs(x, new StackTrace()));
		}
	}
}
