using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LumiSoft.Net.UPnP.Client;

public class UPnP_Client
{
	public UPnP_Device[] Search(int timeout)
	{
		return Search("upnp:rootdevice", timeout);
	}

	public UPnP_Device[] Search(string deviceType, int timeout)
	{
		if (timeout < 1)
		{
			timeout = 1;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("M-SEARCH * HTTP/1.1\r\n");
		stringBuilder.Append("HOST: 239.255.255.250:1900\r\n");
		stringBuilder.Append("MAN: \"ssdp:discover\"\r\n");
		stringBuilder.Append("MX: 1\r\n");
		stringBuilder.Append("ST: " + deviceType + "\r\n");
		stringBuilder.Append("\r\n");
		using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 2);
		socket.SendTo(Encoding.UTF8.GetBytes(stringBuilder.ToString()), new IPEndPoint(IPAddress.Broadcast, 1900));
		List<string> list = new List<string>();
		byte[] array = new byte[32000];
		DateTime now = DateTime.Now;
		while (now.AddMilliseconds(timeout) > DateTime.Now)
		{
			if (!socket.Poll(1, SelectMode.SelectRead))
			{
				continue;
			}
			int count = socket.Receive(array);
			string[] array2 = Encoding.UTF8.GetString(array, 0, count).Split('\n');
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array3 = array2[i].Split(new char[1] { ':' }, 2);
				if (string.Equals(array3[0], "location", StringComparison.InvariantCultureIgnoreCase))
				{
					list.Add(array3[1].Trim());
				}
			}
		}
		List<UPnP_Device> list2 = new List<UPnP_Device>();
		foreach (string item in list)
		{
			try
			{
				list2.Add(new UPnP_Device(item));
			}
			catch
			{
			}
		}
		return list2.ToArray();
	}

	public UPnP_Device[] Search(IPAddress ip, string deviceType, int timeout)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		if (timeout < 1)
		{
			timeout = 1;
		}
		using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 2);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("M-SEARCH * HTTP/1.1\r\n");
		stringBuilder.Append("MAN: \"ssdp:discover\"\r\n");
		stringBuilder.Append("MX: 1\r\n");
		stringBuilder.Append("ST: " + deviceType + "\r\n");
		stringBuilder.Append("\r\n");
		socket.SendTo(Encoding.UTF8.GetBytes(stringBuilder.ToString()), new IPEndPoint(ip, 1900));
		List<string> list = new List<string>();
		byte[] array = new byte[32000];
		DateTime now = DateTime.Now;
		while (now.AddMilliseconds(timeout) > DateTime.Now)
		{
			if (!socket.Poll(1, SelectMode.SelectRead))
			{
				continue;
			}
			int count = socket.Receive(array);
			string[] array2 = Encoding.UTF8.GetString(array, 0, count).Split('\n');
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array3 = array2[i].Split(new char[1] { ':' }, 2);
				if (string.Equals(array3[0], "location", StringComparison.InvariantCultureIgnoreCase))
				{
					list.Add(array3[1].Trim());
				}
			}
		}
		List<UPnP_Device> list2 = new List<UPnP_Device>();
		foreach (string item in list)
		{
			try
			{
				list2.Add(new UPnP_Device(item));
			}
			catch
			{
			}
		}
		return list2.ToArray();
	}
}
