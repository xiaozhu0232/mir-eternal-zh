using System;
using System.Collections.Generic;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_MessagesInfo : EventArgs
{
	private string m_Folder;

	private List<IMAP_MessageInfo> m_pMessages;

	public string Folder => m_Folder;

	public List<IMAP_MessageInfo> MessagesInfo => m_pMessages;

	internal int Exists => m_pMessages.Count;

	internal int Recent
	{
		get
		{
			int num = 0;
			foreach (IMAP_MessageInfo pMessage in m_pMessages)
			{
				string[] flags = pMessage.Flags;
				for (int i = 0; i < flags.Length; i++)
				{
					if (string.Equals(flags[i], "Recent", StringComparison.InvariantCultureIgnoreCase))
					{
						num++;
						break;
					}
				}
			}
			return num;
		}
	}

	internal int FirstUnseen
	{
		get
		{
			for (int i = 0; i < m_pMessages.Count; i++)
			{
				if (!m_pMessages[i].ContainsFlag("Seen"))
				{
					return i + 1;
				}
			}
			return -1;
		}
	}

	internal int Unseen
	{
		get
		{
			int num = m_pMessages.Count;
			foreach (IMAP_MessageInfo pMessage in m_pMessages)
			{
				string[] flags = pMessage.Flags;
				for (int i = 0; i < flags.Length; i++)
				{
					if (string.Equals(flags[i], "Seen", StringComparison.InvariantCultureIgnoreCase))
					{
						num--;
						break;
					}
				}
			}
			return num;
		}
	}

	internal long UidNext
	{
		get
		{
			long num = 0L;
			foreach (IMAP_MessageInfo pMessage in m_pMessages)
			{
				if (pMessage.UID > num)
				{
					num = pMessage.UID;
				}
			}
			return num + 1;
		}
	}

	internal IMAP_e_MessagesInfo(string folder)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		m_Folder = folder;
		m_pMessages = new List<IMAP_MessageInfo>();
	}
}
