using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Search : EventArgs
{
	private IMAP_r_ServerStatus m_pResponse;

	private IMAP_Search_Key m_pCriteria;

	public IMAP_r_ServerStatus Response
	{
		get
		{
			return m_pResponse;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_pResponse = value;
		}
	}

	public IMAP_Search_Key Criteria => m_pCriteria;

	internal event EventHandler<EventArgs<long>> Matched;

	internal IMAP_e_Search(IMAP_Search_Key criteria, IMAP_r_ServerStatus response)
	{
		if (criteria == null)
		{
			throw new ArgumentNullException("criteria");
		}
		m_pResponse = response;
		m_pCriteria = criteria;
	}

	public void AddMessage(long uid)
	{
		OnMatched(uid);
	}

	private void OnMatched(long uid)
	{
		if (this.Matched != null)
		{
			this.Matched(this, new EventArgs<long>(uid));
		}
	}
}
