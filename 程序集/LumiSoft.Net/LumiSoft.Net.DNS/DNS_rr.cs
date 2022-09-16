namespace LumiSoft.Net.DNS;

public abstract class DNS_rr
{
	private string m_Name = "";

	private DNS_QType m_Type = DNS_QType.A;

	private int m_TTL = -1;

	public string Name => m_Name;

	public DNS_QType RecordType => m_Type;

	public int TTL => m_TTL;

	public DNS_rr(string name, DNS_QType recordType, int ttl)
	{
		m_Name = name;
		m_Type = recordType;
		m_TTL = ttl;
	}
}
