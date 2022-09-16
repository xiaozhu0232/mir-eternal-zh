using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LumiSoft.Net.ICMP;

public class Icmp
{
	public static EchoMessage[] Trace(string destIP)
	{
		return Trace(IPAddress.Parse(destIP), 2000);
	}

	public static EchoMessage[] Trace(IPAddress ip, int timeout)
	{
		List<EchoMessage> list = new List<EchoMessage>();
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
		IPEndPoint remoteEP = new IPEndPoint(ip, 80);
		EndPoint remoteEP2 = new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 80);
		byte[] array = CreatePacket((ushort)DateTime.Now.Millisecond);
		int num = 0;
		for (int num2 = 1; num2 <= 30; num2++)
		{
			byte[] array2 = new byte[1024];
			try
			{
				socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, num2);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
				DateTime now = DateTime.Now;
				socket.SendTo(array, array.Length, SocketFlags.None, remoteEP);
				socket.ReceiveFrom(array2, array2.Length, SocketFlags.None, ref remoteEP2);
				list.Add(new EchoMessage(time: (DateTime.Now - now).Milliseconds, ip: ((IPEndPoint)remoteEP2).Address, ttl: num2));
				if (array2[20] == 0)
				{
					break;
				}
				if (array2[20] != 11)
				{
					throw new Exception("UnKnown error !");
				}
				num = 0;
				goto IL_0108;
			}
			catch
			{
				num++;
				goto IL_0108;
			}
			IL_0108:
			if (num >= 3)
			{
				break;
			}
		}
		return list.ToArray();
	}

	public static EchoMessage Ping(IPAddress ip, int timeout)
	{
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
		IPEndPoint remoteEP = new IPEndPoint(ip, 80);
		EndPoint remoteEP2 = new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 80);
		byte[] array = CreatePacket((ushort)DateTime.Now.Millisecond);
		socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, 30);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
		DateTime now = DateTime.Now;
		socket.SendTo(array, array.Length, SocketFlags.None, remoteEP);
		byte[] array2 = new byte[1024];
		socket.ReceiveFrom(array2, array2.Length, SocketFlags.None, ref remoteEP2);
		if (array2[20] != 0 && array2[20] != 11)
		{
			throw new Exception("UnKnown error !");
		}
		return new EchoMessage(time: (DateTime.Now - now).Milliseconds, ip: ((IPEndPoint)remoteEP2).Address, ttl: 0);
	}

	private static byte[] CreatePacket(ushort id)
	{
		byte[] array = new byte[10] { 8, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		Array.Copy(BitConverter.GetBytes(id), 0, array, 4, 2);
		for (int i = 0; i < 2; i++)
		{
			array[i + 8] = 120;
		}
		int num = 0;
		for (int j = 0; j < array.Length; j += 2)
		{
			num += Convert.ToInt32(BitConverter.ToUInt16(array, j));
		}
		num &= 0xFFFF;
		Array.Copy(BitConverter.GetBytes((ushort)(~num)), 0, array, 2, 2);
		return array;
	}
}
