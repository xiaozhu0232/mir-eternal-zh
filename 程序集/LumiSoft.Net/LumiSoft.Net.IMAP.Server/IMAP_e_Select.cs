using System;
using System.Collections.Generic;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Select : EventArgs
{
	private string m_CmdTag;

	private IMAP_r_ServerStatus m_pResponse;

	private string m_Folder;

	private bool m_IsReadOnly;

	private int m_FolderUID;

	private List<string> m_pFlags;

	private List<string> m_pPermanentFlags;

	public string CmdTag => m_CmdTag;

	public IMAP_r_ServerStatus ErrorResponse
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

	public string Folder => m_Folder;

	public bool IsReadOnly
	{
		get
		{
			return m_IsReadOnly;
		}
		set
		{
			m_IsReadOnly = value;
		}
	}

	public int FolderUID
	{
		get
		{
			return m_FolderUID;
		}
		set
		{
			m_FolderUID = value;
		}
	}

	public List<string> Flags => m_pFlags;

	public List<string> PermanentFlags => m_pPermanentFlags;

	internal IMAP_e_Select(string cmdTag, string folder)
	{
		if (cmdTag == null)
		{
			throw new ArgumentNullException("cmdTag");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		m_CmdTag = cmdTag;
		m_Folder = folder;
		m_pFlags = new List<string>();
		m_pPermanentFlags = new List<string>();
		m_pFlags.AddRange(new string[5] { "\\Answered", "\\Flagged", "\\Deleted", "\\Seen", "\\Draft" });
		m_pPermanentFlags.AddRange(new string[5] { "\\Answered", "\\Flagged", "\\Deleted", "\\Seen", "\\Draft" });
	}
}
