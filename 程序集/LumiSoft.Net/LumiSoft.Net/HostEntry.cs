using System;
using System.Net;

namespace LumiSoft.Net;

public class HostEntry
{
	private string m_HostName;

	private IPAddress[] m_pAddresses;

	private string[] m_pAliases;

	public string HostName => m_HostName;

	public IPAddress[] Addresses => m_pAddresses;

	public string[] Aliases => m_pAliases;

	public HostEntry(string hostName, IPAddress[] ipAddresses, string[] aliases)
	{
		if (hostName == null)
		{
			throw new ArgumentNullException("hostName");
		}
		if (hostName == string.Empty)
		{
			throw new ArgumentException("Argument 'hostName' value must be specified.", "hostName");
		}
		if (ipAddresses == null)
		{
			throw new ArgumentNullException("ipAddresses");
		}
		m_HostName = hostName;
		m_pAddresses = ipAddresses;
		m_pAliases = ((aliases == null) ? new string[0] : aliases);
	}
}
