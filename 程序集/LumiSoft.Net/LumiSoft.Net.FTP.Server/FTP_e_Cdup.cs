using System;

namespace LumiSoft.Net.FTP.Server;

public class FTP_e_Cdup : EventArgs
{
	private FTP_t_ReplyLine[] m_pReplyLines;

	public FTP_t_ReplyLine[] Response
	{
		get
		{
			return m_pReplyLines;
		}
		set
		{
			m_pReplyLines = value;
		}
	}
}
