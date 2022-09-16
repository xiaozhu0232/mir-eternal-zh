using System;
using System.Collections.Generic;

namespace LumiSoft.Net.FTP.Server;

public class FTP_e_GetDirListing : EventArgs
{
	private string m_Path;

	private List<FTP_ListItem> m_pItems;

	private FTP_t_ReplyLine[] m_pReplyLines;

	public FTP_t_ReplyLine[] Error
	{
		get
		{
			return m_pReplyLines;
		}
		set
		{
			m_pReplyLines = value;
		}
	}

	public string Path => m_Path;

	public List<FTP_ListItem> Items => m_pItems;

	public FTP_e_GetDirListing(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		m_Path = path;
		m_pItems = new List<FTP_ListItem>();
	}
}
