using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Expunge : EventArgs
{
	private IMAP_r_ServerStatus m_pResponse;

	private string m_Folder;

	private IMAP_MessageInfo m_pMsgInfo;

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

	public string Folder => m_Folder;

	public IMAP_MessageInfo MessageInfo => m_pMsgInfo;

	internal IMAP_e_Expunge(string folder, IMAP_MessageInfo msgInfo, IMAP_r_ServerStatus response)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (msgInfo == null)
		{
			throw new ArgumentNullException("msgInfo");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pResponse = response;
		m_Folder = folder;
		m_pMsgInfo = msgInfo;
	}
}
