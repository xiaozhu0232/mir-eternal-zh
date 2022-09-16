using System;
using System.IO;
using System.Text;
using LumiSoft.Net.IMAP.Server;

namespace LumiSoft.Net.IMAP;

public abstract class IMAP_r
{
	public virtual string ToString(IMAP_Mailbox_Encoding encoding)
	{
		return ToString();
	}

	public bool ToStreamAsync(Stream stream, IMAP_Mailbox_Encoding mailboxEncoding, EventHandler<EventArgs<Exception>> completedAsyncCallback)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return ToStreamAsync(null, stream, mailboxEncoding, completedAsyncCallback);
	}

	internal bool SendAsync(IMAP_Session session, EventHandler<EventArgs<Exception>> completedAsyncCallback)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		return ToStreamAsync(session, session.TcpStream, session.MailboxEncoding, completedAsyncCallback);
	}

	protected virtual bool ToStreamAsync(IMAP_Session session, Stream stream, IMAP_Mailbox_Encoding mailboxEncoding, EventHandler<EventArgs<Exception>> completedAsyncCallback)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		string text = ToString(mailboxEncoding);
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		session?.LogAddWrite(bytes.Length, text.TrimEnd());
		IAsyncResult asyncResult = stream.BeginWrite(bytes, 0, bytes.Length, delegate(IAsyncResult r)
		{
			if (r.CompletedSynchronously)
			{
				return;
			}
			try
			{
				stream.EndWrite(r);
				if (completedAsyncCallback != null)
				{
					completedAsyncCallback(this, new EventArgs<Exception>(null));
				}
			}
			catch (Exception value)
			{
				if (completedAsyncCallback != null)
				{
					completedAsyncCallback(this, new EventArgs<Exception>(value));
				}
			}
		}, null);
		if (asyncResult.CompletedSynchronously)
		{
			stream.EndWrite(asyncResult);
			return false;
		}
		return true;
	}
}
