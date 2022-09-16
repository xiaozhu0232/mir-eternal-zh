using System;

namespace LumiSoft.Net.DNS.Client;

public class DNS_ClientException : Exception
{
	private DNS_RCode m_RCode;

	public DNS_RCode ErrorCode => m_RCode;

	public DNS_ClientException(DNS_RCode rcode)
		: base("Dns error: " + rcode.ToString() + ".")
	{
		m_RCode = rcode;
	}
}
