using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Started : EventArgs
{
	private IMAP_r_u_ServerStatus m_pResponse;

	public IMAP_r_u_ServerStatus Response
	{
		get
		{
			return m_pResponse;
		}
		set
		{
			m_pResponse = value;
		}
	}

	internal IMAP_e_Started(IMAP_r_u_ServerStatus response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pResponse = response;
	}
}
