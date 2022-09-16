using System;
using System.Net;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Hop
{
	private IPEndPoint m_pEndPoint;

	private string m_Transport = "";

	public IPEndPoint EndPoint => m_pEndPoint;

	public IPAddress IP => m_pEndPoint.Address;

	public int Port => m_pEndPoint.Port;

	public string Transport => m_Transport;

	public SIP_Hop(IPEndPoint ep, string transport)
	{
		if (ep == null)
		{
			throw new ArgumentNullException("ep");
		}
		if (transport == null)
		{
			throw new ArgumentNullException("transport");
		}
		if (transport == "")
		{
			throw new ArgumentException("Argument 'transport' value must be specified.");
		}
		m_pEndPoint = ep;
		m_Transport = transport;
	}

	public SIP_Hop(IPAddress ip, int port, string transport)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		if (port < 1)
		{
			throw new ArgumentException("Argument 'port' value must be >= 1.");
		}
		if (transport == null)
		{
			throw new ArgumentNullException("transport");
		}
		if (transport == "")
		{
			throw new ArgumentException("Argument 'transport' value must be specified.");
		}
		m_pEndPoint = new IPEndPoint(ip, port);
		m_Transport = transport;
	}
}
