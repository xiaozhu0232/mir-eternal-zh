using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_ListRights : IMAP_r_u
{
	private string m_FolderName = "";

	private string m_Identifier = "";

	private string m_RequiredRights;

	private string m_OptionalRights;

	public string FolderName => m_FolderName;

	public string Identifier => m_Identifier;

	public string RequiredRights => m_RequiredRights;

	public string OptionalRights => m_OptionalRights;

	public IMAP_r_u_ListRights(string folder, string identifier, string requiredRights, string optionalRights)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' name must be specified.", "folder");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (identifier == string.Empty)
		{
			throw new ArgumentException("Argument 'identifier' name must be specified.", "identifier");
		}
		m_FolderName = folder;
		m_Identifier = identifier;
		m_RequiredRights = ((requiredRights == string.Empty) ? null : requiredRights);
		m_OptionalRights = ((optionalRights == string.Empty) ? null : optionalRights);
	}

	public static IMAP_r_u_ListRights Parse(string listRightsResponse)
	{
		if (listRightsResponse == null)
		{
			throw new ArgumentNullException("listRightsResponse");
		}
		StringReader stringReader = new StringReader(listRightsResponse);
		stringReader.ReadWord();
		stringReader.ReadWord();
		string folder = IMAP_Utils.Decode_IMAP_UTF7_String(stringReader.ReadWord(unQuote: true));
		string identifier = stringReader.ReadWord(unQuote: true);
		string requiredRights = stringReader.ReadWord(unQuote: true);
		string optionalRights = stringReader.ReadWord(unQuote: true);
		return new IMAP_r_u_ListRights(folder, identifier, requiredRights, optionalRights);
	}

	public override string ToString()
	{
		return ToString(IMAP_Mailbox_Encoding.None);
	}

	public override string ToString(IMAP_Mailbox_Encoding encoding)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* LISTRIGHTS " + IMAP_Utils.EncodeMailbox(m_FolderName, encoding) + " \"" + m_RequiredRights + "\" " + m_OptionalRights + "\r\n");
		return stringBuilder.ToString();
	}
}
