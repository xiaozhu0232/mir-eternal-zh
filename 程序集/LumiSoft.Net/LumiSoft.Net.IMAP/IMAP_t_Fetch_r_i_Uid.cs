using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_Uid : IMAP_t_Fetch_r_i
{
	private long m_UID;

	public long UID => m_UID;

	public IMAP_t_Fetch_r_i_Uid(long uid)
	{
		if (uid < 0)
		{
			throw new ArgumentException("Argument 'uid' value must be >= 0.", "uid");
		}
		m_UID = uid;
	}
}
