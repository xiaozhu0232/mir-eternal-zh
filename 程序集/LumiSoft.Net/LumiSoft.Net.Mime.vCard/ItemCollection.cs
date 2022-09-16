using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.Mime.vCard;

public class ItemCollection : IEnumerable
{
	private vCard m_pCard;

	private List<Item> m_pItems;

	public int Count => m_pItems.Count;

	internal ItemCollection(vCard card)
	{
		m_pCard = card;
		m_pItems = new List<Item>();
	}

	public Item Add(string name, string parametes, string value)
	{
		Item item = new Item(m_pCard, name, parametes, value);
		m_pItems.Add(item);
		return item;
	}

	public void Remove(string name)
	{
		for (int i = 0; i < m_pItems.Count; i++)
		{
			if (m_pItems[i].Name.ToLower() == name.ToLower())
			{
				m_pItems.RemoveAt(i);
				i--;
			}
		}
	}

	public void Remove(Item item)
	{
		m_pItems.Remove(item);
	}

	public void Clear()
	{
		m_pItems.Clear();
	}

	public Item GetFirst(string name)
	{
		foreach (Item pItem in m_pItems)
		{
			if (pItem.Name.ToLower() == name.ToLower())
			{
				return pItem;
			}
		}
		return null;
	}

	public Item[] Get(string name)
	{
		List<Item> list = new List<Item>();
		foreach (Item pItem in m_pItems)
		{
			if (pItem.Name.ToLower() == name.ToLower())
			{
				list.Add(pItem);
			}
		}
		return list.ToArray();
	}

	public void SetDecodedValue(string name, string value)
	{
		if (value == null)
		{
			Remove(name);
			return;
		}
		Item first = GetFirst(name);
		if (first != null)
		{
			first.SetDecodedValue(value);
			return;
		}
		first = new Item(m_pCard, name, "", "");
		m_pItems.Add(first);
		first.SetDecodedValue(value);
	}

	public void SetValue(string name, string value)
	{
		SetValue(name, "", value);
	}

	public void SetValue(string name, string parametes, string value)
	{
		if (value == null)
		{
			Remove(name);
			return;
		}
		Item first = GetFirst(name);
		if (first != null)
		{
			first.Value = value;
		}
		else
		{
			m_pItems.Add(new Item(m_pCard, name, parametes, value));
		}
	}

	public IEnumerator GetEnumerator()
	{
		return m_pItems.GetEnumerator();
	}
}
