using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_QuotaRoot : IMAP_r_u
{
	private string m_FolderName = "";

	private string[] m_QuotaRoots;

	public string FolderName => m_FolderName;

	public string[] QuotaRoots => m_QuotaRoots;

	public IMAP_r_u_QuotaRoot(string folder, string[] quotaRoots)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' name must be specified.", "folder");
		}
		if (quotaRoots == null)
		{
			throw new ArgumentNullException("quotaRoots");
		}
		m_FolderName = folder;
		m_QuotaRoots = quotaRoots;
	}

	public static IMAP_r_u_QuotaRoot Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		StringReader stringReader = new StringReader(response);
		stringReader.ReadWord();
		stringReader.ReadWord();
		string folder = TextUtils.UnQuoteString(IMAP_Utils.Decode_IMAP_UTF7_String(stringReader.ReadWord()));
		List<string> list = new List<string>();
		while (stringReader.Available > 0)
		{
			string text = stringReader.ReadWord();
			if (text == null)
			{
				break;
			}
			list.Add(text);
		}
		return new IMAP_r_u_QuotaRoot(folder, list.ToArray());
	}

	public override string ToString()
	{
		return ToString(IMAP_Mailbox_Encoding.None);
	}

	public override string ToString(IMAP_Mailbox_Encoding encoding)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* QUOTAROOT " + IMAP_Utils.EncodeMailbox(m_FolderName, encoding));
		string[] quotaRoots = m_QuotaRoots;
		foreach (string text in quotaRoots)
		{
			stringBuilder.Append(" \"" + text + "\"");
		}
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
