using System;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_NAPTR : DNS_rr
{
	private int m_Order;

	private int m_Preference;

	private string m_Flags = "";

	private string m_Services = "";

	private string m_Regexp = "";

	private string m_Replacement = "";

	public int Order => m_Order;

	public int Preference => m_Preference;

	public string Flags => m_Flags;

	public string Services => m_Services;

	public string Regexp => m_Regexp;

	public string Replacement => m_Replacement;

	public DNS_rr_NAPTR(string name, int order, int preference, string flags, string services, string regexp, string replacement, int ttl)
		: base(name, DNS_QType.NAPTR, ttl)
	{
		m_Order = order;
		m_Preference = preference;
		m_Flags = flags;
		m_Services = services;
		m_Regexp = regexp;
		m_Replacement = replacement;
	}

	public static DNS_rr_NAPTR Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		int order = (reply[offset++] << 8) | reply[offset++];
		int preference = (reply[offset++] << 8) | reply[offset++];
		string flags = Dns_Client.ReadCharacterString(reply, ref offset);
		string services = Dns_Client.ReadCharacterString(reply, ref offset);
		string regexp = Dns_Client.ReadCharacterString(reply, ref offset);
		string name2 = "";
		Dns_Client.GetQName(reply, ref offset, ref name2);
		return new DNS_rr_NAPTR(name, order, preference, flags, services, regexp, name2, ttl);
	}
}
