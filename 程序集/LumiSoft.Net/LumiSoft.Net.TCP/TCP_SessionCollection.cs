using System;
using System.Collections.Generic;
using System.Net;

namespace LumiSoft.Net.TCP;

public class TCP_SessionCollection<T> where T : TCP_Session
{
	private Dictionary<string, T> m_pItems;

	private Dictionary<string, long> m_pConnectionsPerIP;

	public int Count => m_pItems.Count;

	public T this[string id] => m_pItems[id];

	internal TCP_SessionCollection()
	{
		m_pItems = new Dictionary<string, T>();
		m_pConnectionsPerIP = new Dictionary<string, long>();
	}

	internal void Add(T session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		lock (m_pItems)
		{
			m_pItems.Add(session.ID, session);
			if (session.IsConnected && session.RemoteEndPoint != null)
			{
				if (m_pConnectionsPerIP.ContainsKey(session.RemoteEndPoint.Address.ToString()))
				{
					m_pConnectionsPerIP[session.RemoteEndPoint.Address.ToString()]++;
				}
				else
				{
					m_pConnectionsPerIP.Add(session.RemoteEndPoint.Address.ToString(), 1L);
				}
			}
		}
	}

	internal void Remove(T session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		lock (m_pItems)
		{
			m_pItems.Remove(session.ID);
			if (session.IsConnected && m_pConnectionsPerIP.ContainsKey(session.RemoteEndPoint.Address.ToString()))
			{
				m_pConnectionsPerIP[session.RemoteEndPoint.Address.ToString()]--;
				if (m_pConnectionsPerIP[session.RemoteEndPoint.Address.ToString()] == 0L)
				{
					m_pConnectionsPerIP.Remove(session.RemoteEndPoint.Address.ToString());
				}
			}
		}
	}

	internal void Clear()
	{
		lock (m_pItems)
		{
			m_pItems.Clear();
			m_pConnectionsPerIP.Clear();
		}
	}

	public T[] ToArray()
	{
		lock (m_pItems)
		{
			T[] array = new T[m_pItems.Count];
			m_pItems.Values.CopyTo(array, 0);
			return array;
		}
	}

	public long GetConnectionsPerIP(IPAddress ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		long value = 0L;
		m_pConnectionsPerIP.TryGetValue(ip.ToString(), out value);
		return value;
	}
}
