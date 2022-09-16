using System;
using System.Collections.Generic;

namespace LumiSoft.Net.DNS.Client;

[Serializable]
public class DnsServerResponse
{
	private bool m_Success = true;

	private int m_ID;

	private DNS_RCode m_RCODE;

	private List<DNS_rr> m_pAnswers;

	private List<DNS_rr> m_pAuthoritiveAnswers;

	private List<DNS_rr> m_pAdditionalAnswers;

	public bool ConnectionOk => m_Success;

	public int ID => m_ID;

	public DNS_RCode ResponseCode => m_RCODE;

	public DNS_rr[] AllAnswers
	{
		get
		{
			List<DNS_rr> list = new List<DNS_rr>();
			list.AddRange(m_pAnswers.ToArray());
			list.AddRange(m_pAuthoritiveAnswers.ToArray());
			list.AddRange(m_pAdditionalAnswers.ToArray());
			return list.ToArray();
		}
	}

	public DNS_rr[] Answers => m_pAnswers.ToArray();

	public DNS_rr[] AuthoritiveAnswers => m_pAuthoritiveAnswers.ToArray();

	public DNS_rr[] AdditionalAnswers => m_pAdditionalAnswers.ToArray();

	internal DnsServerResponse(bool connectionOk, int id, DNS_RCode rcode, List<DNS_rr> answers, List<DNS_rr> authoritiveAnswers, List<DNS_rr> additionalAnswers)
	{
		m_Success = connectionOk;
		m_ID = id;
		m_RCODE = rcode;
		m_pAnswers = answers;
		m_pAuthoritiveAnswers = authoritiveAnswers;
		m_pAdditionalAnswers = additionalAnswers;
	}

	public DNS_rr_A[] GetARecords()
	{
		List<DNS_rr_A> list = new List<DNS_rr_A>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.A)
			{
				list.Add((DNS_rr_A)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_NS[] GetNSRecords()
	{
		List<DNS_rr_NS> list = new List<DNS_rr_NS>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.NS)
			{
				list.Add((DNS_rr_NS)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_CNAME[] GetCNAMERecords()
	{
		List<DNS_rr_CNAME> list = new List<DNS_rr_CNAME>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.CNAME)
			{
				list.Add((DNS_rr_CNAME)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_SOA[] GetSOARecords()
	{
		List<DNS_rr_SOA> list = new List<DNS_rr_SOA>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.SOA)
			{
				list.Add((DNS_rr_SOA)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_PTR[] GetPTRRecords()
	{
		List<DNS_rr_PTR> list = new List<DNS_rr_PTR>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.PTR)
			{
				list.Add((DNS_rr_PTR)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_HINFO[] GetHINFORecords()
	{
		List<DNS_rr_HINFO> list = new List<DNS_rr_HINFO>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.HINFO)
			{
				list.Add((DNS_rr_HINFO)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_MX[] GetMXRecords()
	{
		List<DNS_rr_MX> list = new List<DNS_rr_MX>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.MX)
			{
				list.Add((DNS_rr_MX)pAnswer);
			}
		}
		DNS_rr_MX[] array = list.ToArray();
		Array.Sort(array);
		return array;
	}

	public DNS_rr_TXT[] GetTXTRecords()
	{
		List<DNS_rr_TXT> list = new List<DNS_rr_TXT>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.TXT)
			{
				list.Add((DNS_rr_TXT)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_AAAA[] GetAAAARecords()
	{
		List<DNS_rr_AAAA> list = new List<DNS_rr_AAAA>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.AAAA)
			{
				list.Add((DNS_rr_AAAA)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_SRV[] GetSRVRecords()
	{
		List<DNS_rr_SRV> list = new List<DNS_rr_SRV>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.SRV)
			{
				list.Add((DNS_rr_SRV)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_NAPTR[] GetNAPTRRecords()
	{
		List<DNS_rr_NAPTR> list = new List<DNS_rr_NAPTR>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.NAPTR)
			{
				list.Add((DNS_rr_NAPTR)pAnswer);
			}
		}
		return list.ToArray();
	}

	public DNS_rr_SPF[] GetSPFRecords()
	{
		List<DNS_rr_SPF> list = new List<DNS_rr_SPF>();
		foreach (DNS_rr pAnswer in m_pAnswers)
		{
			if (pAnswer.RecordType == DNS_QType.SPF)
			{
				list.Add((DNS_rr_SPF)pAnswer);
			}
		}
		return list.ToArray();
	}

	private List<DNS_rr> FilterRecordsX(List<DNS_rr> answers, DNS_QType type)
	{
		List<DNS_rr> list = new List<DNS_rr>();
		foreach (DNS_rr answer in answers)
		{
			if (answer.RecordType == type)
			{
				list.Add(answer);
			}
		}
		return list;
	}
}
