using System;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_SOA : DNS_rr
{
	private string m_NameServer = "";

	private string m_AdminEmail = "";

	private long m_Serial;

	private long m_Refresh;

	private long m_Retry;

	private long m_Expire;

	private long m_Minimum;

	public string NameServer => m_NameServer;

	public string AdminEmail => m_AdminEmail;

	public long Serial => m_Serial;

	public long Refresh => m_Refresh;

	public long Retry => m_Retry;

	public long Expire => m_Expire;

	public long Minimum => m_Minimum;

	public DNS_rr_SOA(string name, string nameServer, string adminEmail, long serial, long refresh, long retry, long expire, long minimum, int ttl)
		: base(name, DNS_QType.SOA, ttl)
	{
		m_NameServer = nameServer;
		m_AdminEmail = adminEmail;
		m_Serial = serial;
		m_Refresh = refresh;
		m_Retry = retry;
		m_Expire = expire;
		m_Minimum = minimum;
	}

	public static DNS_rr_SOA Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		string name2 = "";
		Dns_Client.GetQName(reply, ref offset, ref name2);
		string name3 = "";
		Dns_Client.GetQName(reply, ref offset, ref name3);
		char[] array = name3.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '.')
			{
				array[i] = '@';
				break;
			}
		}
		name3 = new string(array);
		long serial = (reply[offset++] << 24) | (reply[offset++] << 16) | (reply[offset++] << 8) | reply[offset++];
		long refresh = (reply[offset++] << 24) | (reply[offset++] << 16) | (reply[offset++] << 8) | reply[offset++];
		long retry = (reply[offset++] << 24) | (reply[offset++] << 16) | (reply[offset++] << 8) | reply[offset++];
		long expire = (reply[offset++] << 24) | (reply[offset++] << 16) | (reply[offset++] << 8) | reply[offset++];
		long minimum = (reply[offset++] << 24) | (reply[offset++] << 16) | (reply[offset++] << 8) | reply[offset++];
		return new DNS_rr_SOA(name, name2, name3, serial, refresh, retry, expire, minimum, ttl);
	}
}
