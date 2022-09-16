namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_X_GM_MSGID : IMAP_t_Fetch_r_i
{
	private ulong m_MsgID;

	public ulong MsgID => m_MsgID;

	public IMAP_t_Fetch_r_i_X_GM_MSGID(ulong msgID)
	{
		m_MsgID = msgID;
	}
}
