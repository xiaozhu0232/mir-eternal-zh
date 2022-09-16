using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_InternalDate : IMAP_t_Fetch_r_i
{
	private DateTime m_Date;

	public DateTime Date => m_Date;

	public IMAP_t_Fetch_r_i_InternalDate(DateTime date)
	{
		m_Date = date;
	}
}
