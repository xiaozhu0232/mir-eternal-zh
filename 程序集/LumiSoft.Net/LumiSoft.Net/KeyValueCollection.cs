using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net;

public class KeyValueCollection<K, V> : IEnumerable
{
	private Dictionary<K, V> m_pDictionary;

	private List<V> m_pList;

	public int Count => m_pList.Count;

	public V this[K key] => m_pDictionary[key];

	public KeyValueCollection()
	{
		m_pDictionary = new Dictionary<K, V>();
		m_pList = new List<V>();
	}

	public void Add(K key, V value)
	{
		m_pDictionary.Add(key, value);
		m_pList.Add(value);
	}

	public bool Remove(K key)
	{
		V value = default(V);
		if (m_pDictionary.TryGetValue(key, out value))
		{
			m_pDictionary.Remove(key);
			m_pList.Remove(value);
			return true;
		}
		return false;
	}

	public void Clear()
	{
		m_pDictionary.Clear();
		m_pList.Clear();
	}

	public bool ContainsKey(K key)
	{
		return m_pDictionary.ContainsKey(key);
	}

	public bool TryGetValue(K key, out V value)
	{
		return m_pDictionary.TryGetValue(key, out value);
	}

	public bool TryGetValueAt(int index, out V value)
	{
		value = default(V);
		if (m_pList.Count > 0 && index >= 0 && index < m_pList.Count)
		{
			value = m_pList[index];
			return true;
		}
		return false;
	}

	public V[] ToArray()
	{
		lock (m_pList)
		{
			return m_pList.ToArray();
		}
	}

	public IEnumerator GetEnumerator()
	{
		return m_pList.GetEnumerator();
	}
}
