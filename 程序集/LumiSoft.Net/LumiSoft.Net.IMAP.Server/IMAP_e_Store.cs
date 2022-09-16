using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Store : EventArgs
{
	private IMAP_r_ServerStatus m_pResponse;

	private string m_Folder;

	private IMAP_MessageInfo m_pMsgInfo;

	private IMAP_Flags_SetType m_SetType = IMAP_Flags_SetType.Replace;

	private string[] m_pFlags;

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

	public string Folder => m_Folder;

	public IMAP_MessageInfo MessageInfo => m_pMsgInfo;

	public IMAP_Flags_SetType FlagsSetType => m_SetType;

	public string[] Flags => m_pFlags;

	internal IMAP_e_Store(string folder, IMAP_MessageInfo msgInfo, IMAP_Flags_SetType flagsSetType, string[] flags, IMAP_r_ServerStatus response)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (msgInfo == null)
		{
			throw new ArgumentNullException("msgInfo");
		}
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		m_pResponse = response;
		m_Folder = folder;
		m_pMsgInfo = msgInfo;
		m_SetType = flagsSetType;
		m_pFlags = flags;
	}
}
