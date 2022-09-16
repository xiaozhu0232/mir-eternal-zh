using System;
using System.Collections.Generic;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_GetQuotaRoot : EventArgs
{
	private List<IMAP_r_u_QuotaRoot> m_pQuotaRootResponses;

	private List<IMAP_r_u_Quota> m_pQuotaResponses;

	private IMAP_r_ServerStatus m_pResponse;

	private string m_Folder;

	public List<IMAP_r_u_QuotaRoot> QuotaRootResponses => m_pQuotaRootResponses;

	public List<IMAP_r_u_Quota> QuotaResponses => m_pQuotaResponses;

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

	internal IMAP_e_GetQuotaRoot(string folder, IMAP_r_ServerStatus response)
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
		m_pQuotaRootResponses = new List<IMAP_r_u_QuotaRoot>();
		m_pQuotaResponses = new List<IMAP_r_u_Quota>();
	}
}
