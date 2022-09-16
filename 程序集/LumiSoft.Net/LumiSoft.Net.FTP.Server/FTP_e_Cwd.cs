using System;

namespace LumiSoft.Net.FTP.Server;

public class FTP_e_Cwd : EventArgs
{
	private FTP_t_ReplyLine[] m_pReplyLines;

	private string m_DirName;

	public FTP_t_ReplyLine[] Response
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

	public string DirName => m_DirName;

	public FTP_e_Cwd(string dirName)
	{
		if (dirName == null)
		{
			throw new ArgumentNullException("dirName");
		}
		m_DirName = dirName;
	}
}
