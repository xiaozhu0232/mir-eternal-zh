using System;
using System.IO;

namespace LumiSoft.Net.FTP.Server;

public class FTP_e_Stor : EventArgs
{
	private string m_FileName;

	private FTP_t_ReplyLine[] m_pReplyLines;

	private Stream m_pFileStream;

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

	public Stream FileStream
	{
		get
		{
			return m_pFileStream;
		}
		set
		{
			m_pFileStream = value;
		}
	}

	public FTP_e_Stor(string file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file == string.Empty)
		{
			throw new ArgumentException("Argument 'file' name must be specified.", "file");
		}
		m_FileName = file;
	}
}
