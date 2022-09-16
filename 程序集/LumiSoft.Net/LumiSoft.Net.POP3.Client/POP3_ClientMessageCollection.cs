using System;
using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.POP3.Client;

public class POP3_ClientMessageCollection : IEnumerable, IDisposable
{
	private POP3_Client m_pPop3Client;

	private List<POP3_ClientMessage> m_pMessages;

	private bool m_IsDisposed;

	public long TotalSize
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			long num = 0L;
			foreach (POP3_ClientMessage pMessage in m_pMessages)
			{
				num += pMessage.Size;
			}
			return num;
		}
	}

	public int Count
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pMessages.Count;
		}
	}

	public POP3_ClientMessage this[int index]
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (index < 0 || index > m_pMessages.Count)
			{
				throw new ArgumentOutOfRangeException();
			}
			return m_pMessages[index];
		}
	}

	public POP3_ClientMessage this[string uid]
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!m_pPop3Client.IsUidlSupported)
			{
				throw new NotSupportedException();
			}
			foreach (POP3_ClientMessage pMessage in m_pMessages)
			{
				if (pMessage.UID == uid)
				{
					return pMessage;
				}
			}
			return null;
		}
	}

	internal POP3_ClientMessageCollection(POP3_Client pop3)
	{
		m_pPop3Client = pop3;
		m_pMessages = new List<POP3_ClientMessage>();
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		m_IsDisposed = true;
		foreach (POP3_ClientMessage pMessage in m_pMessages)
		{
			pMessage.Dispose();
		}
		m_pMessages = null;
	}

	internal void Add(int size)
	{
		m_pMessages.Add(new POP3_ClientMessage(m_pPop3Client, m_pMessages.Count + 1, size));
	}

	public IEnumerator GetEnumerator()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		return m_pMessages.GetEnumerator();
	}
}
