using System;
using System.Collections.Generic;
using System.Timers;

namespace LumiSoft.Net.DNS.Client;

public class DNS_ClientCache
{
	private class CacheEntry
	{
		private DnsServerResponse m_pResponse;

		private DateTime m_Expires;

		public DnsServerResponse Response => m_pResponse;

		public DateTime Expires => m_Expires;

		public CacheEntry(DnsServerResponse response, DateTime expires)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			m_pResponse = response;
			m_Expires = expires;
		}
	}

	private Dictionary<string, CacheEntry> m_pCache;

	private int m_MaxCacheTtl = 86400;

	private int m_MaxNegativeCacheTtl = 900;

	private TimerEx m_pTimerTimeout;

	public int MaxCacheTtl
	{
		get
		{
			return m_MaxCacheTtl;
		}
		set
		{
			m_MaxCacheTtl = value;
		}
	}

	public int MaxNegativeCacheTtl
	{
		get
		{
			return m_MaxNegativeCacheTtl;
		}
		set
		{
			m_MaxNegativeCacheTtl = value;
		}
	}

	public int Count => m_pCache.Count;

	internal DNS_ClientCache()
	{
		m_pCache = new Dictionary<string, CacheEntry>();
		m_pTimerTimeout = new TimerEx(60000.0);
		m_pTimerTimeout.Elapsed += m_pTimerTimeout_Elapsed;
		m_pTimerTimeout.Start();
	}

	internal void Dispose()
	{
		m_pCache = null;
		m_pTimerTimeout.Dispose();
		m_pTimerTimeout = null;
	}

	private void m_pTimerTimeout_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (m_pCache)
		{
			List<KeyValuePair<string, CacheEntry>> list = new List<KeyValuePair<string, CacheEntry>>();
			foreach (KeyValuePair<string, CacheEntry> item in m_pCache)
			{
				list.Add(item);
			}
			foreach (KeyValuePair<string, CacheEntry> item2 in list)
			{
				if (DateTime.Now > item2.Value.Expires)
				{
					m_pCache.Remove(item2.Key);
				}
			}
		}
	}

	public DnsServerResponse GetFromCache(string qname, int qtype)
	{
		if (qname == null)
		{
			throw new ArgumentNullException("qname");
		}
		if (qname == string.Empty)
		{
			throw new ArgumentException("Argument 'qname' value must be specified.", "qname");
		}
		CacheEntry value = null;
		if (m_pCache.TryGetValue(qname + qtype, out value))
		{
			if (DateTime.Now > value.Expires)
			{
				return null;
			}
			return value.Response;
		}
		return null;
	}

	public void AddToCache(string qname, int qtype, DnsServerResponse response)
	{
		if (qname == null)
		{
			throw new ArgumentNullException("qname");
		}
		if (qname == string.Empty)
		{
			throw new ArgumentException("Argument 'qname' value must be specified.", "qname");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		lock (m_pCache)
		{
			if (m_pCache.ContainsKey(qname + qtype))
			{
				m_pCache.Remove(qname + qtype);
			}
			if (response.ResponseCode == DNS_RCode.NO_ERROR)
			{
				int num = m_MaxCacheTtl;
				DNS_rr[] allAnswers = response.AllAnswers;
				foreach (DNS_rr dNS_rr in allAnswers)
				{
					if (dNS_rr.TTL < num)
					{
						num = dNS_rr.TTL;
					}
				}
				m_pCache.Add(qname + qtype, new CacheEntry(response, DateTime.Now.AddSeconds(num)));
			}
			else
			{
				m_pCache.Add(qname + qtype, new CacheEntry(response, DateTime.Now.AddSeconds(m_MaxNegativeCacheTtl)));
			}
		}
	}

	public void ClearCache()
	{
		lock (m_pCache)
		{
			m_pCache.Clear();
		}
	}
}
