using System;
using System.Collections.Generic;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_GetQuota : EventArgs
{
	private List<IMAP_r_u_Quota> m_pQuotaResponses;

	private IMAP_r_ServerStatus m_pResponse;

	private string m_QuotaRoot;

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

	public string QuotaRoot => m_QuotaRoot;

	internal IMAP_e_GetQuota(string quotaRoot, IMAP_r_ServerStatus response)
	{
		if (quotaRoot == null)
		{
			throw new ArgumentNullException("quotaRoot");
		}
		m_QuotaRoot = quotaRoot;
		m_pResponse = response;
		m_pQuotaResponses = new List<IMAP_r_u_Quota>();
	}
}
