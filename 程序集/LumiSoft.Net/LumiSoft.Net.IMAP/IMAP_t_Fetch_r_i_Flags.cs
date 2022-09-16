using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_Flags : IMAP_t_Fetch_r_i
{
	private IMAP_t_MsgFlags m_pFlags;

	public IMAP_t_MsgFlags Flags => m_pFlags;

	public IMAP_t_Fetch_r_i_Flags(IMAP_t_MsgFlags flags)
	{
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		m_pFlags = flags;
	}
}
