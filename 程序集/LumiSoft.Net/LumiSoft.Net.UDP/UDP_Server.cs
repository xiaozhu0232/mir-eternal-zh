using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LumiSoft.Net.UDP;

public class UDP_Server : IDisposable
{
	private bool m_IsDisposed;

	private bool m_IsRunning;

	private int m_MTU = 1400;

	private IPEndPoint[] m_pBindings;

	private DateTime m_StartTime;

	private List<Socket> m_pSockets;

	private CircleCollection<Socket> m_pSendSocketsIPv4;

	private CircleCollection<Socket> m_pSendSocketsIPv6;

	private int m_ReceiversPerSocket = 10;

	private List<UDP_DataReceiver> m_pDataReceivers;

	private long m_BytesReceived;

	private long m_PacketsReceived;

	private long m_BytesSent;

	private long m_PacketsSent;

	public bool IsDisposed => m_IsDisposed;

	public bool IsRunning
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			return m_IsRunning;
		}
	}

	public int MTU
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			return m_MTU;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			if (m_IsRunning)
			{
				throw new InvalidOperationException("MTU value can be changed only if UDP server is not running.");
			}
			m_MTU = value;
		}
	}

	public IPEndPoint[] Bindings
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			return m_pBindings;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			bool flag = false;
			if (m_pBindings == null)
			{
				flag = true;
			}
			else if (m_pBindings.Length != value.Length)
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
				Restart();
			}
		}
	}

	public DateTime StartTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("UDP server is not running.");
			}
			return m_StartTime;
		}
	}

	public long BytesReceived
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("UDP server is not running.");
			}
			return m_BytesReceived;
		}
	}

	public long PacketsReceived
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("UDP server is not running.");
			}
			return m_PacketsReceived;
		}
	}

	public long BytesSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("UDP server is not running.");
			}
			return m_BytesSent;
		}
	}

	public long PacketsSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			if (!m_IsRunning)
			{
				throw new InvalidOperationException("UDP server is not running.");
			}
			return m_PacketsSent;
		}
	}

	public event EventHandler<UDP_e_PacketReceived> PacketReceived;

	public event ErrorEventHandler Error;

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = false;
			Stop();
			this.Error = null;
			this.PacketReceived = null;
		}
	}

	public void Start()
	{
		if (m_IsRunning)
		{
			return;
		}
		m_IsRunning = true;
		m_StartTime = DateTime.Now;
		m_pDataReceivers = new List<UDP_DataReceiver>();
		if (m_pBindings == null)
		{
			return;
		}
		List<IPEndPoint> list = new List<IPEndPoint>();
		IPEndPoint[] pBindings = m_pBindings;
		foreach (IPEndPoint iPEndPoint in pBindings)
		{
			if (iPEndPoint.Address.Equals(IPAddress.Any))
			{
				IPEndPoint item = new IPEndPoint(IPAddress.Loopback, iPEndPoint.Port);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
				IPAddress[] hostAddresses = Dns.GetHostAddresses("");
				for (int j = 0; j < hostAddresses.Length; j++)
				{
					IPEndPoint item2 = new IPEndPoint(hostAddresses[j], iPEndPoint.Port);
					if (!list.Contains(item2))
					{
						list.Add(item2);
					}
				}
			}
			else if (!list.Contains(iPEndPoint))
			{
				list.Add(iPEndPoint);
			}
		}
		m_pSockets = new List<Socket>();
		foreach (IPEndPoint item3 in list)
		{
			try
			{
				Socket socket = Net_Utils.CreateSocket(item3, ProtocolType.Udp);
				m_pSockets.Add(socket);
				for (int k = 0; k < m_ReceiversPerSocket; k++)
				{
					UDP_DataReceiver uDP_DataReceiver = new UDP_DataReceiver(socket);
					uDP_DataReceiver.PacketReceived += delegate(object s, UDP_e_PacketReceived e)
					{
						try
						{
							ProcessUdpPacket(e);
						}
						catch (Exception x2)
						{
							OnError(x2);
						}
					};
					uDP_DataReceiver.Error += delegate(object s, ExceptionEventArgs e)
					{
						OnError(e.Exception);
					};
					m_pDataReceivers.Add(uDP_DataReceiver);
					uDP_DataReceiver.Start();
				}
			}
			catch (Exception x)
			{
				OnError(x);
			}
		}
		m_pSendSocketsIPv4 = new CircleCollection<Socket>();
		m_pSendSocketsIPv6 = new CircleCollection<Socket>();
		foreach (Socket pSocket in m_pSockets)
		{
			if (((IPEndPoint)pSocket.LocalEndPoint).AddressFamily == AddressFamily.InterNetwork)
			{
				if (!((IPEndPoint)pSocket.LocalEndPoint).Address.Equals(IPAddress.Loopback))
				{
					m_pSendSocketsIPv4.Add(pSocket);
				}
			}
			else if (((IPEndPoint)pSocket.LocalEndPoint).AddressFamily == AddressFamily.InterNetworkV6)
			{
				m_pSendSocketsIPv6.Add(pSocket);
			}
		}
	}

	public void Stop()
	{
		if (!m_IsRunning)
		{
			return;
		}
		m_IsRunning = false;
		foreach (UDP_DataReceiver pDataReceiver in m_pDataReceivers)
		{
			pDataReceiver.Dispose();
		}
		m_pDataReceivers = null;
		foreach (Socket pSocket in m_pSockets)
		{
			pSocket.Close();
		}
		m_pSockets = null;
		m_pSendSocketsIPv4 = null;
		m_pSendSocketsIPv6 = null;
	}

	public void Restart()
	{
		if (m_IsRunning)
		{
			Stop();
			Start();
		}
	}

	public void SendPacket(byte[] packet, int offset, int count, IPEndPoint remoteEP)
	{
		IPEndPoint localEP = null;
		SendPacket(packet, offset, count, remoteEP, out localEP);
	}

	public void SendPacket(byte[] packet, int offset, int count, IPEndPoint remoteEP, out IPEndPoint localEP)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("UdpServer");
		}
		if (!m_IsRunning)
		{
			throw new InvalidOperationException("UDP server is not running.");
		}
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		localEP = null;
		SendPacket(null, packet, offset, count, remoteEP, out localEP);
	}

	public void SendPacket(IPEndPoint localEP, byte[] packet, int offset, int count, IPEndPoint remoteEP)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("UdpServer");
		}
		if (!m_IsRunning)
		{
			throw new InvalidOperationException("UDP server is not running.");
		}
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		if (localEP.AddressFamily != remoteEP.AddressFamily)
		{
			throw new ArgumentException("Argumnet localEP and remoteEP AddressFamily won't match.");
		}
		Socket socket = null;
		if (localEP.AddressFamily == AddressFamily.InterNetwork)
		{
			Socket[] array = m_pSendSocketsIPv4.ToArray();
			foreach (Socket socket2 in array)
			{
				if (localEP.Equals((IPEndPoint)socket2.LocalEndPoint))
				{
					socket = socket2;
					break;
				}
			}
		}
		else
		{
			if (localEP.AddressFamily != AddressFamily.InterNetworkV6)
			{
				throw new ArgumentException("Argument 'localEP' has unknown AddressFamily.");
			}
			Socket[] array = m_pSendSocketsIPv6.ToArray();
			foreach (Socket socket3 in array)
			{
				if (localEP.Equals((IPEndPoint)socket3.LocalEndPoint))
				{
					socket = socket3;
					break;
				}
			}
		}
		if (socket == null)
		{
			throw new ArgumentException("Specified local end point '" + localEP?.ToString() + "' doesn't exist.");
		}
		IPEndPoint localEP2 = null;
		SendPacket(socket, packet, offset, count, remoteEP, out localEP2);
	}

	internal void SendPacket(Socket socket, byte[] packet, int offset, int count, IPEndPoint remoteEP, out IPEndPoint localEP)
	{
		if (socket == null)
		{
			if (remoteEP.AddressFamily == AddressFamily.InterNetwork)
			{
				if (m_pSendSocketsIPv4.Count == 0)
				{
					throw new ArgumentException("There is no suitable IPv4 local end point in this.Bindings.");
				}
				socket = m_pSendSocketsIPv4.Next();
			}
			else
			{
				if (remoteEP.AddressFamily != AddressFamily.InterNetworkV6)
				{
					throw new ArgumentException("Invalid remote end point address family.");
				}
				if (m_pSendSocketsIPv6.Count == 0)
				{
					throw new ArgumentException("There is no suitable IPv6 local end point in this.Bindings.");
				}
				socket = m_pSendSocketsIPv6.Next();
			}
		}
		socket.SendTo(packet, 0, count, SocketFlags.None, remoteEP);
		localEP = (IPEndPoint)socket.LocalEndPoint;
		m_BytesSent += count;
		m_PacketsSent++;
	}

	public IPEndPoint GetLocalEndPoint(IPEndPoint remoteEP)
	{
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		if (remoteEP.AddressFamily == AddressFamily.InterNetwork)
		{
			if (m_pSendSocketsIPv4.Count == 0)
			{
				throw new InvalidOperationException("There is no suitable IPv4 local end point in this.Bindings.");
			}
			return (IPEndPoint)m_pSendSocketsIPv4.Next().LocalEndPoint;
		}
		if (remoteEP.AddressFamily == AddressFamily.InterNetworkV6)
		{
			if (m_pSendSocketsIPv6.Count == 0)
			{
				throw new InvalidOperationException("There is no suitable IPv6 local end point in this.Bindings.");
			}
			return (IPEndPoint)m_pSendSocketsIPv6.Next().LocalEndPoint;
		}
		throw new ArgumentException("Argument 'remoteEP' has unknown AddressFamily.");
	}

	private void ProcessUdpPacket(UDP_e_PacketReceived e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		OnUdpPacketReceived(e);
	}

	private void OnUdpPacketReceived(UDP_e_PacketReceived e)
	{
		if (this.PacketReceived != null)
		{
			this.PacketReceived(this, e);
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
