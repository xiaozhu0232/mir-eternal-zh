using System;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_NS : DNS_rr
{
	private string m_NameServer = "";

	public string NameServer => m_NameServer;

	public DNS_rr_NS(string name, string nameServer, int ttl)
		: base(name, DNS_QType.NS, ttl)
	{
		m_NameServer = nameServer;
	}

	public static DNS_rr_NS Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		string name2 = "";
		if (Dns_Client.GetQName(reply, ref offset, ref name2))
		{
			return new DNS_rr_NS(name, name2, ttl);
		}
		throw new ArgumentException("Invalid NS resource record data !");
	}
}
