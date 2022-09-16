using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_MyRights : EventArgs
{
	private IMAP_r_u_MyRights m_pMyRightsResponse;

	private IMAP_r_ServerStatus m_pResponse;

	private string m_Folder;

	public IMAP_r_u_MyRights MyRightsResponse
	{
		get
		{
			return m_pMyRightsResponse;
		}
		set
		{
			m_pMyRightsResponse = value;
		}
	}

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

	internal IMAP_e_MyRights(string folder, IMAP_r_ServerStatus response)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_Folder = folder;
		m_pResponse = response;
	}
}
