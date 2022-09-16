using System;

namespace LumiSoft.Net.IMAP.Client;

[Obsolete("Use Fetch(bool uid,IMAP_t_SeqSet seqSet,IMAP_t_Fetch_i[] items,EventHandler<EventArgs<IMAP_r_u>> callback) intead.")]
public class IMAP_Client_FetchHandler
{
	private int m_CurrentSeqNo = -1;

	public int CurrentSeqNo => m_CurrentSeqNo;

	public event EventHandler NextMessage;

	public event EventHandler<IMAP_Client_Fetch_Body_EArgs> Body;

	public event EventHandler<EventArgs<IMAP_Envelope>> Envelope;

	public event EventHandler<EventArgs<string[]>> Flags;

	public event EventHandler<EventArgs<DateTime>> InternalDate;

	public event EventHandler<IMAP_Client_Fetch_Rfc822_EArgs> Rfc822;

	public event EventHandler<EventArgs<string>> Rfc822Header;

	public event EventHandler<EventArgs<int>> Rfc822Size;

	public event EventHandler<EventArgs<string>> Rfc822Text;

	public event EventHandler<EventArgs<long>> UID;

	public event EventHandler<EventArgs<ulong>> X_GM_MSGID;

	public event EventHandler<EventArgs<ulong>> X_GM_THRID;

	internal void SetCurrentSeqNo(int seqNo)
	{
		m_CurrentSeqNo = seqNo;
	}

	internal void OnNextMessage()
	{
		if (this.NextMessage != null)
		{
			this.NextMessage(this, new EventArgs());
		}
	}

	internal void OnBody(IMAP_Client_Fetch_Body_EArgs eArgs)
	{
		if (this.Body != null)
		{
			this.Body(this, eArgs);
		}
	}

	internal void OnEnvelope(IMAP_Envelope envelope)
	{
		if (this.Envelope != null)
		{
			this.Envelope(this, new EventArgs<IMAP_Envelope>(envelope));
		}
	}

	internal void OnFlags(string[] flags)
	{
		if (this.Flags != null)
		{
			this.Flags(this, new EventArgs<string[]>(flags));
		}
	}

	internal void OnInternalDate(DateTime date)
	{
		if (this.InternalDate != null)
		{
			this.InternalDate(this, new EventArgs<DateTime>(date));
		}
	}

	internal void OnRfc822(IMAP_Client_Fetch_Rfc822_EArgs eArgs)
	{
		if (this.Rfc822 != null)
		{
			this.Rfc822(this, eArgs);
		}
	}

	internal void OnRfc822Header(string header)
	{
		if (this.Rfc822Header != null)
		{
			this.Rfc822Header(this, new EventArgs<string>(header));
		}
	}

	internal void OnSize(int size)
	{
		if (this.Rfc822Size != null)
		{
			this.Rfc822Size(this, new EventArgs<int>(size));
		}
	}

	internal void OnRfc822Text(string text)
	{
		if (this.Rfc822Text != null)
		{
			this.Rfc822Text(this, new EventArgs<string>(text));
		}
	}

	internal void OnUID(long uid)
	{
		if (this.UID != null)
		{
			this.UID(this, new EventArgs<long>(uid));
		}
	}

	internal void OnX_GM_MSGID(ulong msgID)
	{
		if (this.X_GM_MSGID != null)
		{
			this.X_GM_MSGID(this, new EventArgs<ulong>(msgID));
		}
	}

	internal void OnX_GM_THRID(ulong threadID)
	{
		if (this.X_GM_THRID != null)
		{
			this.X_GM_THRID(this, new EventArgs<ulong>(threadID));
		}
	}
}
