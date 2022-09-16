using System;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_PTR : DNS_rr
{
	private string m_DomainName = "";

	public string DomainName => m_DomainName;

	public DNS_rr_PTR(string name, string domainName, int ttl)
		: base(name, DNS_QType.PTR, ttl)
	{
		m_DomainName = domainName;
	}

	public static DNS_rr_PTR Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		string name2 = "";
		if (Dns_Client.GetQName(reply, ref offset, ref name2))
		{
			return new DNS_rr_PTR(name, name2, ttl);
		}
		throw new ArgumentException("Invalid PTR resource record data !");
	}
}
