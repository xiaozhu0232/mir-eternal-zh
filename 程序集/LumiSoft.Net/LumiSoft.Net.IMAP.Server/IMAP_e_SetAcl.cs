using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_SetAcl : EventArgs
{
	private IMAP_r_ServerStatus m_pResponse;

	private string m_Folder;

	private string m_Identifier;

	private IMAP_Flags_SetType m_SetType = IMAP_Flags_SetType.Replace;

	private string m_Rights;

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

	public string Identifier => m_Identifier;

	public IMAP_Flags_SetType FlagsSetType => m_SetType;

	public string Rights => m_Rights;

	internal IMAP_e_SetAcl(string folder, string identifier, IMAP_Flags_SetType flagsSetType, string rights, IMAP_r_ServerStatus response)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (rights == null)
		{
			throw new ArgumentNullException("rights");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pResponse = response;
		m_Folder = folder;
		m_Identifier = identifier;
		m_SetType = flagsSetType;
		m_Rights = rights;
	}
}
