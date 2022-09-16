using System;

namespace LumiSoft.Net.FTP.Server;

public class FTP_e_Rnto : EventArgs
{
	private FTP_t_ReplyLine[] m_pReplyLines;

	private string m_SourcePath;

	private string m_TargetPath;

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

	public string SourcePath => m_SourcePath;

	public string TargetPath => m_TargetPath;

	public FTP_e_Rnto(string sourcePath, string targetPath)
	{
		if (sourcePath == null)
		{
			throw new ArgumentNullException("sourcePath");
		}
		if (targetPath == null)
		{
			throw new ArgumentNullException("targetPath");
		}
		m_SourcePath = sourcePath;
		m_TargetPath = targetPath;
	}
}
