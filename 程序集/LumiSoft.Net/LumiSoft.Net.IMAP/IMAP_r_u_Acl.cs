using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Acl : IMAP_r_u
{
	private string m_FolderName = "";

	private IMAP_Acl_Entry[] m_pEntries;

	public string FolderName => m_FolderName;

	public IMAP_Acl_Entry[] Entires => m_pEntries;

	public IMAP_r_u_Acl(string folderName, IMAP_Acl_Entry[] entries)
	{
		if (folderName == null)
		{
			throw new ArgumentNullException("folderName");
		}
		if (folderName == string.Empty)
		{
			throw new ArgumentException("Argument 'folderName' value must be specified.", "folderName");
		}
		if (entries == null)
		{
			throw new ArgumentNullException("entries");
		}
		m_FolderName = folderName;
		m_pEntries = entries;
	}

	public static IMAP_r_u_Acl Parse(string aclResponse)
	{
		if (aclResponse == null)
		{
			throw new ArgumentNullException("aclResponse");
		}
		StringReader stringReader = new StringReader(aclResponse);
		stringReader.ReadWord();
		stringReader.ReadWord();
		string folderName = TextUtils.UnQuoteString(IMAP_Utils.Decode_IMAP_UTF7_String(stringReader.ReadWord()));
		string[] array = stringReader.ReadToEnd().Split(' ');
		List<IMAP_Acl_Entry> list = new List<IMAP_Acl_Entry>();
		for (int i = 0; i < array.Length; i += 2)
		{
			list.Add(new IMAP_Acl_Entry(array[i], array[i + 1]));
		}
		return new IMAP_r_u_Acl(folderName, list.ToArray());
	}

	public override string ToString()
	{
		return ToString(IMAP_Mailbox_Encoding.None);
	}

	public override string ToString(IMAP_Mailbox_Encoding encoding)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* ACL ");
		stringBuilder.Append(IMAP_Utils.EncodeMailbox(m_FolderName, encoding));
		IMAP_Acl_Entry[] pEntries = m_pEntries;
		foreach (IMAP_Acl_Entry iMAP_Acl_Entry in pEntries)
		{
			stringBuilder.Append(" \"" + iMAP_Acl_Entry.Identifier + "\" \"" + iMAP_Acl_Entry.Rights + "\"");
		}
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
