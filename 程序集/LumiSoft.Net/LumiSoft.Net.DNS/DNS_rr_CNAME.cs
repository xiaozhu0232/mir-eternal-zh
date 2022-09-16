using System;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_CNAME : DNS_rr
{
	private string m_Alias = "";

	public string Alias => m_Alias;

	public DNS_rr_CNAME(string name, string alias, int ttl)
		: base(name, DNS_QType.CNAME, ttl)
	{
		m_Alias = alias;
	}

	public static DNS_rr_CNAME Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		string name2 = "";
		if (Dns_Client.GetQName(reply, ref offset, ref name2))
		{
			return new DNS_rr_CNAME(name, name2, ttl);
		}
		throw new ArgumentException("Invalid CNAME resource record data !");
	}
}
