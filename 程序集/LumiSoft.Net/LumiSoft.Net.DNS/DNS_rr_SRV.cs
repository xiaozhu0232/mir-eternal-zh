using System;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_SRV : DNS_rr
{
	private int m_Priority = 1;

	private int m_Weight = 1;

	private int m_Port;

	private string m_Target = "";

	public int Priority => m_Priority;

	public int Weight => m_Weight;

	public int Port => m_Port;

	public string Target => m_Target;

	public DNS_rr_SRV(string name, int priority, int weight, int port, string target, int ttl)
		: base(name, DNS_QType.SRV, ttl)
	{
		m_Priority = priority;
		m_Weight = weight;
		m_Port = port;
		m_Target = target;
	}

	public static DNS_rr_SRV Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		int priority = (reply[offset++] << 8) | reply[offset++];
		int weight = (reply[offset++] << 8) | reply[offset++];
		int port = (reply[offset++] << 8) | reply[offset++];
		string name2 = "";
		Dns_Client.GetQName(reply, ref offset, ref name2);
		return new DNS_rr_SRV(name, priority, weight, port, name2, ttl);
	}
}
