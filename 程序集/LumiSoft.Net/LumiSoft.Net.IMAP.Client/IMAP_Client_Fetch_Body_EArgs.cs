using System;
using System.IO;

namespace LumiSoft.Net.IMAP.Client;

[Obsolete("Use Fetch(bool uid,IMAP_t_SeqSet seqSet,IMAP_t_Fetch_i[] items,EventHandler<EventArgs<IMAP_r_u>> callback) intead.")]
public class IMAP_Client_Fetch_Body_EArgs : EventArgs
{
	private string m_Section;

	private int m_Offset = -1;

	private Stream m_pStream;

	public string BodySection => m_Section;

	public int Offset => m_Offset;

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

	internal IMAP_Client_Fetch_Body_EArgs(string bodySection, int offset)
	{
		m_Section = bodySection;
		m_Offset = offset;
	}

	internal void OnStoringCompleted()
	{
		if (this.StoringCompleted != null)
		{
			this.StoringCompleted(this, new EventArgs());
		}
	}
}
