using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_t_MailboxList : IEnumerable
{
	private bool m_IsModified;

	private List<Mail_t_Mailbox> m_pList;

	public bool IsModified => m_IsModified;

	public int Count => m_pList.Count;

	public Mail_t_Mailbox this[int index]
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

	public Mail_t_MailboxList()
	{
		m_pList = new List<Mail_t_Mailbox>();
	}

	public static Mail_t_MailboxList Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		MIME_Reader mIME_Reader = new MIME_Reader(value);
		Mail_t_MailboxList mail_t_MailboxList = new Mail_t_MailboxList();
		while (true)
		{
			string text = mIME_Reader.QuotedReadToDelimiter(new char[2] { ',', '<' });
			if (string.IsNullOrEmpty(text) && mIME_Reader.Available == 0)
			{
				break;
			}
			if (mIME_Reader.Peek(readToFirstChar: true) == 60)
			{
				mail_t_MailboxList.Add(new Mail_t_Mailbox((text != null) ? MIME_Encoding_EncodedWord.DecodeS(TextUtils.UnQuoteString(text.Trim())) : null, mIME_Reader.ReadParenthesized()));
			}
			else
			{
				mail_t_MailboxList.Add(new Mail_t_Mailbox(null, text));
			}
			if (mIME_Reader.Peek(readToFirstChar: true) == 44)
			{
				mIME_Reader.Char(readToFirstChar: false);
			}
		}
		return mail_t_MailboxList;
	}

	public void Insert(int index, Mail_t_Mailbox value)
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

	public void Add(Mail_t_Mailbox value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_pList.Add(value);
		m_IsModified = true;
	}

	public void Remove(Mail_t_Mailbox value)
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

	public Mail_t_Mailbox[] ToArray()
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
