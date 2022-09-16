using System;
using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.SIP.Message;

public class SIP_ParameterCollection : IEnumerable
{
	private List<SIP_Parameter> m_pCollection;

	public int Count => m_pCollection.Count;

	public SIP_Parameter this[string name]
	{
		get
		{
			foreach (SIP_Parameter item in m_pCollection)
			{
				if (item.Name.ToLower() == name.ToLower())
				{
					return item;
				}
			}
			return null;
		}
	}

	public SIP_ParameterCollection()
	{
		m_pCollection = new List<SIP_Parameter>();
	}

	public void Add(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (Contains(name))
		{
			throw new ArgumentException("Prameter '' with specified name already exists in the collection !");
		}
		m_pCollection.Add(new SIP_Parameter(name, value));
	}

	public void Set(string name, string value)
	{
		if (Contains(name))
		{
			this[name].Value = value;
		}
		else
		{
			Add(name, value);
		}
	}

	public void Clear()
	{
		m_pCollection.Clear();
	}

	public void Remove(string name)
	{
		SIP_Parameter sIP_Parameter = this[name];
		if (sIP_Parameter != null)
		{
			m_pCollection.Remove(sIP_Parameter);
		}
	}

	public bool Contains(string name)
	{
		if (this[name] != null)
		{
			return true;
		}
		return false;
	}

	public IEnumerator GetEnumerator()
	{
		return m_pCollection.GetEnumerator();
	}
}
