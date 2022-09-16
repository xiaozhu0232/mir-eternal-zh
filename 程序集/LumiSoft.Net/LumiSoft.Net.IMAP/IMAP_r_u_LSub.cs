using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_LSub : IMAP_r_u
{
	private string m_FolderName = "";

	private char m_Delimiter = '/';

	private string[] m_pFolderAttributes = new string[0];

	public string FolderName => m_FolderName;

	public char HierarchyDelimiter => m_Delimiter;

	public string[] FolderAttributes => m_pFolderAttributes;

	public IMAP_r_u_LSub(string folder, char delimiter, string[] attributes)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		m_FolderName = folder;
		m_Delimiter = delimiter;
		if (attributes != null)
		{
			m_pFolderAttributes = attributes;
		}
	}

	public static IMAP_r_u_LSub Parse(string lSubResponse)
	{
		if (lSubResponse == null)
		{
			throw new ArgumentNullException("lSubResponse");
		}
		StringReader stringReader = new StringReader(lSubResponse);
		stringReader.ReadWord();
		stringReader.ReadWord();
		string text = stringReader.ReadParenthesized();
		return new IMAP_r_u_LSub(delimiter: stringReader.ReadWord()[0], folder: TextUtils.UnQuoteString(IMAP_Utils.DecodeMailbox(stringReader.ReadToEnd().Trim())), attributes: (text == string.Empty) ? new string[0] : text.Split(' '));
	}

	public override string ToString()
	{
		return ToString(IMAP_Mailbox_Encoding.None);
	}

	public override string ToString(IMAP_Mailbox_Encoding encoding)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* LSUB (");
		if (m_pFolderAttributes != null)
		{
			for (int i = 0; i < m_pFolderAttributes.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(m_pFolderAttributes[i]);
			}
		}
		stringBuilder.Append(") ");
		stringBuilder.Append("\"" + m_Delimiter + "\" ");
		stringBuilder.Append(IMAP_Utils.EncodeMailbox(m_FolderName, encoding));
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
