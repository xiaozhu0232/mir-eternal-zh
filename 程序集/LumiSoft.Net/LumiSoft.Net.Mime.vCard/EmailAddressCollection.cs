using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.Mime.vCard;

public class EmailAddressCollection : IEnumerable
{
	private vCard m_pOwner;

	private List<EmailAddress> m_pCollection;

	public int Count => m_pCollection.Count;

	public EmailAddress this[int index] => m_pCollection[index];

	internal EmailAddressCollection(vCard owner)
	{
		m_pOwner = owner;
		m_pCollection = new List<EmailAddress>();
		Item[] array = owner.Items.Get("EMAIL");
		foreach (Item item in array)
		{
			m_pCollection.Add(EmailAddress.Parse(item));
		}
	}

	public EmailAddress Add(EmailAddressType_enum type, string email)
	{
		Item item = m_pOwner.Items.Add("EMAIL", EmailAddress.EmailTypeToString(type), "");
		item.SetDecodedValue(email);
		EmailAddress emailAddress = new EmailAddress(item, type, email);
		m_pCollection.Add(emailAddress);
		return emailAddress;
	}

	public void Remove(EmailAddress item)
	{
		m_pOwner.Items.Remove(item.Item);
		m_pCollection.Remove(item);
	}

	public void Clear()
	{
		foreach (EmailAddress item in m_pCollection)
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
