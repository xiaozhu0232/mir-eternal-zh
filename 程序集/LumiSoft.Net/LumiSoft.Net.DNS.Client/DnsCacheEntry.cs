using System;

namespace LumiSoft.Net.DNS.Client;

[Serializable]
internal struct DnsCacheEntry
{
	private DnsServerResponse m_pResponse;

	private DateTime m_Time;

	public DnsServerResponse Answers => m_pResponse;

	public DateTime Time => m_Time;

	public DnsCacheEntry(DnsServerResponse answers, DateTime addTime)
	{
		m_pResponse = answers;
		m_Time = addTime;
	}
}
