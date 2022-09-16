using System;
using System.IO;

namespace LumiSoft.Net.IMAP.Client;

[Obsolete("Use Fetch(bool uid,IMAP_t_SeqSet seqSet,IMAP_t_Fetch_i[] items,EventHandler<EventArgs<IMAP_r_u>> callback) intead.")]
public class IMAP_Client_Fetch_Rfc822_EArgs : EventArgs
{
	private Stream m_pStream;

	public Stream Stream
	{
		get
		{
			return m_pStream;
		}
		set
		{
			m_pStream = value;
		}
	}

	public event EventHandler StoringCompleted;

	internal IMAP_Client_Fetch_Rfc822_EArgs()
	{
	}

	internal void OnStoringCompleted()
	{
		if (this.StoringCompleted != null)
		{
			this.StoringCompleted(this, new EventArgs());
		}
	}
}
