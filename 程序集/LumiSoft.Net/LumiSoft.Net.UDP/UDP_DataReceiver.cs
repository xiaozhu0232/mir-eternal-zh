using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LumiSoft.Net.UDP;

public class UDP_DataReceiver : IDisposable
{
	private bool m_IsDisposed;

	private bool m_IsRunning;

	private Socket m_pSocket;

	private byte[] m_pBuffer;

	private int m_BufferSize = 8000;

	private SocketAsyncEventArgs m_pSocketArgs;

	private UDP_e_PacketReceived m_pEventArgs;

	public event EventHandler<UDP_e_PacketReceived> PacketReceived;

	public event EventHandler<ExceptionEventArgs> Error;

	public UDP_DataReceiver(Socket socket)
	{
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		m_pSocket = socket;
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			m_pSocket = null;
			m_pBuffer = null;
			if (m_pSocketArgs != null)
			{
				m_pSocketArgs.Dispose();
				m_pSocketArgs = null;
			}
			m_pEventArgs = null;
			this.PacketReceived = null;
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
		bool isIoCompletionSupported = Net_Utils.IsSocketAsyncSupported();
		m_pEventArgs = new UDP_e_PacketReceived();
		m_pBuffer = new byte[m_BufferSize];
		if (isIoCompletionSupported)
		{
			m_pSocketArgs = new SocketAsyncEventArgs();
			m_pSocketArgs.SetBuffer(m_pBuffer, 0, m_BufferSize);
			m_pSocketArgs.RemoteEndPoint = new IPEndPoint((m_pSocket.AddressFamily == AddressFamily.InterNetwork) ? IPAddress.Any : IPAddress.IPv6Any, 0);
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
						OnPacketReceived(m_pBuffer, m_pSocketArgs.BytesTransferred, (IPEndPoint)m_pSocketArgs.RemoteEndPoint);
					}
					else
					{
						OnError(new Exception("Socket error '" + m_pSocketArgs.SocketError.ToString() + "'."));
					}
					IOCompletionReceive();
				}
				catch (Exception x2)
				{
					OnError(x2);
				}
			};
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			if (m_IsDisposed)
			{
				return;
			}
			try
			{
				if (isIoCompletionSupported)
				{
					IOCompletionReceive();
				}
				else
				{
					EndPoint remoteEP = new IPEndPoint((m_pSocket.AddressFamily == AddressFamily.InterNetwork) ? IPAddress.Any : IPAddress.IPv6Any, 0);
					m_pSocket.BeginReceiveFrom(m_pBuffer, 0, m_BufferSize, SocketFlags.None, ref remoteEP, AsyncSocketReceive, null);
				}
			}
			catch (Exception x)
			{
				OnError(x);
			}
		});
	}

	private void IOCompletionReceive()
	{
		try
		{
			while (!m_IsDisposed && !m_pSocket.ReceiveFromAsync(m_pSocketArgs))
			{
				if (m_pSocketArgs.SocketError == SocketError.Success)
				{
					try
					{
						OnPacketReceived(m_pBuffer, m_pSocketArgs.BytesTransferred, (IPEndPoint)m_pSocketArgs.RemoteEndPoint);
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
				m_pSocketArgs.RemoteEndPoint = new IPEndPoint((m_pSocket.AddressFamily == AddressFamily.InterNetwork) ? IPAddress.Any : IPAddress.IPv6Any, 0);
			}
		}
		catch (Exception x2)
		{
			OnError(x2);
		}
	}

	private void AsyncSocketReceive(IAsyncResult ar)
	{
		if (m_IsDisposed)
		{
			return;
		}
		try
		{
			EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
			int count = m_pSocket.EndReceiveFrom(ar, ref endPoint);
			OnPacketReceived(m_pBuffer, count, (IPEndPoint)endPoint);
		}
		catch (Exception x)
		{
			OnError(x);
		}
		try
		{
			EndPoint remoteEP = new IPEndPoint((m_pSocket.AddressFamily == AddressFamily.InterNetwork) ? IPAddress.Any : IPAddress.IPv6Any, 0);
			m_pSocket.BeginReceiveFrom(m_pBuffer, 0, m_BufferSize, SocketFlags.None, ref remoteEP, AsyncSocketReceive, null);
		}
		catch (Exception x2)
		{
			OnError(x2);
		}
	}

	private void OnPacketReceived(byte[] buffer, int count, IPEndPoint remoteEP)
	{
		if (this.PacketReceived != null)
		{
			m_pEventArgs.Reuse(m_pSocket, buffer, count, remoteEP);
			this.PacketReceived(this, m_pEventArgs);
		}
	}

	private void OnError(Exception x)
	{
		if (!m_IsDisposed && this.Error != null)
		{
			this.Error(this, new ExceptionEventArgs(x));
		}
	}
}
