using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Folder : EventArgs
{
	private IMAP_r_ServerStatus m_pResponse;

	private string m_CmdTag;

	private string m_Folder = "";

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

	public string Folder => m_Folder;

	internal IMAP_e_Folder(string cmdTag, string folder, IMAP_r_ServerStatus response)
	{
		if (cmdTag == null)
		{
			throw new ArgumentNullException("cmdTag");
		}
		if (cmdTag == string.Empty)
		{
			throw new ArgumentException("Argument 'cmdTag' value must be specified.", "cmdTag");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pResponse = response;
		m_CmdTag = cmdTag;
		m_Folder = folder;
	}
}
