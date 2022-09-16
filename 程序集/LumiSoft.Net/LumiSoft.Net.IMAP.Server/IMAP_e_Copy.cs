using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Copy : EventArgs
{
	private IMAP_r_ServerStatus m_pResponse;

	private string m_SourceFolder;

	private string m_TargetFolder;

	private IMAP_MessageInfo[] m_pMessagesInfo;

	public IMAP_r_ServerStatus Response
	{
		get
		{
			return m_pResponse;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_pResponse = value;
		}
	}

	public string SourceFolder => m_SourceFolder;

	public string TargetFolder => m_TargetFolder;

	public IMAP_MessageInfo[] MessagesInfo => m_pMessagesInfo;

	internal IMAP_e_Copy(string sourceFolder, string targetFolder, IMAP_MessageInfo[] messagesInfo, IMAP_r_ServerStatus response)
	{
		if (sourceFolder == null)
		{
			throw new ArgumentNullException("sourceFolder");
		}
		if (targetFolder == null)
		{
			throw new ArgumentNullException("targetFolder");
		}
		if (messagesInfo == null)
		{
			throw new ArgumentNullException("messagesInfo");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pResponse = response;
		m_SourceFolder = sourceFolder;
		m_TargetFolder = targetFolder;
		m_pMessagesInfo = messagesInfo;
	}
}
