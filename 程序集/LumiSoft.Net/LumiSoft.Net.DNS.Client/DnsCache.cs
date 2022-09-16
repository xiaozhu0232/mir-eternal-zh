using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LumiSoft.Net.DNS.Client;

[Obsolete("Use DNS_Client.Cache instead.")]
public class DnsCache
{
	private static Hashtable m_pCache;

	private static long m_CacheTime;

	public static long CacheTime
	{
		get
		{
			return m_CacheTime;
		}
		set
		{
			m_CacheTime = value;
		}
	}

	static DnsCache()
	{
		m_pCache = null;
		m_CacheTime = 10000L;
		m_pCache = new Hashtable();
	}

	public static DnsServerResponse GetFromCache(string qname, int qtype)
	{
		try
		{
			if (m_pCache.Contains(qname + qtype))
			{
				DnsCacheEntry dnsCacheEntry = (DnsCacheEntry)m_pCache[qname + qtype];
				if (dnsCacheEntry.Time.AddSeconds(m_CacheTime) > DateTime.Now)
				{
					return dnsCacheEntry.Answers;
				}
			}
		}
		catch
		{
		}
		return null;
	}

	public static void AddToCache(string qname, int qtype, DnsServerResponse answers)
	{
		if (answers == null)
		{
			return;
		}
		try
		{
			lock (m_pCache)
			{
				if (m_pCache.Contains(qname + qtype))
				{
					m_pCache.Remove(qname + qtype);
				}
				m_pCache.Add(qname + qtype, new DnsCacheEntry(answers, DateTime.Now));
			}
		}
		catch
		{
		}
	}

	public static void ClearCache()
	{
		lock (m_pCache)
		{
			m_pCache.Clear();
		}
	}

	public static byte[] SerializeCache()
	{
		lock (m_pCache)
		{
			MemoryStream memoryStream = new MemoryStream();
			new BinaryFormatter().Serialize(memoryStream, m_pCache);
			return memoryStream.ToArray();
		}
	}

	public static void DeSerializeCache(byte[] cacheData)
	{
		lock (m_pCache)
		{
			MemoryStream serializationStream = new MemoryStream(cacheData);
			m_pCache = (Hashtable)new BinaryFormatter().Deserialize(serializationStream);
		}
	}
}
