using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_t_AddressList : IEnumerable
{
	private bool m_IsModified;

	private List<Mail_t_Address> m_pList;

	public bool IsModified => m_IsModified;

	public int Count => m_pList.Count;

	public Mail_t_Address this[int index]
	{
		get
		{
			if (index < 0 || index >= m_pList.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return m_pList[index];
		}
	}

	public Mail_t_Mailbox[] Mailboxes
	{
		get
		{
			List<Mail_t_Mailbox> list = new List<Mail_t_Mailbox>();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Mail_t_Address mail_t_Address = (Mail_t_Address)enumerator.Current;
					if (mail_t_Address is Mail_t_Mailbox)
					{
						list.Add((Mail_t_Mailbox)mail_t_Address);
					}
					else
					{
						list.AddRange(((Mail_t_Group)mail_t_Address).Members);
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
			return list.ToArray();
		}
	}

	public Mail_t_AddressList()
	{
		m_pList = new List<Mail_t_Address>();
	}

	public static Mail_t_AddressList Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		MIME_Reader mIME_Reader = new MIME_Reader(value);
		Mail_t_AddressList mail_t_AddressList = new Mail_t_AddressList();
		while (true)
		{
			string text = mIME_Reader.QuotedReadToDelimiter(new char[3] { ',', '<', ':' });
			if (text == null && mIME_Reader.Available == 0)
			{
				break;
			}
			if (mIME_Reader.Peek(readToFirstChar: true) == 58)
			{
				Mail_t_Group mail_t_Group = new Mail_t_Group((text != null) ? MIME_Encoding_EncodedWord.DecodeS(TextUtils.UnQuoteString(text)) : null);
				mIME_Reader.Char(readToFirstChar: true);
				while (true)
				{
					text = mIME_Reader.QuotedReadToDelimiter(new char[4] { ',', '<', ':', ';' });
					if ((text == null && mIME_Reader.Available == 0) || mIME_Reader.Peek(readToFirstChar: false) == 59)
					{
						break;
					}
					if (text == string.Empty)
					{
						throw new ParseException("Invalid address-list value '" + value + "'.");
					}
					if (mIME_Reader.Peek(readToFirstChar: true) == 60)
					{
						mail_t_Group.Members.Add(new Mail_t_Mailbox((text != null) ? MIME_Encoding_EncodedWord.DecodeS(TextUtils.UnQuoteString(text)) : null, mIME_Reader.ReadParenthesized()));
					}
					else
					{
						mail_t_Group.Members.Add(new Mail_t_Mailbox(null, text));
					}
					if (mIME_Reader.Peek(readToFirstChar: true) == 59)
					{
						mIME_Reader.Char(readToFirstChar: true);
						break;
					}
					if (mIME_Reader.Peek(readToFirstChar: true) == 44)
					{
						mIME_Reader.Char(readToFirstChar: false);
					}
				}
				mail_t_AddressList.Add(mail_t_Group);
			}
			else if (mIME_Reader.Peek(readToFirstChar: true) == 60)
			{
				mail_t_AddressList.Add(new Mail_t_Mailbox((text != null) ? MIME_Encoding_EncodedWord.DecodeS(TextUtils.UnQuoteString(text.Trim())) : null, mIME_Reader.ReadParenthesized()));
			}
			else
			{
				mail_t_AddressList.Add(new Mail_t_Mailbox(null, text));
			}
			if (mIME_Reader.Peek(readToFirstChar: true) == 44)
			{
				mIME_Reader.Char(readToFirstChar: false);
			}
		}
		return mail_t_AddressList;
	}

	public void Insert(int index, Mail_t_Address value)
	{
		if (index < 0 || index > m_pList.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_pList.Insert(index, value);
		m_IsModified = true;
	}

	public void Add(Mail_t_Address value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_pList.Add(value);
		m_IsModified = true;
	}

	public void Remove(Mail_t_Address value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_pList.Remove(value);
	}

	public void Clear()
	{
		m_pList.Clear();
		m_IsModified = true;
	}

	public Mail_t_Address[] ToArray()
	{
		return m_pList.ToArray();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < m_pList.Count; i++)
		{
			if (i == m_pList.Count - 1)
			{
				stringBuilder.Append(m_pList[i].ToString());
			}
			else
			{
				stringBuilder.Append(m_pList[i].ToString() + ",");
			}
		}
		return stringBuilder.ToString();
	}

	internal void AcceptChanges()
	{
		m_IsModified = false;
	}

	public IEnumerator GetEnumerator()
	{
		return m_pList.GetEnumerator();
	}
}
