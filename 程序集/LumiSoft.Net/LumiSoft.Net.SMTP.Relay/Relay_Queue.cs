using System;
using System.Collections.Generic;
using System.IO;

namespace LumiSoft.Net.SMTP.Relay;

public class Relay_Queue : IDisposable
{
	private string m_Name = "";

	private Queue<Relay_QueueItem> m_pQueue;

	public string Name => m_Name;

	public int Count => m_pQueue.Count;

	public Relay_Queue(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == "")
		{
			throw new ArgumentException("Argument 'name' value may not be empty.");
		}
		m_Name = name;
		m_pQueue = new Queue<Relay_QueueItem>();
	}

	public void Dispose()
	{
	}

	public void QueueMessage(string from, string to, string messageID, Stream message, object tag)
	{
		QueueMessage(null, from, null, SMTP_DSN_Ret.NotSpecified, to, null, SMTP_DSN_Notify.NotSpecified, messageID, message, tag);
	}

	public void QueueMessage(string from, string envelopeID, SMTP_DSN_Ret ret, string to, string originalRecipient, SMTP_DSN_Notify notify, string messageID, Stream message, object tag)
	{
		QueueMessage(null, from, envelopeID, ret, to, originalRecipient, notify, messageID, message, tag);
	}

	public void QueueMessage(Relay_SmartHost targetServer, string from, string envelopeID, SMTP_DSN_Ret ret, string to, string originalRecipient, SMTP_DSN_Notify notify, string messageID, Stream message, object tag)
	{
		if (messageID == null)
		{
			throw new ArgumentNullException("messageID");
		}
		if (messageID == "")
		{
			throw new ArgumentException("Argument 'messageID' value must be specified.");
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		lock (m_pQueue)
		{
			m_pQueue.Enqueue(new Relay_QueueItem(this, targetServer, from, envelopeID, ret, to, originalRecipient, notify, messageID, message, tag));
		}
	}

	public Relay_QueueItem DequeueMessage()
	{
		lock (m_pQueue)
		{
			if (m_pQueue.Count > 0)
			{
				return m_pQueue.Dequeue();
			}
			return null;
		}
	}
}
