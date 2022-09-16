using System;

namespace LumiSoft.Net.FTP.Server;

public class FTP_e_Dele : EventArgs
{
	private FTP_t_ReplyLine[] m_pReplyLines;

	private string m_FileName;

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

	public string FileName => m_FileName;

	public FTP_e_Dele(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		m_FileName = fileName;
	}
}
