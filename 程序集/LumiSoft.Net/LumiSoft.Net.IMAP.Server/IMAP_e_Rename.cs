using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Rename : EventArgs
{
	private IMAP_r_ServerStatus m_pResponse;

	private string m_CmdTag;

	private string m_CurrentFolder;

	private string m_NewFolder;

	public IMAP_r_ServerStatus Response
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

	public string CmdTag => m_CmdTag;

	public string CurrentFolder => m_CurrentFolder;

	public string NewFolder => m_NewFolder;

	internal IMAP_e_Rename(string cmdTag, string currentFolder, string newFolder)
	{
		if (cmdTag == null)
		{
			throw new ArgumentNullException("cmdTag");
		}
		if (currentFolder == null)
		{
			throw new ArgumentNullException("currentFolder");
		}
		if (newFolder == null)
		{
			throw new ArgumentNullException("newFolder");
		}
		m_CmdTag = cmdTag;
		m_CurrentFolder = currentFolder;
		m_NewFolder = newFolder;
	}
}
