using System;

namespace LumiSoft.Net.FTP.Server;

public class FTP_e_GetFileSize : EventArgs
{
	private string m_FileName;

	private long m_FileSize;

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

	public string FileName => m_FileName;

	public long FileSize
	{
		get
		{
			return m_FileSize;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("Property 'FileSize' value must be >= 0.", "FileSize");
			}
			m_FileSize = value;
		}
	}

	public FTP_e_GetFileSize(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		m_FileName = fileName;
	}
}
