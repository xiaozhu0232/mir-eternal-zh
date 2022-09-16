using System;
using System.Net;
using System.Net.Sockets;

namespace LumiSoft.Net.UDP;

public class UDP_e_PacketReceived : EventArgs
{
	private Socket m_pSocket;

	private byte[] m_pBuffer;

	private int m_Count;

	private IPEndPoint m_pRemoteEP;

	public Socket Socket => m_pSocket;

	public byte[] Buffer => m_pBuffer;

	public int Count => m_Count;

	public IPEndPoint RemoteEP => m_pRemoteEP;

	internal UDP_e_PacketReceived()
	{
	}

	internal void Reuse(Socket socket, byte[] buffer, int count, IPEndPoint remoteEP)
	{
		m_pSocket = socket;
		m_pBuffer = buffer;
		m_Count = count;
		m_pRemoteEP = remoteEP;
	}
}
