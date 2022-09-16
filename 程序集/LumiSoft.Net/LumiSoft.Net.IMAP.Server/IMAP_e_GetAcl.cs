using System;
using System.Collections.Generic;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_GetAcl : EventArgs
{
	private List<IMAP_r_u_Acl> m_pAclResponses;

	private IMAP_r_ServerStatus m_pResponse;

	private string m_Folder;

	public List<IMAP_r_u_Acl> AclResponses => m_pAclResponses;

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

	internal IMAP_e_GetAcl(string folder, IMAP_r_ServerStatus response)
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
		m_pAclResponses = new List<IMAP_r_u_Acl>();
	}
}
