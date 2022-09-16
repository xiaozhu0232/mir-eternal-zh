using System;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_SPF : DNS_rr
{
	private string m_Text = "";

	public string Text => m_Text;

	public DNS_rr_SPF(string name, string text, int ttl)
		: base(name, DNS_QType.SPF, ttl)
	{
	}

	public static DNS_rr_SPF Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		string text = Dns_Client.ReadCharacterString(reply, ref offset);
		return new DNS_rr_SPF(name, text, ttl);
	}
}
