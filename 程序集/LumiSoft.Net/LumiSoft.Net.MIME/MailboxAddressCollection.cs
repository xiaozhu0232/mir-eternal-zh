using System;
using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class MailboxAddressCollection : IEnumerable
{
	private Address m_pOwner;

	private List<MailboxAddress> m_pMailboxes;

	public MailboxAddress this[int index] => m_pMailboxes[index];

	public int Count => m_pMailboxes.Count;

	internal Address Owner
	{
		get
		{
			return m_pOwner;
		}
		set
		{
			m_pOwner = value;
		}
	}

	public MailboxAddressCollection()
	{
		m_pMailboxes = new List<MailboxAddress>();
	}

	public void Add(MailboxAddress mailbox)
	{
		m_pMailboxes.Add(mailbox);
		OnCollectionChanged();
	}

	public void Insert(int index, MailboxAddress mailbox)
	{
		m_pMailboxes.Insert(index, mailbox);
		OnCollectionChanged();
	}

	public void Remove(int index)
	{
		m_pMailboxes.RemoveAt(index);
		OnCollectionChanged();
	}

	public void Remove(MailboxAddress mailbox)
	{
		m_pMailboxes.Remove(mailbox);
		OnCollectionChanged();
	}

	public void Clear()
	{
		m_pMailboxes.Clear();
		OnCollectionChanged();
	}

	public void Parse(string mailboxList)
	{
		string[] array = TextUtils.SplitQuotedString(mailboxList, ',');
		foreach (string mailbox in array)
		{
			m_pMailboxes.Add(MailboxAddress.Parse(mailbox));
		}
	}

	public string ToMailboxListString()
	{
		string text = "";
		for (int i = 0; i < m_pMailboxes.Count; i++)
		{
			text = ((i != m_pMailboxes.Count - 1) ? (text + m_pMailboxes[i].ToMailboxAddressString() + ",\t") : (text + m_pMailboxes[i].ToMailboxAddressString()));
		}
		return text;
	}

	internal void OnCollectionChanged()
	{
		if (m_pOwner != null && m_pOwner is GroupAddress)
		{
			((GroupAddress)m_pOwner).OnChanged();
		}
	}

	public IEnumerator GetEnumerator()
	{
		return m_pMailboxes.GetEnumerator();
	}
}
