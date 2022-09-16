using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class AddressList : IEnumerable
{
	private HeaderField m_HeaderField;

	private List<Address> m_pAddresses;

	public MailboxAddress[] Mailboxes
	{
		get
		{
			ArrayList arrayList = new ArrayList();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Address address = (Address)enumerator.Current;
					if (!address.IsGroupAddress)
					{
						arrayList.Add((MailboxAddress)address);
						continue;
					}
					foreach (MailboxAddress groupMember in ((GroupAddress)address).GroupMembers)
					{
						arrayList.Add(groupMember);
					}
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			MailboxAddress[] array = new MailboxAddress[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}
	}

	public Address this[int index] => m_pAddresses[index];

	public int Count => m_pAddresses.Count;

	internal HeaderField BoundedHeaderField
	{
		get
		{
			return m_HeaderField;
		}
		set
		{
			m_HeaderField = value;
		}
	}

	public AddressList()
	{
		m_pAddresses = new List<Address>();
	}

	public void Add(Address address)
	{
		address.Owner = this;
		m_pAddresses.Add(address);
		OnCollectionChanged();
	}

	public void Insert(int index, Address address)
	{
		address.Owner = this;
		m_pAddresses.Insert(index, address);
		OnCollectionChanged();
	}

	public void Remove(int index)
	{
		Remove(m_pAddresses[index]);
	}

	public void Remove(Address address)
	{
		address.Owner = null;
		m_pAddresses.Remove(address);
		OnCollectionChanged();
	}

	public void Clear()
	{
		foreach (Address pAddress in m_pAddresses)
		{
			pAddress.Owner = null;
		}
		m_pAddresses.Clear();
		OnCollectionChanged();
	}

	public void Parse(string addressList)
	{
		addressList = addressList.Trim();
		StringReader stringReader = new StringReader(addressList);
		while (stringReader.SourceString.Length > 0)
		{
			int num = TextUtils.QuotedIndexOf(stringReader.SourceString, ',');
			int num2 = TextUtils.QuotedIndexOf(stringReader.SourceString, ':');
			if (num2 == -1 || (num < num2 && num != -1))
			{
				MailboxAddress mailboxAddress = MailboxAddress.Parse(stringReader.QuotedReadToDelimiter(','));
				m_pAddresses.Add(mailboxAddress);
				mailboxAddress.Owner = this;
				continue;
			}
			GroupAddress groupAddress = GroupAddress.Parse(stringReader.QuotedReadToDelimiter(';'));
			m_pAddresses.Add(groupAddress);
			groupAddress.Owner = this;
			if (stringReader.SourceString.Length > 0)
			{
				stringReader.QuotedReadToDelimiter(',');
			}
		}
		OnCollectionChanged();
	}

	public string ToAddressListString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < m_pAddresses.Count; i++)
		{
			if (m_pAddresses[i] is MailboxAddress)
			{
				if (i == m_pAddresses.Count - 1)
				{
					stringBuilder.Append(((MailboxAddress)m_pAddresses[i]).ToMailboxAddressString());
				}
				else
				{
					stringBuilder.Append(((MailboxAddress)m_pAddresses[i]).ToMailboxAddressString() + ",\t");
				}
			}
			else if (m_pAddresses[i] is GroupAddress)
			{
				if (i == m_pAddresses.Count - 1)
				{
					stringBuilder.Append(((GroupAddress)m_pAddresses[i]).GroupString);
				}
				else
				{
					stringBuilder.Append(((GroupAddress)m_pAddresses[i]).GroupString + ",\t");
				}
			}
		}
		return stringBuilder.ToString();
	}

	internal void OnCollectionChanged()
	{
		if (m_HeaderField != null)
		{
			m_HeaderField.Value = ToAddressListString();
		}
	}

	public IEnumerator GetEnumerator()
	{
		return m_pAddresses.GetEnumerator();
	}
}
