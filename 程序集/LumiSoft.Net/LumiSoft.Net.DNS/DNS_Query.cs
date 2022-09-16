using System;

namespace LumiSoft.Net.DNS;

public class DNS_Query
{
	private DNS_QClass m_QClass = DNS_QClass.IN;

	private DNS_QType m_QType = DNS_QType.ANY;

	private string m_QName = "";

	public DNS_QClass QueryClass => m_QClass;

	public DNS_QType QueryType => m_QType;

	public string QueryName => m_QName;

	public DNS_Query(DNS_QType qtype, string qname)
		: this(DNS_QClass.IN, qtype, qname)
	{
	}

	public DNS_Query(DNS_QClass qclass, DNS_QType qtype, string qname)
	{
		if (qname == null)
		{
			throw new ArgumentNullException("qname");
		}
		m_QClass = qclass;
		m_QType = qtype;
		m_QName = qname;
	}
}
