using System;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_MX : DNS_rr, IComparable
{
	private int m_Preference;

	private string m_Host = "";

	public int Preference => m_Preference;

	public string Host => m_Host;

	public DNS_rr_MX(string name, int preference, string host, int ttl)
		: base(name, DNS_QType.MX, ttl)
	{
		m_Preference = preference;
		m_Host = host;
	}

	public static DNS_rr_MX Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		int preference = (reply[offset++] << 8) | reply[offset++];
		string name2 = "";
		if (Dns_Client.GetQName(reply, ref offset, ref name2))
		{
			return new DNS_rr_MX(name, preference, name2, ttl);
		}
		throw new ArgumentException("Invalid MX resource record data !");
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (!(obj is DNS_rr_MX))
		{
			throw new ArgumentException("Argument obj is not MX_Record !");
		}
		DNS_rr_MX dNS_rr_MX = (DNS_rr_MX)obj;
		if (Preference > dNS_rr_MX.Preference)
		{
			return 1;
		}
		if (Preference < dNS_rr_MX.Preference)
		{
			return -1;
		}
		return 0;
	}
}
