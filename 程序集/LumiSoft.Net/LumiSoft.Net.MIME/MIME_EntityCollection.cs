using System;
using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.MIME;

public class MIME_EntityCollection : IEnumerable
{
	private bool m_IsModified;

	private List<MIME_Entity> m_pCollection;

	public bool IsModified
	{
		get
		{
			if (m_IsModified)
			{
				return true;
			}
			foreach (MIME_Entity item in m_pCollection)
			{
				if (item.IsModified)
				{
					return true;
				}
			}
			return false;
		}
	}

	public int Count => m_pCollection.Count;

	public MIME_Entity this[int index] => m_pCollection[index];

	internal MIME_EntityCollection()
	{
		m_pCollection = new List<MIME_Entity>();
	}

	public void Add(MIME_Entity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		m_pCollection.Add(entity);
		m_IsModified = true;
	}

	public void Insert(int index, MIME_Entity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		m_pCollection.Insert(index, entity);
		m_IsModified = true;
	}

	public void Remove(MIME_Entity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("field");
		}
		m_pCollection.Remove(entity);
		m_IsModified = true;
	}

	public void Remove(int index)
	{
		m_pCollection.RemoveAt(index);
		m_IsModified = true;
	}

	public void Clear()
	{
		m_pCollection.Clear();
		m_IsModified = true;
	}

	public bool Contains(MIME_Entity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		return m_pCollection.Contains(entity);
	}

	internal void SetModified(bool isModified)
	{
		m_IsModified = isModified;
	}

	public IEnumerator GetEnumerator()
	{
		return m_pCollection.GetEnumerator();
	}
}
