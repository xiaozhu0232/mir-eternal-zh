using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using LumiSoft.Net.STUN.Message;

namespace LumiSoft.Net.STUN.Client;

public class STUN_Client
{
	public static STUN_Result Query(string host, int port, IPEndPoint localEP)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		socket.Bind(localEP);
		return Query(host, port, socket);
	}

	public static STUN_Result Query(string host, int port, Socket socket)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		if (port < 1)
		{
			throw new ArgumentException("Port value must be >= 1 !");
		}
		if (socket.ProtocolType != ProtocolType.Udp)
		{
			throw new ArgumentException("Socket must be UDP socket !");
		}
		IPEndPoint remoteEndPoint = new IPEndPoint(Dns.GetHostAddresses(host)[0], port);
		try
		{
			STUN_Message sTUN_Message = DoTransaction(new STUN_Message
			{
				Type = STUN_MessageType.BindingRequest
			}, socket, remoteEndPoint, 1600);
			if (sTUN_Message == null)
			{
				return new STUN_Result(STUN_NetType.UdpBlocked, null);
			}
			STUN_Message sTUN_Message2 = new STUN_Message();
			sTUN_Message2.Type = STUN_MessageType.BindingRequest;
			sTUN_Message2.ChangeRequest = new STUN_t_ChangeRequest(changeIP: true, changePort: true);
			if (socket.LocalEndPoint.Equals(sTUN_Message.MappedAddress))
			{
				if (DoTransaction(sTUN_Message2, socket, remoteEndPoint, 1600) != null)
				{
					return new STUN_Result(STUN_NetType.OpenInternet, sTUN_Message.MappedAddress);
				}
				return new STUN_Result(STUN_NetType.SymmetricUdpFirewall, sTUN_Message.MappedAddress);
			}
			if (DoTransaction(sTUN_Message2, socket, remoteEndPoint, 1600) != null)
			{
				return new STUN_Result(STUN_NetType.FullCone, sTUN_Message.MappedAddress);
			}
			STUN_Message sTUN_Message3 = DoTransaction(new STUN_Message
			{
				Type = STUN_MessageType.BindingRequest
			}, socket, sTUN_Message.ChangedAddress, 1600);
			if (sTUN_Message3 == null)
			{
				throw new Exception("STUN Test I(II) dind't get resonse !");
			}
			if (!sTUN_Message3.MappedAddress.Equals(sTUN_Message.MappedAddress))
			{
				return new STUN_Result(STUN_NetType.Symmetric, sTUN_Message.MappedAddress);
			}
			if (DoTransaction(new STUN_Message
			{
				Type = STUN_MessageType.BindingRequest,
				ChangeRequest = new STUN_t_ChangeRequest(changeIP: false, changePort: true)
			}, socket, sTUN_Message.ChangedAddress, 1600) != null)
			{
				return new STUN_Result(STUN_NetType.RestrictedCone, sTUN_Message.MappedAddress);
			}
			return new STUN_Result(STUN_NetType.PortRestrictedCone, sTUN_Message.MappedAddress);
		}
		finally
		{
			DateTime now = DateTime.Now;
			while (now.AddMilliseconds(200.0) > DateTime.Now)
			{
				if (socket.Poll(1, SelectMode.SelectRead))
				{
					byte[] buffer = new byte[512];
					socket.Receive(buffer);
				}
			}
		}
	}

	public static IPAddress GetPublicIP(string stunServer, int port, IPAddress localIP)
	{
		if (stunServer == null)
		{
			throw new ArgumentNullException("stunServer");
		}
		if (stunServer == "")
		{
			throw new ArgumentException("Argument 'stunServer' value must be specified.");
		}
		if (port < 1)
		{
			throw new ArgumentException("Invalid argument 'port' value.");
		}
		if (localIP == null)
		{
			throw new ArgumentNullException("localIP");
		}
		if (!Net_Utils.IsPrivateIP(localIP))
		{
			return localIP;
		}
		STUN_Result sTUN_Result = Query(stunServer, port, Net_Utils.CreateSocket(new IPEndPoint(localIP, 0), ProtocolType.Udp));
		if (sTUN_Result.PublicEndPoint != null)
		{
			return sTUN_Result.PublicEndPoint.Address;
		}
		throw new IOException("Failed to STUN public IP address. STUN server name is invalid or firewall blocks STUN.");
	}

	public static IPEndPoint GetPublicEP(string stunServer, int port, Socket socket)
	{
		if (stunServer == null)
		{
			throw new ArgumentNullException("stunServer");
		}
		if (stunServer == "")
		{
			throw new ArgumentException("Argument 'stunServer' value must be specified.");
		}
		if (port < 1)
		{
			throw new ArgumentException("Invalid argument 'port' value.");
		}
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		if (socket.ProtocolType != ProtocolType.Udp)
		{
			throw new ArgumentException("Socket must be UDP socket !");
		}
		IPEndPoint remoteEndPoint = new IPEndPoint(Dns.GetHostAddresses(stunServer)[0], port);
		try
		{
			return (DoTransaction(new STUN_Message
			{
				Type = STUN_MessageType.BindingRequest
			}, socket, remoteEndPoint, 1000) ?? throw new IOException("Failed to STUN public IP address. STUN server name is invalid or firewall blocks STUN.")).SourceAddress;
		}
		catch
		{
			throw new IOException("Failed to STUN public IP address. STUN server name is invalid or firewall blocks STUN.");
		}
		finally
		{
			DateTime now = DateTime.Now;
			while (now.AddMilliseconds(200.0) > DateTime.Now)
			{
				if (socket.Poll(1, SelectMode.SelectRead))
				{
					byte[] buffer = new byte[512];
					socket.Receive(buffer);
				}
			}
		}
	}

	private void GetSharedSecret()
	{
	}

	private static STUN_Message DoTransaction(STUN_Message request, Socket socket, IPEndPoint remoteEndPoint, int timeout)
	{
		byte[] buffer = request.ToByteData();
		DateTime now = DateTime.Now;
		while (now.AddMilliseconds(timeout) > DateTime.Now)
		{
			try
			{
				socket.SendTo(buffer, remoteEndPoint);
				if (socket.Poll(500000, SelectMode.SelectRead))
				{
					byte[] array = new byte[512];
					socket.Receive(array);
					STUN_Message sTUN_Message = new STUN_Message();
					sTUN_Message.Parse(array);
					if (Net_Utils.CompareArray(request.TransactionID, sTUN_Message.TransactionID))
					{
						return sTUN_Message;
					}
				}
			}
			catch
			{
			}
		}
		return null;
	}
}
