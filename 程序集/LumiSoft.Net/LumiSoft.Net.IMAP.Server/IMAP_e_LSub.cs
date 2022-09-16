using System;
using System.Collections.Generic;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_LSub : EventArgs
{
	private string m_FolderReferenceName;

	private string m_FolderFilter;

	private List<IMAP_r_u_LSub> m_pFolders;

	public string FolderReferenceName => m_FolderReferenceName;

	public string FolderFilter => m_FolderFilter;

	public List<IMAP_r_u_LSub> Folders => m_pFolders;

	internal IMAP_e_LSub(string referenceName, string folderFilter)
	{
		m_FolderReferenceName = referenceName;
		m_FolderFilter = folderFilter;
		m_pFolders = new List<IMAP_r_u_LSub>();
	}
}
