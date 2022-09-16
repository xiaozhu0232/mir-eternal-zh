using System;
using System.Collections.Generic;

namespace LumiSoft.Net;

public class CircleCollection<T>
{
	private List<T> m_pItems;

	private int m_Index;

	public int Count => m_pItems.Count;

	public T this[int index] => m_pItems[index];

	public CircleCollection()
	{
		m_pItems = new List<T>();
	}

	public void Add(T[] items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		foreach (T item in items)
		{
			Add(item);
		}
	}

	public void Add(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		m_pItems.Add(item);
		m_Index = 0;
	}

	public void Remove(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		m_pItems.Remove(item);
		m_Index = 0;
	}

	public void Clear()
	{
		m_pItems.Clear();
		m_Index = 0;
	}

	public bool Contains(T item)
	{
		return m_pItems.Contains(item);
	}

	public T Next()
	{
		if (m_pItems.Count == 0)
		{
			throw new InvalidOperationException("There is no items in the collection.");
		}
		lock (m_pItems)
		{
			T result = m_pItems[m_Index];
			m_Index++;
			if (m_Index >= m_pItems.Count)
			{
				m_Index = 0;
			}
			return result;
		}
	}

	public T[] ToArray()
	{
		lock (m_pItems)
		{
			return m_pItems.ToArray();
		}
	}

	public T[] ToCurrentOrderArray()
	{
		lock (m_pItems)
		{
			int num = m_Index;
			T[] array = new T[m_pItems.Count];
			for (int i = 0; i < m_pItems.Count; i++)
			{
				array[i] = m_pItems[num];
				num++;
				if (num >= m_pItems.Count)
				{
					num = 0;
				}
			}
			return array;
		}
	}
}
