using System;
using System.Net;

namespace LumiSoft.Net.DNS;

[Serializable]
public class DNS_rr_A : DNS_rr
{
	private IPAddress m_IP;

	public IPAddress IP => m_IP;

	public DNS_rr_A(string name, IPAddress ip, int ttl)
		: base(name, DNS_QType.A, ttl)
	{
		m_IP = ip;
	}

	public static DNS_rr_A Parse(string name, byte[] reply, ref int offset, int rdLength, int ttl)
	{
		byte[] array = new byte[rdLength];
		Array.Copy(reply, offset, array, 0, rdLength);
		offset += rdLength;
		return new DNS_rr_A(name, new IPAddress(array), ttl);
	}
}
