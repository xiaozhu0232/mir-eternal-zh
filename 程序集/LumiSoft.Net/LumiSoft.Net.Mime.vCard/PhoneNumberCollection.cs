using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.Mime.vCard;

public class PhoneNumberCollection : IEnumerable
{
	private vCard m_pOwner;

	private List<PhoneNumber> m_pCollection;

	public int Count => m_pCollection.Count;

	internal PhoneNumberCollection(vCard owner)
	{
		m_pOwner = owner;
		m_pCollection = new List<PhoneNumber>();
		Item[] array = owner.Items.Get("TEL");
		foreach (Item item in array)
		{
			m_pCollection.Add(PhoneNumber.Parse(item));
		}
	}

	public void Add(PhoneNumberType_enum type, string number)
	{
		Item item = m_pOwner.Items.Add("TEL", PhoneNumber.PhoneTypeToString(type), number);
		m_pCollection.Add(new PhoneNumber(item, type, number));
	}

	public void Remove(PhoneNumber item)
	{
		m_pOwner.Items.Remove(item.Item);
		m_pCollection.Remove(item);
	}

	public void Clear()
	{
		foreach (PhoneNumber item in m_pCollection)
		{
			m_pOwner.Items.Remove(item.Item);
		}
		m_pCollection.Clear();
	}

	public IEnumerator GetEnumerator()
	{
		return m_pCollection.GetEnumerator();
	}
}
