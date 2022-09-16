using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

public class DNS_rr_HINFO : DNS_rr
{
	private string m_CPU = "";

	private string m_OS = "";

	public string CPU => m_CPU;

	public string OS => m_OS;

	public DNS_rr_HINFO(string name, string cpu, string os, int ttl)
		: base(name, DNS_QType.HINFO, ttl)
	{
		m_CPU = cpu;
		m_OS = os;
	}

	public static DNS_rr_HINFO Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		string cpu = Dns_Client.ReadCharacterString(reply, ref offset);
		string os = Dns_Client.ReadCharacterString(reply, ref offset);
		return new DNS_rr_HINFO(name, cpu, os, ttl);
	}
}
