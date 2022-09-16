using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using LumiSoft.Net.IO;
using LumiSoft.Net.Log;

namespace LumiSoft.Net.TCP;

public class TCP_Client : TCP_Session
{
	public class ConnectAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IPEndPoint m_pLocalEP;

		private IPEndPoint m_pRemoteEP;

		private bool m_SSL;

		private RemoteCertificateValidationCallback m_pCertCallback;

		private TCP_Client m_pTcpClient;

		private Socket m_pSocket;

		private Stream m_pStream;

		private bool m_RiseCompleted;

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

		public Socket Socket
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Socket' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				return m_pSocket;
			}
		}

		public Stream Stream
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Stream' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				return m_pStream;
			}
		}

		public event EventHandler<EventArgs<ConnectAsyncOP>> CompletedAsync;

		public ConnectAsyncOP(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl, RemoteCertificateValidationCallback certCallback)
		{
			if (remoteEP == null)
			{
				throw new ArgumentNullException("localEP");
			}
			m_pLocalEP = localEP;
			m_pRemoteEP = remoteEP;
			m_SSL = ssl;
			m_pCertCallback = certCallback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pLocalEP = null;
				m_pRemoteEP = null;
				m_SSL = false;
				m_pCertCallback = null;
				m_pTcpClient = null;
				m_pSocket = null;
				m_pStream = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(TCP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pTcpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				if (m_pRemoteEP.AddressFamily == AddressFamily.InterNetwork)
				{
					m_pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					m_pSocket.ReceiveTimeout = m_pTcpClient.m_Timeout;
					m_pSocket.SendTimeout = m_pTcpClient.m_Timeout;
				}
				else if (m_pRemoteEP.AddressFamily == AddressFamily.InterNetworkV6)
				{
					m_pSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
					m_pSocket.ReceiveTimeout = m_pTcpClient.m_Timeout;
					m_pSocket.SendTimeout = m_pTcpClient.m_Timeout;
				}
				if (m_pLocalEP != null)
				{
					m_pSocket.Bind(m_pLocalEP);
				}
				m_pTcpClient.LogAddText("Connecting to " + m_pRemoteEP.ToString() + ".");
				m_pSocket.BeginConnect(m_pRemoteEP, BeginConnectCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				CleanupSocketRelated();
				if (m_pTcpClient != null)
				{
					m_pTcpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
				return false;
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

		private void BeginConnectCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSocket.EndConnect(ar);
				m_pTcpClient.LogAddText("Connected, localEP='" + m_pSocket.LocalEndPoint.ToString() + "'; remoteEP='" + m_pSocket.RemoteEndPoint.ToString() + "'.");
				if (m_SSL)
				{
					m_pTcpClient.LogAddText("Starting SSL handshake.");
					m_pStream = new SslStream(new NetworkStream(m_pSocket, ownsSocket: true), leaveInnerStreamOpen: false, RemoteCertificateValidationCallback);
					((SslStream)m_pStream).BeginAuthenticateAsClient("dummy", BeginAuthenticateAsClientCompleted, null);
				}
				else
				{
					m_pStream = new NetworkStream(m_pSocket, ownsSocket: true);
					InternalConnectCompleted();
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				CleanupSocketRelated();
				if (m_pTcpClient != null)
				{
					m_pTcpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void BeginAuthenticateAsClientCompleted(IAsyncResult ar)
		{
			try
			{
				((SslStream)m_pStream).EndAuthenticateAsClient(ar);
				m_pTcpClient.LogAddText("SSL handshake completed sucessfully.");
				InternalConnectCompleted();
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				CleanupSocketRelated();
				if (m_pTcpClient != null)
				{
					m_pTcpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (m_pCertCallback != null)
			{
				return m_pCertCallback(sender, certificate, chain, sslPolicyErrors);
			}
			if (sslPolicyErrors == SslPolicyErrors.None || (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) > SslPolicyErrors.None)
			{
				return true;
			}
			return false;
		}

		private void CleanupSocketRelated()
		{
			try
			{
				if (m_pStream != null)
				{
					m_pStream.Dispose();
				}
				if (m_pSocket != null)
				{
					m_pSocket.Close();
				}
			}
			catch
			{
			}
		}

		private void InternalConnectCompleted()
		{
			m_pTcpClient.m_IsConnected = true;
			m_pTcpClient.m_ID = Guid.NewGuid().ToString();
			m_pTcpClient.m_ConnectTime = DateTime.Now;
			m_pTcpClient.m_pLocalEP = (IPEndPoint)m_pSocket.LocalEndPoint;
			m_pTcpClient.m_pRemoteEP = (IPEndPoint)m_pSocket.RemoteEndPoint;
			m_pTcpClient.m_pTcpStream = new SmartStream(m_pStream, owner: true);
			m_pTcpClient.m_pTcpStream.Encoding = Encoding.UTF8;
			m_pTcpClient.OnConnected(CompleteConnectCallback);
		}

		private void CompleteConnectCallback(Exception error)
		{
			m_pException = error;
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ConnectAsyncOP>(this));
			}
		}
	}

	private delegate void DisconnectDelegate();

	protected class SwitchToSecureAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private bool m_RiseCompleted;

		private AsyncOP_State m_State;

		private Exception m_pException;

		private RemoteCertificateValidationCallback m_pCertCallback;

		private TCP_Client m_pTcpClient;

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

		public SwitchToSecureAsyncOP(RemoteCertificateValidationCallback certCallback)
		{
			m_pCertCallback = certCallback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pCertCallback = null;
				m_pSslStream = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(TCP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pTcpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				m_pSslStream = new SslStream(m_pTcpClient.m_pTcpStream.SourceStream, leaveInnerStreamOpen: false, RemoteCertificateValidationCallback);
				m_pSslStream.BeginAuthenticateAsClient("dummy", BeginAuthenticateAsClientCompleted, null);
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

		private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (m_pCertCallback != null)
			{
				return m_pCertCallback(sender, certificate, chain, sslPolicyErrors);
			}
			if (sslPolicyErrors == SslPolicyErrors.None || (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) > SslPolicyErrors.None)
			{
				return true;
			}
			return false;
		}

		private void BeginAuthenticateAsClientCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSslStream.EndAuthenticateAsClient(ar);
				m_pTcpClient.m_pTcpStream.IsOwner = false;
				m_pTcpClient.m_pTcpStream.Dispose();
				m_pTcpClient.m_IsSecure = true;
				m_pTcpClient.m_pTcpStream = new SmartStream(m_pSslStream, owner: true);
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

	protected delegate void CompleteConnectCallback(Exception error);

	private delegate void BeginConnectHostDelegate(string host, int port, bool ssl);

	private delegate void BeginConnectEPDelegate(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl);

	private bool m_IsDisposed;

	private bool m_IsConnected;

	private string m_ID = "";

	private DateTime m_ConnectTime;

	private IPEndPoint m_pLocalEP;

	private IPEndPoint m_pRemoteEP;

	private bool m_IsSecure;

	private SmartStream m_pTcpStream;

	private Logger m_pLogger;

	private RemoteCertificateValidationCallback m_pCertificateCallback;

	private int m_Timeout = 61000;

	public bool IsDisposed => m_IsDisposed;

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

	public override bool IsConnected => m_IsConnected;

	public override string ID
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("TCP_Client");
			}
			if (!m_IsConnected)
			{
				throw new InvalidOperationException("TCP client is not connected.");
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
				throw new ObjectDisposedException("TCP_Client");
			}
			if (!m_IsConnected)
			{
				throw new InvalidOperationException("TCP client is not connected.");
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
				throw new ObjectDisposedException("TCP_Client");
			}
			if (!m_IsConnected)
			{
				throw new InvalidOperationException("TCP client is not connected.");
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
				throw new ObjectDisposedException("TCP_Client");
			}
			if (!m_IsConnected)
			{
				throw new InvalidOperationException("TCP client is not connected.");
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
				throw new ObjectDisposedException("TCP_Client");
			}
			if (!m_IsConnected)
			{
				throw new InvalidOperationException("TCP client is not connected.");
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
				throw new ObjectDisposedException("TCP_Client");
			}
			if (!m_IsConnected)
			{
				throw new InvalidOperationException("TCP client is not connected.");
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
				throw new ObjectDisposedException("TCP_Client");
			}
			if (!m_IsConnected)
			{
				throw new InvalidOperationException("TCP client is not connected.");
			}
			return m_pTcpStream;
		}
	}

	public RemoteCertificateValidationCallback ValidateCertificateCallback
	{
		get
		{
			return m_pCertificateCallback;
		}
		set
		{
			m_pCertificateCallback = value;
		}
	}

	public int Timeout
	{
		get
		{
			return m_Timeout;
		}
		set
		{
			m_Timeout = value;
		}
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (!m_IsDisposed)
			{
				try
				{
					Disconnect();
				}
				catch
				{
				}
				m_IsDisposed = true;
			}
		}
	}

	public void Connect(string host, int port)
	{
		Connect(host, port, ssl: false);
	}

	public void Connect(string host, int port, bool ssl)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("TCP_Client");
		}
		if (m_IsConnected)
		{
			throw new InvalidOperationException("TCP client is already connected.");
		}
		if (string.IsNullOrEmpty(host))
		{
			throw new ArgumentException("Argument 'host' value may not be null or empty.");
		}
		if (port < 1)
		{
			throw new ArgumentException("Argument 'port' value must be >= 1.");
		}
		IPAddress[] hostAddresses = Dns.GetHostAddresses(host);
		for (int i = 0; i < hostAddresses.Length; i++)
		{
			try
			{
				Connect(null, new IPEndPoint(hostAddresses[i], port), ssl);
				break;
			}
			catch (Exception ex)
			{
				if (IsConnected)
				{
					throw ex;
				}
				if (i == hostAddresses.Length - 1)
				{
					throw ex;
				}
			}
		}
	}

	public void Connect(IPEndPoint remoteEP, bool ssl)
	{
		Connect(null, remoteEP, ssl);
	}

	public void Connect(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl)
	{
		Connect(localEP, remoteEP, ssl, null);
	}

	public void Connect(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl, RemoteCertificateValidationCallback certCallback)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_IsConnected)
		{
			throw new InvalidOperationException("TCP client is already connected.");
		}
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using ConnectAsyncOP connectAsyncOP = new ConnectAsyncOP(localEP, remoteEP, ssl, certCallback);
		connectAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!ConnectAsync(connectAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (connectAsyncOP.Error != null)
		{
			throw connectAsyncOP.Error;
		}
	}

	public bool ConnectAsync(ConnectAsyncOP op)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
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
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("TCP_Client");
		}
		if (!m_IsConnected)
		{
			throw new InvalidOperationException("TCP client is not connected.");
		}
		m_IsConnected = false;
		m_pLocalEP = null;
		m_pRemoteEP = null;
		m_pTcpStream.Dispose();
		m_IsSecure = false;
		m_pTcpStream = null;
		LogAddText("Disconnected.");
	}

	public IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!m_IsConnected)
		{
			throw new InvalidOperationException("TCP client is not connected.");
		}
		DisconnectDelegate disconnectDelegate = Disconnect;
		AsyncResultState asyncResultState = new AsyncResultState(this, disconnectDelegate, callback, state);
		asyncResultState.SetAsyncResult(disconnectDelegate.BeginInvoke(asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	public void EndDisconnect(IAsyncResult asyncResult)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginDisconnect method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("EndDisconnect was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is DisconnectDelegate)
		{
			((DisconnectDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginDisconnect method.");
	}

	protected void SwitchToSecure()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("TCP_Client");
		}
		if (!m_IsConnected)
		{
			throw new InvalidOperationException("TCP client is not connected.");
		}
		if (m_IsSecure)
		{
			throw new InvalidOperationException("TCP client is already secure.");
		}
		LogAddText("Switching to SSL.");
		SslStream sslStream = new SslStream(m_pTcpStream.SourceStream, leaveInnerStreamOpen: true, RemoteCertificateValidationCallback);
		sslStream.AuthenticateAsClient("dummy");
		m_pTcpStream.IsOwner = false;
		m_pTcpStream.Dispose();
		m_IsSecure = true;
		m_pTcpStream = new SmartStream(sslStream, owner: true);
	}

	private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		if (m_pCertificateCallback != null)
		{
			return m_pCertificateCallback(sender, certificate, chain, sslPolicyErrors);
		}
		if (sslPolicyErrors == SslPolicyErrors.None || (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) > SslPolicyErrors.None)
		{
			return true;
		}
		return false;
	}

	protected bool SwitchToSecureAsync(SwitchToSecureAsyncOP op)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (IsSecureConnection)
		{
			throw new InvalidOperationException("Connection is already secure.");
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

	protected virtual void OnConnected()
	{
	}

	protected virtual void OnConnected(CompleteConnectCallback callback)
	{
		try
		{
			OnConnected();
			callback(null);
		}
		catch (Exception error)
		{
			callback(error);
		}
	}

	protected string ReadLine()
	{
		SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
		TcpStream.ReadLine(readLineAsyncOP, async: false);
		if (readLineAsyncOP.Error != null)
		{
			throw readLineAsyncOP.Error;
		}
		string lineUtf = readLineAsyncOP.LineUtf8;
		if (readLineAsyncOP.BytesInBuffer > 0)
		{
			LogAddRead(readLineAsyncOP.BytesInBuffer, lineUtf);
		}
		else
		{
			LogAddText("Remote host closed connection.");
		}
		return lineUtf;
	}

	protected void WriteLine(string line)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		int num = TcpStream.WriteLine(line);
		LogAddWrite(num, line);
	}

	protected internal void LogAddRead(long size, string text)
	{
		try
		{
			if (m_pLogger != null)
			{
				m_pLogger.AddRead(ID, AuthenticatedUserIdentity, size, text, LocalEndPoint, RemoteEndPoint);
			}
		}
		catch
		{
		}
	}

	protected internal void LogAddWrite(long size, string text)
	{
		try
		{
			if (m_pLogger != null)
			{
				m_pLogger.AddWrite(ID, AuthenticatedUserIdentity, size, text, LocalEndPoint, RemoteEndPoint);
			}
		}
		catch
		{
		}
	}

	protected internal void LogAddText(string text)
	{
		try
		{
			if (m_pLogger != null)
			{
				m_pLogger.AddText(IsConnected ? ID : "", IsConnected ? AuthenticatedUserIdentity : null, text, IsConnected ? LocalEndPoint : null, IsConnected ? RemoteEndPoint : null);
			}
		}
		catch
		{
		}
	}

	protected internal void LogAddException(string text, Exception x)
	{
		try
		{
			if (m_pLogger != null)
			{
				m_pLogger.AddException(IsConnected ? ID : "", IsConnected ? AuthenticatedUserIdentity : null, text, IsConnected ? LocalEndPoint : null, IsConnected ? RemoteEndPoint : null, x);
			}
		}
		catch
		{
		}
	}

	[Obsolete("Use method ConnectAsync instead.")]
	public IAsyncResult BeginConnect(string host, int port, AsyncCallback callback, object state)
	{
		return BeginConnect(host, port, ssl: false, callback, state);
	}

	[Obsolete("Use method ConnectAsync instead.")]
	public IAsyncResult BeginConnect(string host, int port, bool ssl, AsyncCallback callback, object state)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_IsConnected)
		{
			throw new InvalidOperationException("TCP client is already connected.");
		}
		if (string.IsNullOrEmpty(host))
		{
			throw new ArgumentException("Argument 'host' value may not be null or empty.");
		}
		if (port < 1)
		{
			throw new ArgumentException("Argument 'port' value must be >= 1.");
		}
		BeginConnectHostDelegate beginConnectHostDelegate = Connect;
		AsyncResultState asyncResultState = new AsyncResultState(this, beginConnectHostDelegate, callback, state);
		asyncResultState.SetAsyncResult(beginConnectHostDelegate.BeginInvoke(host, port, ssl, asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method ConnectAsync instead.")]
	public IAsyncResult BeginConnect(IPEndPoint remoteEP, bool ssl, AsyncCallback callback, object state)
	{
		return BeginConnect(null, remoteEP, ssl, callback, state);
	}

	[Obsolete("Use method ConnectAsync instead.")]
	public IAsyncResult BeginConnect(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl, AsyncCallback callback, object state)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_IsConnected)
		{
			throw new InvalidOperationException("TCP client is already connected.");
		}
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		BeginConnectEPDelegate beginConnectEPDelegate = Connect;
		AsyncResultState asyncResultState = new AsyncResultState(this, beginConnectEPDelegate, callback, state);
		asyncResultState.SetAsyncResult(beginConnectEPDelegate.BeginInvoke(localEP, remoteEP, ssl, asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method ConnectAsync instead.")]
	public void EndConnect(IAsyncResult asyncResult)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginConnect method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("EndConnect was previously called for the asynchronous operation.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is BeginConnectHostDelegate)
		{
			((BeginConnectHostDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		if (asyncResultState.AsyncDelegate is BeginConnectEPDelegate)
		{
			((BeginConnectEPDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginConnect method.");
	}

	[Obsolete("Don't use this method.")]
	protected void OnError(Exception x)
	{
		try
		{
			_ = m_pLogger;
		}
		catch
		{
		}
	}
}
