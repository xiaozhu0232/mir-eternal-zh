namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_X_GM_THRID : IMAP_t_Fetch_r_i
{
	private ulong m_ThreadID;

	public ulong ThreadID => m_ThreadID;

	public IMAP_t_Fetch_r_i_X_GM_THRID(ulong threadID)
	{
		m_ThreadID = threadID;
	}
}
