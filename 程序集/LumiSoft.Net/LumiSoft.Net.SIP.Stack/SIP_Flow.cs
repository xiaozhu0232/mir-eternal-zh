using System;
using System.IO;
using System.Net;
using System.Threading;
using LumiSoft.Net.IO;
using LumiSoft.Net.MIME;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.TCP;
using LumiSoft.Net.UDP;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Flow : IDisposable
{
	private object m_pLock = new object();

	private bool m_IsDisposed;

	private bool m_IsServer;

	private SIP_Stack m_pStack;

	private TCP_Session m_pTcpSession;

	private DateTime m_CreateTime;

	private string m_ID = "";

	private IPEndPoint m_pLocalEP;

	private IPEndPoint m_pLocalPublicEP;

	private IPEndPoint m_pRemoteEP;

	private string m_Transport = "";

	private DateTime m_LastActivity;

	private DateTime m_LastPing;

	private long m_BytesWritten;

	private MemoryStream m_pMessage;

	private bool m_LastCRLF;

	private TimerEx m_pKeepAliveTimer;

	public bool IsDisposed => m_IsDisposed;

	public bool IsServer
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_IsServer;
		}
	}

	public DateTime CreateTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_CreateTime;
		}
	}

	public string ID
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_ID;
		}
	}

	public IPEndPoint LocalEP
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pLocalEP;
		}
	}

	public IPEndPoint LocalPublicEP
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_pLocalPublicEP != null)
			{
				return m_pLocalPublicEP;
			}
			m_pLocalPublicEP = LocalEP;
			try
			{
				AutoResetEvent completionWaiter = new AutoResetEvent(initialState: false);
				SIP_Request sIP_Request = m_pStack.CreateRequest("OPTIONS", new SIP_t_NameAddress("sip:ping@publicIP.com"), new SIP_t_NameAddress("sip:ping@publicIP.com"));
				sIP_Request.MaxForwards = 0;
				SIP_ClientTransaction optionsTransaction = m_pStack.TransactionLayer.CreateClientTransaction(this, sIP_Request, addVia: true);
				optionsTransaction.ResponseReceived += delegate(object s, SIP_ResponseReceivedEventArgs e)
				{
					SIP_t_ViaParm topMostValue = e.Response.Via.GetTopMostValue();
					IPEndPoint iPEndPoint = new IPEndPoint((topMostValue.Received == null) ? LocalEP.Address : topMostValue.Received, (topMostValue.RPort > 0) ? topMostValue.RPort : LocalEP.Port);
					if (!LocalEP.Address.Equals(iPEndPoint.Address))
					{
						m_pLocalPublicEP = iPEndPoint;
					}
					if (completionWaiter != null)
					{
						completionWaiter.Set();
					}
				};
				optionsTransaction.StateChanged += delegate
				{
					if (optionsTransaction.State == SIP_TransactionState.Terminated && completionWaiter != null)
					{
						completionWaiter.Set();
					}
				};
				optionsTransaction.Start();
				completionWaiter.WaitOne();
				completionWaiter.Close();
				completionWaiter = null;
			}
			catch
			{
			}
			return m_pLocalPublicEP;
		}
	}

	public IPEndPoint RemoteEP
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRemoteEP;
		}
	}

	public string Transport
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Transport;
		}
	}

	public bool IsReliable
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Transport != "UDP";
		}
	}

	public bool IsSecure
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_Transport == "TLS")
			{
				return true;
			}
			return false;
		}
	}

	public bool SendKeepAlives
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pKeepAliveTimer != null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value)
			{
				if (m_pKeepAliveTimer != null)
				{
					return;
				}
				m_pKeepAliveTimer = new TimerEx(15000.0, autoReset: true);
				m_pKeepAliveTimer.Elapsed += delegate
				{
					try
					{
						if (m_pStack.TransportLayer.Stack.Logger != null)
						{
							m_pStack.TransportLayer.Stack.Logger.AddWrite("", null, 2L, "Flow [id='" + ID + "'] sent \"ping\"", LocalEP, RemoteEP);
						}
						SendInternal(new byte[4] { 13, 10, 13, 10 });
					}
					catch
					{
					}
				};
				m_pKeepAliveTimer.Enabled = true;
			}
			else if (m_pKeepAliveTimer != null)
			{
				m_pKeepAliveTimer.Dispose();
				m_pKeepAliveTimer = null;
			}
		}
	}

	public DateTime LastActivity
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_Transport == "TCP" || m_Transport == "TLS")
			{
				return m_pTcpSession.LastActivity;
			}
			return m_LastActivity;
		}
	}

	public DateTime LastPing
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LastPing;
		}
	}

	public long BytesWritten
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_BytesWritten;
		}
	}

	public event EventHandler IsDisposing;

	internal SIP_Flow(SIP_Stack stack, bool isServer, IPEndPoint localEP, IPEndPoint remoteEP, string transport)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		if (transport == null)
		{
			throw new ArgumentNullException("transport");
		}
		m_pStack = stack;
		m_IsServer = isServer;
		m_pLocalEP = localEP;
		m_pRemoteEP = remoteEP;
		m_Transport = transport.ToUpper();
		m_CreateTime = DateTime.Now;
		m_LastActivity = DateTime.Now;
		m_ID = m_pLocalEP.ToString() + "-" + m_pRemoteEP.ToString() + "-" + m_Transport;
		m_pMessage = new MemoryStream();
	}

	internal SIP_Flow(SIP_Stack stack, TCP_ServerSession session)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		m_pStack = stack;
		m_pTcpSession = session;
		m_IsServer = true;
		m_pLocalEP = session.LocalEndPoint;
		m_pRemoteEP = session.RemoteEndPoint;
		m_Transport = (session.IsSecureConnection ? "TLS" : "TCP");
		m_CreateTime = DateTime.Now;
		m_LastActivity = DateTime.Now;
		m_ID = m_pLocalEP.ToString() + "-" + m_pRemoteEP.ToString() + "-" + m_Transport;
		m_pMessage = new MemoryStream();
		session.Disposed += delegate
		{
			Dispose();
		};
		BeginReadHeader();
	}

	private void Session_Disposed(object sender, EventArgs e)
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		lock (m_pLock)
		{
			if (!m_IsDisposed)
			{
				OnDisposing();
				m_IsDisposed = true;
				if (m_pTcpSession != null)
				{
					m_pTcpSession.Dispose();
					m_pTcpSession = null;
				}
				m_pMessage = null;
				if (m_pKeepAliveTimer != null)
				{
					m_pKeepAliveTimer.Dispose();
					m_pKeepAliveTimer = null;
				}
			}
		}
	}

	public void Send(SIP_Request request)
	{
		lock (m_pLock)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			SendInternal(request.ToByteData());
		}
	}

	public void Send(SIP_Response response)
	{
		lock (m_pLock)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			SendInternal(response.ToByteData());
			m_LastPing = DateTime.Now;
		}
	}

	public void SendPing()
	{
		lock (m_pLock)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_pStack.TransportLayer.Stack.Logger != null)
			{
				m_pStack.TransportLayer.Stack.Logger.AddWrite("", null, 2L, "Flow [id='" + ID + "'] sent \"ping\"", LocalEP, RemoteEP);
			}
			SendInternal(new byte[4] { 13, 10, 13, 10 });
		}
	}

	internal void Start()
	{
		AutoResetEvent startLock = new AutoResetEvent(initialState: false);
		ThreadPool.QueueUserWorkItem(delegate
		{
			lock (m_pLock)
			{
				startLock.Set();
				if (!m_IsServer && m_Transport != "UDP")
				{
					try
					{
						TCP_Client tCP_Client = new TCP_Client();
						tCP_Client.Connect(m_pLocalEP, m_pRemoteEP, m_Transport == "TLS");
						m_pTcpSession = tCP_Client;
						BeginReadHeader();
						return;
					}
					catch
					{
						Dispose();
						return;
					}
				}
			}
		});
		startLock.WaitOne();
		startLock.Close();
	}

	internal void SendInternal(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		try
		{
			if (m_Transport == "UDP")
			{
				m_pStack.TransportLayer.UdpServer.SendPacket(m_pLocalEP, data, 0, data.Length, m_pRemoteEP);
			}
			else if (m_Transport == "TCP")
			{
				m_pTcpSession.TcpStream.Write(data, 0, data.Length);
			}
			else if (m_Transport == "TLS")
			{
				m_pTcpSession.TcpStream.Write(data, 0, data.Length);
			}
			m_BytesWritten += data.Length;
		}
		catch (IOException ex)
		{
			Dispose();
			throw ex;
		}
	}

	private void BeginReadHeader()
	{
		m_pMessage.SetLength(0L);
		m_pTcpSession.TcpStream.BeginReadHeader(m_pMessage, m_pStack.TransportLayer.Stack.MaximumMessageSize, SizeExceededAction.JunkAndThrowException, BeginReadHeader_Completed, null);
	}

	private void BeginReadHeader_Completed(IAsyncResult asyncResult)
	{
		try
		{
			if (m_pTcpSession.TcpStream.EndReadHeader(asyncResult) == 0)
			{
				if (IsServer)
				{
					if (m_LastCRLF)
					{
						m_LastCRLF = false;
						m_pStack.TransportLayer.OnMessageReceived(this, new byte[4] { 13, 10, 13, 10 });
					}
					else
					{
						m_LastCRLF = true;
					}
				}
				else
				{
					m_pStack.TransportLayer.OnMessageReceived(this, new byte[2] { 13, 10 });
				}
				BeginReadHeader();
				return;
			}
			m_LastCRLF = false;
			m_pMessage.Write(new byte[2] { 13, 10 }, 0, 2);
			m_pMessage.Position = 0L;
			string text = MIME_Utils.ParseHeaderField("Content-Length:", m_pMessage);
			m_pMessage.Position = m_pMessage.Length;
			int num = 0;
			if (text != "")
			{
				num = Convert.ToInt32(text);
			}
			if (num > 0)
			{
				m_pTcpSession.TcpStream.BeginReadFixedCount(m_pMessage, num, BeginReadData_Completed, null);
				return;
			}
			byte[] message = m_pMessage.ToArray();
			BeginReadHeader();
			m_pStack.TransportLayer.OnMessageReceived(this, message);
		}
		catch
		{
			Dispose();
		}
	}

	private void BeginReadData_Completed(IAsyncResult asyncResult)
	{
		try
		{
			m_pTcpSession.TcpStream.EndReadFixedCount(asyncResult);
			byte[] message = m_pMessage.ToArray();
			BeginReadHeader();
			m_pStack.TransportLayer.OnMessageReceived(this, message);
		}
		catch
		{
			Dispose();
		}
	}

	internal void OnUdpPacketReceived(UDP_e_PacketReceived e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		m_LastActivity = DateTime.Now;
		byte[] array = new byte[e.Count];
		Array.Copy(e.Buffer, array, e.Count);
		m_pStack.TransportLayer.OnMessageReceived(this, array);
	}

	private void OnDisposing()
	{
		if (this.IsDisposing != null)
		{
			this.IsDisposing(this, new EventArgs());
		}
	}
}
