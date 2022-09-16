using System;
using System.Net;

namespace LumiSoft.Net.Mail;

public class Mail_t_TcpInfo
{
	private IPAddress m_pIP;

	private string m_HostName;

	public IPAddress IP => m_pIP;

	public string HostName => m_HostName;

	public Mail_t_TcpInfo(IPAddress ip, string hostName)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		m_pIP = ip;
		m_HostName = hostName;
	}

	public override string ToString()
	{
		if (string.IsNullOrEmpty(m_HostName))
		{
			return "[" + m_pIP.ToString() + "]";
		}
		return m_HostName + " [" + m_pIP.ToString() + "]";
	}
}
