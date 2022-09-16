using System;
using System.Net;

namespace LumiSoft.Net;

public class HostEndPoint
{
	private string m_Host = "";

	private int m_Port;

	public bool IsIPAddress => Net_Utils.IsIPAddress(m_Host);

	public string Host => m_Host;

	public int Port => m_Port;

	public HostEndPoint(string host, int port)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (host == "")
		{
			throw new ArgumentException("Argument 'host' value must be specified.");
		}
		m_Host = host;
		m_Port = port;
	}

	public HostEndPoint(IPEndPoint endPoint)
	{
		if (endPoint == null)
		{
			throw new ArgumentNullException("endPoint");
		}
		m_Host = endPoint.Address.ToString();
		m_Port = endPoint.Port;
	}

	public static HostEndPoint Parse(string value)
	{
		return Parse(value, -1);
	}

	public static HostEndPoint Parse(string value, int defaultPort)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value == "")
		{
			throw new ArgumentException("Argument 'value' value must be specified.");
		}
		if (value.IndexOf(':') > -1)
		{
			string[] array = value.Split(new char[1] { ':' }, 2);
			try
			{
				return new HostEndPoint(array[0], Convert.ToInt32(array[1]));
			}
			catch
			{
				throw new ArgumentException("Argument 'value' has invalid value.");
			}
		}
		return new HostEndPoint(value, defaultPort);
	}

	public override string ToString()
	{
		if (m_Port == -1)
		{
			return m_Host;
		}
		return m_Host + ":" + m_Port;
	}
}
